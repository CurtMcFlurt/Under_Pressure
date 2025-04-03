using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct HexCell
{
    public Vector3 hexCoords; // QRS Coordinates
    public float height;      // Height Value
    public int stackLevel;    // Stack Position for Multi-level Maps
    public bool isWalkable;   // Walkability Flag
    public HeatMapValues weight;
    public HexCell(Vector3 hexCoords, float height, int stackLevel, bool isWalkable,HeatMapValues weight=new HeatMapValues())
    {
        this.hexCoords = hexCoords;
        this.height = height;
        this.stackLevel = stackLevel;
        this.isWalkable = isWalkable;
        this.weight = weight;
    }
}

[ExecuteAlways]
public class HexagonalWeight : MonoBehaviour
{

    public int range = 5;
    public float cellSize = 1;
    private Vector3[] corners;
    public HexGridScript grid;
    public Color hexcolor = new Color(0.124f, 0.135f, 0.134f);
    private int oldRange;
    private float oldCell;
    public List<HexCell> walkableHexagons = new List<HexCell>();
    public int walkables = 0;
    public LayerMask floorMask;
    private void OnEnable()
    {
        SetUpHexes();
    }

    private void Update()
    {
        Vector3 currentHex = new Vector3();
        for (int i = 0; i < grid.coordinate.Count; i++)
        {
            currentHex = grid.coordinate[i];
         
        }
        if(range != oldRange || cellSize != oldCell)
        {
            SetUpHexes();
        }
        walkables = walkableHexagons.Count;
    }


    private void SetUpHexes()
    {
        grid = new HexGridScript(range, cellSize);
        oldCell = cellSize;
        oldRange = range;
        FindAllAccessibleHexes();
    }
  
    private Vector3 HexCorners(Vector3 pos,float height, int i)
    {
        var angle_deg = 60 * i;
        float angle_rad = Mathf.PI / 180 * angle_deg;
        return new Vector3(pos.x + cellSize * Mathf.Cos(angle_rad), height, pos.z + cellSize * Mathf.Sin(angle_rad));
    }
    private void FindAllAccessibleHexes()
    {
        walkableHexagons.Clear();
        foreach (var hex in grid.hexvalue)
        {
            Vector3 worldPos = grid.Axial2Pixel(hex);
            int stackLevel = 0;
            float maxCheckHeight = 50f; // Maximum height to check for additional levels
            bool foundBase = false;

            while (worldPos.y <= maxCheckHeight)
            {
                if (NavMesh.SamplePosition(worldPos, out var hit, 3f, NavMesh.AllAreas))
                {
                    float height = hit.position.y;
                    HexCell hexCell = new HexCell(hex, height, stackLevel, true);
                    walkableHexagons.Add(hexCell);
                    foundBase = true;
                }

                if (foundBase)
                {
                    if (Physics.Raycast(worldPos, Vector3.up, out RaycastHit levelHit, maxCheckHeight - worldPos.y, floorMask))
                    {
                        worldPos = levelHit.point + Vector3.up * 0.1f; // Move slightly above the detected floor
                        stackLevel++;
                        continue;
                    }
                    else
                    {
                        break; // No more levels found
                    }
                }

                worldPos.y += cellSize; // Move up incrementally until a base is found
            }
        }
    }


    public void OnDrawGizmosSelected()
    {
        foreach(var hex in walkableHexagons)
        {
            Gizmos.color = new Color(hex.weight.food/10, hex.weight.safety/10, hex.weight.sound/10);
            Gizmos.DrawCube(grid.Axial2Pixel(hex.hexCoords)+(hex.height+ new Vector3(hex.weight.food, hex.weight.safety, hex.weight.sound).magnitude/2) *Vector3.up, new Vector3(1, new Vector3(hex.weight.food, hex.weight.safety, hex.weight.sound).magnitude, 1));

            corners = new Vector3[6];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = HexCorners(grid.Axial2Pixel(hex.hexCoords) ,hex.height, i);
            }

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Debug.DrawLine(corners[i], corners[i + 1], hexcolor);
            }
            Debug.DrawLine(corners[5], corners[0], hexcolor);


        } 
    }




}   