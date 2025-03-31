using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class LightManager : MonoBehaviour
{
    public Material fogShader;
    public Light[] lights;
    private const int MAX_LIGHTS = 8;

    private Vector4[] lightPositions = new Vector4[MAX_LIGHTS];
    private Vector4[] lightColors = new Vector4[MAX_LIGHTS];
    private Vector4[] lightDirections = new Vector4[MAX_LIGHTS];
    private float[] lightIntensities = new float[MAX_LIGHTS];
    private float[] lightRanges = new float[MAX_LIGHTS];
    private float[] lightAngles = new float[MAX_LIGHTS];

    [System.Obsolete]
    void Update()
    {
        lights = FindObjectsOfType<Light>();
        Camera cam = Camera.main;
        if (cam == null) return;

        int count = 0;
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional || count >= MAX_LIGHTS)
                continue;

            // 🏆 **New Raycast Check: Is the light EFFECTIVELY visible?**
            Vector3 lightPos = light.transform.position;
            Vector3 camPos = cam.transform.position;
            Vector3 dirToLight = (lightPos - camPos).normalized;
            float distToLight = Vector3.Distance(camPos, lightPos);

            // 🔥 Reduce the raycast distance by the light's radius
            float checkDistance = Mathf.Max(distToLight - light.range, 0.1f); // Ensure a small minimum distance

            if (Physics.Raycast(camPos, dirToLight, checkDistance, LayerMask.GetMask("Default")))
            {
                // 🚫 Skip light if blocked within its effective range
                continue;
            }

            // ✅ Light is visible within its effective range
            lightPositions[count] = lightPos;
            lightColors[count] = light.color;
            lightIntensities[count] = light.intensity;
            lightRanges[count] = light.range;

            if (light.type == LightType.Spot)
            {
                lightDirections[count] = light.transform.forward;
                lightAngles[count] = Mathf.Cos(light.spotAngle * 0.5f * Mathf.Deg2Rad);
            }
            else
            {
                lightDirections[count] = Vector3.zero; // No direction for point lights
                lightAngles[count] = 0; // No angle for point lights
            }

            count++;
        }

        // 🔥 Send data to shader
        if (fogShader)
        {
            fogShader.SetInt("_totalLights", count);
            fogShader.SetVectorArray("_AdditionalLightPositions", lightPositions);
            fogShader.SetVectorArray("_AdditionalLightColors", lightColors);
            fogShader.SetVectorArray("_AdditionalLightDirections", lightDirections);
            fogShader.SetFloatArray("_AdditionalLightIntensities", lightIntensities);
            fogShader.SetFloatArray("_AdditionalLightRanges", lightRanges);
            fogShader.SetFloatArray("_AdditionalLightAngles", lightAngles);
        }
    }
}
