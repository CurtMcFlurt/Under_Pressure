Shader "Unlit/VolumetricFog"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
       _MaxDistance("Max distance",float) = 100
       _StepSize("Step size",Range(0.1,20)) = 1
       _DensityMultiplier("Density Multiplier", Range(0,10)) = 1       
       _NoiseOffset("noise offset",float) = 0

       _FogNoise("Fog noise", 3D)="white"{}
       _NoiseTiling("Noise tiling",float)=1
       _DensityThreshold("Density threshold",Range(0,1))=0.1

       [HDR] _LightContribution("Light contribution",Color)=(1,1,1,1)
       _LightScattering("Light scattering",Range(0,1))=0.2
       _visionMinDistance("Vision min distance",float)=0
       _visionMaxDistance("vision max distance",float)=50
       _fogFalloff("fog fallof",float)=1
       _MaxClearFogDistance("light showing their objects",float)=50
       _LightPower("added light Power",float)=100
       _totalLights("script value Lights",int)=0
        
    }
    SubShader
    {
       Tags{"RenderType"="Transparent""RenderPipeline" ="UniversalPipeline" "UniversalMaterialType" = "Lit"   "LightMode" = "UniversalForward"}

         
       Pass{
           HLSLPROGRAM
           #pragma vertex Vert
           #pragma fragment frag
           #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
             #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
           #include "Packages/com.unity.render-pipelines.core/RunTime/Utilities/Blit.hlsl"           
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
      #define MAX_ADDITIONAL_LIGHTS 12
float4 _AdditionalLightPositions[MAX_ADDITIONAL_LIGHTS];
float4 _AdditionalLightColors[MAX_ADDITIONAL_LIGHTS];
float3 _AdditionalLightDirections[MAX_ADDITIONAL_LIGHTS];  // Spotlights need a direction
float _AdditionalLightIntensities[MAX_ADDITIONAL_LIGHTS];
float _AdditionalLightRanges[MAX_ADDITIONAL_LIGHTS];       // Light range
float _AdditionalLightAngles[MAX_ADDITIONAL_LIGHTS];       // Spotlight angle (cosine of inner angle)

float4 _Color;
float _MaxDistance;
float _DensityMultiplier;
float _StepSize;
float _NoiseOffset;
TEXTURE3D(_FogNoise);
float _DensityThreshold;
float _NoiseTiling;
float4 _LightContribution;
float _LightScattering;
float _visionMinDistance;
float _MaxClearFogDistance; // How far away lights can influence fog (e.g. 50)
float _visionMaxDistance;
float _fogFalloff;
float _LightPower;
int _totalLights;


float henyey_greenstein(float angle, float scattering)
{
    return (1.0 - angle * angle) / (4.0 * PI * pow(1.0 + scattering * scattering - (2.0 * scattering) * angle, 1.5f));
}

float get_density(float3 worldPos)
{
    float4 noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, worldPos * 0.01 * _NoiseTiling, 0);
    float density = dot(noise, noise);
    density = saturate(density - _DensityThreshold) * _DensityMultiplier;
    return density;
}
half4 frag(Varyings IN) : SV_Target
{
    float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
    float depth = SampleSceneDepth(IN.texcoord);
    float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);
    float3 entryPoint = _WorldSpaceCameraPos;
    float3 viewDir = worldPos - entryPoint;
    float viewLength = length(viewDir);
    float3 rayDir = normalize(viewDir);
    float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;

    float distLimit = min(viewLength, _MaxDistance);
    float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(unity_DeltaTime.x, HALF_EPS))) * _NoiseOffset;

    float transmittance = 1.0;
    float4 fogCol = _Color;

    int maxLights = min(_totalLights, MAX_ADDITIONAL_LIGHTS);

    // 🔁 March through fog volume
    while (distTravelled < distLimit)
    {
        float3 rayPos = entryPoint + rayDir * distTravelled;
        float density = 0;

        if (distTravelled > _visionMinDistance)
        {
            density = get_density(rayPos);

            if (distTravelled < _visionMaxDistance)
            {
                density -= (1 - (distTravelled / (_visionMaxDistance * _fogFalloff)));
            }
        }

        float lightDensityReduction = 0.0;
        float3 lightCol = 0.0;

        // 🔦 Spotlight evaluation
        [unroll(MAX_ADDITIONAL_LIGHTS)]
        for (int i = 0; i < MAX_ADDITIONAL_LIGHTS; ++i)
        {
            if (i >= maxLights) break;

            float3 lightPos = _AdditionalLightPositions[i].xyz;
            float lightRange = _AdditionalLightRanges[i];
            float3 lightDir = normalize(_AdditionalLightDirections[i]);
            float3 toSample = rayPos - lightPos;
            float distToSample = length(toSample);

            if (distToSample > lightRange)
                continue;

            float3 toSampleDir = toSample / max(distToSample, 0.001);
            float spotCos = dot(toSampleDir, lightDir);

            float cosCutoff = _AdditionalLightAngles[i];
            if (cosCutoff > 0.0 && spotCos < cosCutoff)
                continue;

            // Intensity falloffs
            float attenuation = 1.0 / (1.0 + distToSample * distToSample * 0.05);
            float coneFalloff = (cosCutoff > 0.0)
                ? smoothstep(cosCutoff, cosCutoff + 0.1, spotCos)
                : 1.0;
            float edgeFalloff = saturate(1.0 - (distToSample / lightRange));
            float distanceFade = saturate(1.0 - (distTravelled / _MaxClearFogDistance));

            float lightEffect = attenuation * coneFalloff * edgeFalloff *
                                _AdditionalLightIntensities[i] * _LightPower;

            // 🌫️ Reduce fog density only *within* cone
            lightDensityReduction += lightEffect *.25f;

            // 🌈 Add light color with smooth falloff (even past fog clear)
            lightCol += _AdditionalLightColors[i].rgb * lightEffect *
                        _LightContribution.rgb * distanceFade;
        }

        density = max(0, density - lightDensityReduction);

      // Always add light color to fog, even if density is zero
        fogCol.rgb += (lightCol * _StepSize);

        if (density > 0)
        {
            Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos));

            fogCol.rgb += mainLight.color.rgb * _LightContribution.rgb *
                          henyey_greenstein(dot(rayDir, mainLight.direction), _LightScattering) *
                          density * mainLight.shadowAttenuation * _StepSize;

            transmittance *= exp(-density * _StepSize);
        }

        distTravelled += _StepSize;
    }

    return lerp(col, fogCol, 1.0 - saturate(transmittance));
}
           ENDHLSL
       }
    }
}
