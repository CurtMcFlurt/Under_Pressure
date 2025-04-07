using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public struct HeatMapValues
{
    public float food;
    public float sound;
    public float safety;
    public HeatMapValues(float food=1,float sound=1,float safety=1)
    {
        this.food = food;
        this.sound = sound;
        this.safety = safety;
    }
}
[ExecuteAlways]
public class Hexagon_HeatmapManager : MonoBehaviour
{
    public HexagonalWeight hexWeighter;
    public List<WeightChangers> staticWeightChangers = new List<WeightChangers>();

  
    
    public HeatMapValues heatcooling;

    void OnEnable()
    {
        if (hexWeighter == null)
            hexWeighter = GetComponent<HexagonalWeight>();

        staticWeightChangers.Clear();
        staticWeightChangers.AddRange(Resources.FindObjectsOfTypeAll<WeightChangers>());
    }

    void FixedUpdate()
    {
        var hexDict = hexWeighter.walkableHexagons;

        // Apply active changers
        foreach (var changer in staticWeightChangers)
        {
            if (changer.isActiveAndEnabled)
            {
                var nearest = HexMath.NearestHex(changer.transform.position, hexDict.Values.ToList(), hexWeighter.cellSize);
                ApplyHeatChange(nearest, changer.range, changer.myHeat, changer.falloff);
            }
        }

        // Cool down all heatmap values toward 1
        var keys = hexDict.Keys.ToList(); // Copy keys to avoid modifying collection while iterating
        foreach (var key in keys)
        {
            HexCell hex = hexDict[key];

            hex.weight.food = Mathf.Lerp(hex.weight.food, 1f, heatcooling.food * Time.fixedDeltaTime);
            hex.weight.sound = Mathf.Lerp(hex.weight.sound, 1f, heatcooling.sound * Time.fixedDeltaTime);
            hex.weight.safety = Mathf.Lerp(hex.weight.safety, 1f, heatcooling.safety * Time.fixedDeltaTime);

            hexDict[key] = hex; // Store modified back
        }
    }

    public void ApplyHeatChange(HexCell centerHex, int range, HeatMapValues values, float falloff)
    {
        var hexDict = hexWeighter.walkableHexagons;

        foreach (var kvp in hexDict.ToList())
        {
            Vector3 key = kvp.Key;
            HexCell hex = kvp.Value;

            int distance = HexMath.HexDistance(centerHex.hexCoords, hex.hexCoords);

            if (distance <= range)
            {
                float modifier = Mathf.Max(0, 1 - (distance * falloff));
                hex.weight = HexMath.AddHeatMapValues(hex.weight, values, modifier);

                hexDict[key] = hex;
            }
        }
    }
}