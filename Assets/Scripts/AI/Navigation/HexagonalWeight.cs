using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    public HashSet<Vector4> walkableHexagons = new HashSet<Vector4>();
    public int walkables = 0;
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
            DrawHex(currentHex);
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
        FindAllAcessibleHexes();
    }
    private void DrawHex(Vector3 pos)
    {
        corners = new Vector3[6];
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = HexCorners(pos, i);
        }

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Debug.DrawLine(corners[i], corners[i + 1], hexcolor);
        }
        Debug.DrawLine(corners[5], corners[0], hexcolor);


        

    }
    private Vector3 HexCorners(Vector3 pos, int i)
    {
        var angle_deg = 60 * i;
        float angle_rad = Mathf.PI / 180 * angle_deg;
        return new Vector3(pos.x + cellSize * Mathf.Cos(angle_rad), 0, pos.z + cellSize * Mathf.Sin(angle_rad));
    }
    private void FindAllAcessibleHexes()
    {
        for (int i = 0; i < grid.coordinate.Count; i++)
       

        {

            if (NavMesh.SamplePosition(grid.coordinate[i],out var hit, 3f, NavMesh.AllAreas))
            {
                walkableHexagons.Add(new Vector4(grid.hexvalue[i].x, grid.hexvalue[i].y, grid.hexvalue[i].z, 1));
                continue;
            }
                       
            //for (int j = 0; j < 6; j++)
            //{
            //    if (NavMesh.SamplePosition(HexCorners(grid.coordinate[i], i), out var hits, 1f, NavMesh.AllAreas))
            //    {
            //        walkableHexagons.Add(new Vector4(grid.hexvalue[i].x, grid.hexvalue[i].y, grid.hexvalue[i].z, 1));
            //        break;
            //    }
            //}
        }
    }



    public void OnDrawGizmos()
    {
        foreach(var hex in walkableHexagons)
        {
            Gizmos.DrawCube(grid.Axial2Pixel(hex), new Vector3(1, 2, 1));
        }
    }




}   