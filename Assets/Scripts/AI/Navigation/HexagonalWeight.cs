using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[System.Serializable]
public struct HexCell
{
    public Vector3 hexCoords; // QRS Coordinates
    public float height;      // Height Value
    public int stackLevel;    // Stack Position for Multi-level Maps
    public bool isWalkable;   // Walkability Flag
    public HeatMapValues weight;
    public float timeSinceChecked;
    public HexCell(Vector3 hexCoords, float height, int stackLevel, bool isWalkable,HeatMapValues weight=new HeatMapValues())
    {
        this.hexCoords = hexCoords;
        this.height = height;
        this.stackLevel = stackLevel;
        this.isWalkable = isWalkable;
        this.weight = weight;
        timeSinceChecked = Time.deltaTime;
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

    // Changed from List to Dictionary
    public Dictionary<Vector3, HexCell> walkableHexagons = new Dictionary<Vector3, HexCell>();
    public int walkables = 0;
    public LayerMask floorMask;

    private void OnEnable()
    {
        SetUpHexes();
    }

    private void Update()
    {
        foreach (var coord in grid.coordinate)
        {
            // You could draw gizmos here if needed
        }

        if (range != oldRange || cellSize != oldCell)
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

    private Vector3 HexCorners(Vector3 pos, float height, int i)
    {
        float angle_rad = Mathf.PI / 180 * (60 * i);
        return new Vector3(pos.x + cellSize * Mathf.Cos(angle_rad), height, pos.z + cellSize * Mathf.Sin(angle_rad));
    }

    private void FindAllAccessibleHexes()
    {
        walkableHexagons.Clear();

        foreach (var hexCoord in grid.hexvalue)
        {
            Vector3 worldPos = grid.Axial2Pixel(hexCoord);
            int stackLevel = 0;
            float maxCheckHeight = 50f;
            bool foundBase = false;

            while (worldPos.y <= maxCheckHeight)
            {
                if (NavMesh.SamplePosition(worldPos, out var hit, 3f, NavMesh.AllAreas))
                {
                    float height = hit.position.y;
                    HexCell hexCell = new HexCell(hexCoord, height, stackLevel, true);

                    // Insert into dictionary using hexCoord as key
                    walkableHexagons[hexCoord] = hexCell;
                    foundBase = true;
                }

                if (foundBase)
                {
                    if (Physics.Raycast(worldPos, Vector3.up, out RaycastHit levelHit, maxCheckHeight - worldPos.y, floorMask))
                    {
                        worldPos = levelHit.point + Vector3.up * 0.1f;
                        stackLevel++;
                        continue;
                    }
                    else break;
                }

                worldPos.y += cellSize;
            }
        }
    }



    public void OnDrawGizmosSelected()
    {
        foreach (var hex in walkableHexagons.Values)
        {
            // Calculate heat magnitude for cube size and color
            var weightVec = new Vector3(hex.weight.food, hex.weight.safety, hex.weight.sound);
            float magnitude = weightVec.magnitude;
            Vector3 basePos = grid.Axial2Pixel(hex.hexCoords);

            // Set gizmo color based on heat values
            Gizmos.color = new Color(hex.weight.food / 10f, hex.weight.safety / 10f, hex.weight.sound / 10f);

            // Draw cube with height based on heat magnitude
            Gizmos.DrawCube(basePos + (hex.height + magnitude / 2f) * Vector3.up, new Vector3(1, magnitude, 1));

            // Draw hex outline
            corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                corners[i] = HexCorners(basePos, hex.height, i);
            }
        }

        
    }




}   