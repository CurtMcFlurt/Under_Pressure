using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class HexMath 
{
    public static HeatMapValues AddHeatMapValues(HeatMapValues original, HeatMapValues added, float modifier)
    {
        return new HeatMapValues(
            original.food + (added.food * modifier),
            original.sound + (added.sound * modifier),
            original.safety + (added.safety * modifier)
        );
    }
  public static Vector3[] cubeDirectionVectors = new Vector3[]
  {
        new Vector3(+1, 0, -1),
        new Vector3(+1, -1, 0),
        new Vector3(0, -1, +1),
        new Vector3(-1, 0, +1),
        new Vector3(-1, +1, 0),
        new Vector3(0, +1, -1)
  };
    public static int HexDistance(Vector3 a, Vector3 b)
    {
        return (Mathf.Abs((int)a.x - (int)b.x) + Mathf.Abs((int)a.y - (int)b.y) + Mathf.Abs((int)a.z - (int)b.z)) / 2;
    }
    public static HexCell NearestHex(Vector3 worldPos,List<HexCell> walkableHexagons,float cellSize)
    {
        HexCell result = walkableHexagons[0];
        foreach (var v in walkableHexagons)
        {
            if ((Axial2World(v,cellSize) - worldPos).magnitude < (Axial2World(result,cellSize) - worldPos).magnitude)
            {
                result = v;
            }
        }

        return result;
    }
    public static void UpdateWeights(ref HexCell memoryCell, HexCell liveCell)
    {
        HeatMapValues w = memoryCell.weight;

        w.food = liveCell.weight.food;
        w.safety = liveCell.weight.safety;
        w.sound = liveCell.weight.sound;

        memoryCell.weight = w;
    }
    public static Vector3 Axial2World(HexCell con,float cellSize)
    {
        Vector3 centre = new Vector3();
        centre.x = cellSize * (1.5f * con.hexCoords.x);
        centre.z = cellSize * ((Mathf.Sqrt(3) / 2) * con.hexCoords.x + Mathf.Sqrt(3) * con.hexCoords.y);
        centre.y = con.height;
        return centre;
    }
}
