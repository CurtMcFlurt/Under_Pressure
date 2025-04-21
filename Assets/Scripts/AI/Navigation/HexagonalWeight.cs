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
    public Dictionary<Vector3, HexCell> fullMapHexagons = new Dictionary<Vector3, HexCell>();
    public List<WeightChangers> weightChangers = new List<WeightChangers>();
    public List<WeightChangers> playerWeights = new List<WeightChangers>();
    public int walkables = 0;
    public LayerMask floorMask;

    private void OnEnable()
    {
        
        SetUpHexes();
        playerWeights.Clear();
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
        foreach(var Weight in weightChangers)
        {
            if (Weight.Player && !playerWeights.Contains(Weight)) playerWeights.Add(Weight);
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
        fullMapHexagons.Clear();

        HashSet<Vector3> addedToFullMap = new HashSet<Vector3>();

        foreach (var hexCoord in grid.hexvalue)
        {
            Vector3 worldPos = grid.Axial2Pixel(hexCoord);
            int stackLevel = 0;
            float maxCheckHeight = 50f;
            bool foundBase = false;

            while (worldPos.y <= maxCheckHeight)
            {
                if (NavMesh.SamplePosition(worldPos, out var hit, 1.35f, NavMesh.AllAreas))
                {
                    float height = hit.position.y;
                    HexCell hexCell = new HexCell(hexCoord, height, stackLevel, true);

                    walkableHexagons[hexCoord] = hexCell;
                    fullMapHexagons[hexCoord] = hexCell;
                    addedToFullMap.Add(hexCoord);
                    foundBase = true;
                    break;
                }
                else if (Physics.Raycast(worldPos + Vector3.up * 0.5f, Vector3.down, out var rayHit, 5f, floorMask))
                {
                    float height = rayHit.point.y;
                    HexCell hexCell = new HexCell(hexCoord, height, stackLevel, false);
                    fullMapHexagons[hexCoord] = hexCell;
                    addedToFullMap.Add(hexCoord);
                    foundBase = true;
                    break;
                }

                worldPos.y += cellSize;
            }

            // Add direct neighbors of valid hex, inherit height and stackLevel from base hex
            if (foundBase)
            {
                float baseHeight = fullMapHexagons[hexCoord].height;
                int baseStack = fullMapHexagons[hexCoord].stackLevel;

                foreach (var direction in HexMath.cubeDirectionVectors)
                {
                    Vector3 neighborCoord = hexCoord + direction;

                    if (!addedToFullMap.Contains(neighborCoord))
                    {
                        HexCell neighborHex = new HexCell(neighborCoord, baseHeight, baseStack, false);
                        fullMapHexagons[neighborCoord] = neighborHex;
                        addedToFullMap.Add(neighborCoord);
                    }
                }
            }
        }
    }




    public void OnDrawGizmos()
    {
        foreach (var hex in fullMapHexagons.Values)
        {
            Vector3 weightVec = new Vector3(hex.weight.food, hex.weight.safety, hex.weight.sound);
            float magnitude = weightVec.magnitude;
            Vector3 basePos = grid.Axial2Pixel(hex.hexCoords);

            // Normalize weight values to create a target color
            Color targetColor = new Color(hex.weight.food / 10f, hex.weight.safety / 10f, hex.weight.sound / 10f);

            // Lerp from white if walkable, black otherwise
            if (walkableHexagons.ContainsKey(hex.hexCoords))
            {
                Gizmos.color = Color.Lerp(Color.white, targetColor, 0.8f); // 80% toward heat color
            }
            else
            {
                Gizmos.color = targetColor * 0.25f; // dim non-walkables (optional)
            }

            // Draw cube based on weight magnitude
            Gizmos.DrawCube(basePos + (hex.height + magnitude / 2f) * Vector3.up, new Vector3(1, magnitude, 1));

            // Draw hex outline
            corners = new Vector3[6];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = HexCorners(basePos, hex.height, i);
            }
            for (int i = 0; i < 5; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
            Gizmos.DrawLine(corners[5], corners[0]);
        }


    }




}   