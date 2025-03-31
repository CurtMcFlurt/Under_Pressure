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
      #define MAX_ADDITIONAL_LIGHTS 8
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
    float3 viewDir = worldPos - _WorldSpaceCameraPos;
    float viewLength = length(viewDir);
    float3 rayDir = normalize(viewDir);
    float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;

    float distLimit = min(viewLength, _MaxDistance);
    float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(unity_DeltaTime.x, HALF_EPS))) * _NoiseOffset;

    float transmittance = 1.0;
    float4 fogCol = _Color;

    while (distTravelled < distLimit)
    {
        float3 rayPos = entryPoint + rayDir * distTravelled;
        float density = 0;
        if(distTravelled>_visionMinDistance)
        {
             density=get_density(rayPos);

            if(distTravelled<_visionMaxDistance)
            {
                density -= (1-(distTravelled/(_visionMaxDistance*_fogFalloff)));
            } 
           
        }else density=0;
        
        float lightDensityReduction = 0.0; // Reset per-step

        // 💡 Accumulate additional lights
        for (int i = 0; i < _totalLights; ++i)
        {
            float3 lightDir = normalize(_AdditionalLightPositions[i].xyz - rayPos);
            float lightDistance = length(_AdditionalLightPositions[i].xyz - rayPos);

            // ✅ **Light Range Attenuation**
            if (lightDistance > _AdditionalLightRanges[i])
                continue;  // Skip if fragment is outside the light range

            float attenuation = 1.0 / (1.0 + lightDistance * lightDistance * 0.1); // Inverse-square law

            // ✅ **Spotlight Handling**
            float spotlightFactor = 1.0; // Default to 1 for point lights
            if (_AdditionalLightAngles[i] > 0.0)  // If the light is a spotlight
            {
                float spotCosine = dot(-lightDir, normalize(_AdditionalLightDirections[i])); // Compare angle
                if (spotCosine < _AdditionalLightAngles[i])
                    continue; // Skip if outside the cone

                spotlightFactor = smoothstep(_AdditionalLightAngles[i], _AdditionalLightAngles[i] + 0.5, spotCosine);
            }

            // Accumulate light contribution
            lightDensityReduction += (attenuation * spotlightFactor * _AdditionalLightIntensities[i] * _LightPower);

            // Add light color to fog
            fogCol.rgb += (_AdditionalLightColors[i].rgb * lightDensityReduction * _LightContribution.rgb);
        }

        // Apply accumulated light reduction to the density
        density = max(0, density - lightDensityReduction);

        if (density > 0)
        {
            Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos));

            fogCol.rgb += mainLight.color.rgb * _LightContribution.rgb *
                          henyey_greenstein(dot(rayDir, mainLight.direction), _LightScattering) *
                          density * mainLight.shadowAttenuation * _StepSize;

            density -= max(- distTravelled, 0);
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
