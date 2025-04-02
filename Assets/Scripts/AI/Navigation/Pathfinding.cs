using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pathfinding
{
    protected NavMeshTriangulation navMesh;
    public List<Vector3> PosPoints = new List<Vector3>();

    public virtual void GetPath(Vector3 start, Vector3 end, LayerMask mask)
    {
        var curpos = new NavMeshHit();
        navMesh = NavMesh.CalculateTriangulation();
        Debug.Log(navMesh.areas.Length + " areas");
        Debug.Log(navMesh.indices.Length + " indicies");
        Debug.Log(navMesh.vertices.Length + " Verticies");
        PosPoints.Clear();

        NavMesh.SamplePosition(start, out curpos, 1, mask);

        for (int i = 0; i < navMesh.areas.Length; i++)
        {
            var p1 = navMesh.indices[i * 3];
            var p2 = navMesh.indices[i * 3 + 1];
            var p3 = navMesh.indices[i * 3 + 2];
            PosPoints.Add(triangleCenter(navMesh.vertices[p1], navMesh.vertices[p2], navMesh.vertices[p3]));
     

        }
    }

    public virtual List<Vector3> GetPolygonCenters()
    {
        navMesh = NavMesh.CalculateTriangulation();
        //Debug.Log(navMesh.areas.Length + " areas");
        //Debug.Log(navMesh.indices.Length + " indicies");
        //Debug.Log(navMesh.vertices.Length + " Verticies");

        PosPoints.Clear();

        for (int i = 0; i < navMesh.areas.Length; i++)
        {
            var p1 = navMesh.indices[i * 3];
            var p2 = navMesh.indices[i * 3 + 1];
            var p3 = navMesh.indices[i * 3 + 2];
            PosPoints.Add(triangleCenter(navMesh.vertices[p1], navMesh.vertices[p2], navMesh.vertices[p3]));
   
        }

        return PosPoints;
    }

    protected Vector3 triangleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return new Vector3((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3, (p1.z + p2.z + p3.z) / 3);
    }
}