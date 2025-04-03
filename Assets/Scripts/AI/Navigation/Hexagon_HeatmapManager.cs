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
    private Vector3[] cubeDirectionVectors = new Vector3[]
 {
    new Vector3(+1, 0, -1),
    new Vector3(+1, -1, 0),
    new Vector3(0, -1, +1),
    new Vector3(-1, 0, +1),
    new Vector3(-1, +1, 0),
    new Vector3(0, +1, -1)
 };
    public float cooling;
    void OnEnable()
    {
        if (hexWeighter == null)hexWeighter=GetComponent<HexagonalWeight>();
        staticWeightChangers.Clear();
        staticWeightChangers.AddRange(Resources.FindObjectsOfTypeAll<WeightChangers>());
    }

    // Update is called once per frame
    void Update()
    {
     foreach(var changer in staticWeightChangers)
        {
            if (changer.isActiveAndEnabled)
            {
                ApplyHeatChange(HexMath.NearestHex(changer.transform.position,hexWeighter.walkableHexagons,hexWeighter.cellSize), changer.range, changer.myHeat, changer.falloff);
            }
        }
        var hexList = hexWeighter.walkableHexagons;
        for (int i = 0; i < hexList.Count; i++)
        {
            HexCell hex = hexList[i];

            // Adjust heat values towards 1
            hex.weight.food = Mathf.Lerp(hex.weight.food, 1f, cooling * Time.deltaTime);
            hex.weight.sound = Mathf.Lerp(hex.weight.sound, 1f, cooling * Time.deltaTime);
            hex.weight.safety = Mathf.Lerp(hex.weight.safety, 1f, cooling * Time.deltaTime);

            // Assign back since structs are value types
            hexList[i] = hex;
        }
        
    }

    public void ApplyHeatChange(HexCell centerHex, int range, HeatMapValues values, float falloff)
    {
        var hexList = hexWeighter.walkableHexagons; // Convert HashSet to List to allow modification

        for (int i = 0; i < hexList.Count; i++)
        {
            HexCell hex = hexList[i]; // Copy the struct

            int distance = HexMath.HexDistance(centerHex.hexCoords, hex.hexCoords);

            if (distance <= range) // Within range
            {
                float modifier = Mathf.Max(0, 1 - (distance * falloff)); // Reduces influence over distance
                hex.weight = HexMath.AddHeatMapValues(hex.weight, values, modifier); // Modify copy

                hexList[i] = hex; // Store modified struct back
            }
        }

    }

    
}
