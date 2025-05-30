using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct HeatMapValues
{
    public float food;
    public float sound;
    public float safety;
    public float soundMemory;

    public HeatMapValues(float food = 1, float sound = 1, float safety = 1)
    {
        this.food = food;
        this.sound = sound;
        this.safety = safety;
        this.soundMemory = 0;
    }
}

public class Hexagon_HeatmapManager : NetworkBehaviour
{
    public HexagonalWeight hexWeighter;
    public List<WeightChangers> WeightChangers = new List<WeightChangers>();

    public HeatMapValues heatcooling;

    private readonly List<Vector3> _hexKeysBuffer = new();
    private readonly List<KeyValuePair<Vector3, HexCell>> _hexPairsBuffer = new();

    void OnEnable()
    {
        if (hexWeighter == null)
            hexWeighter = GetComponent<HexagonalWeight>();

        WeightChangers.Clear();
        WeightChangers.AddRange(Resources.FindObjectsOfTypeAll<WeightChangers>());

        hexWeighter.weightChangers = WeightChangers;
    }

    void FixedUpdate()
    {
        var hexDict = hexWeighter.fullMapHexagons;
        var nearesthex = hexWeighter.walkableHexagons;

        // Apply active changers
        foreach (var changer in WeightChangers)
        {
            if (changer != null && changer.isActiveAndEnabled)
            {
                var nearest = HexMath.NearestHex(changer.transform.position, nearesthex.Values.ToList(), hexWeighter.cellSize);
                changer.myHex = nearest;

                if (changer.OneOff)
                {
                    changer.enabled = false;
                }

                ApplyHeatChange(nearest, changer.range, changer.myHeat, changer.falloff);
            }
        }

        // Cool down all heatmap values toward 1
        _hexKeysBuffer.Clear();
        _hexKeysBuffer.AddRange(hexDict.Keys);

        foreach (var key in _hexKeysBuffer)
        {
            HexCell hex = hexDict[key];

            hex.weight.food = Mathf.Lerp(hex.weight.food, 1f, heatcooling.food * Time.fixedDeltaTime);
            hex.weight.sound = Mathf.Lerp(hex.weight.sound, 1f, heatcooling.sound * Time.fixedDeltaTime);
            hex.weight.safety = Mathf.Lerp(hex.weight.safety, 1f, heatcooling.safety * Time.fixedDeltaTime);

            hexDict[key] = hex;
        }
    }

    public void ApplyHeatChange(HexCell centerHex, int range, HeatMapValues values, float falloff)
    {
        var hexDict = hexWeighter.fullMapHexagons;

        _hexPairsBuffer.Clear();
        _hexPairsBuffer.AddRange(hexDict);

        foreach (var kvp in _hexPairsBuffer)
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
