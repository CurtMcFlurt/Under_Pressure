using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[ExecuteAlways]
public class GetNavData : MonoBehaviour
{
    private NavMeshTriangulation navMesh;
    public List<Vector3> PosPoints = new List<Vector3>();

    void OnEnable()
    {
        navMesh = NavMesh.CalculateTriangulation();
        Debug.Log(navMesh.areas.Length + " areas");
        Debug.Log(navMesh.indices.Length + " indicies");
        Debug.Log(navMesh.vertices.Length + " Verticies");
        PosPoints.Clear();
        for (int i = 0; i < navMesh.areas.Length; i++)
        {
            var p1 = navMesh.indices[i * 3];
            var p2 = navMesh.indices[i * 3 + 1];
            var p3 = navMesh.indices[i * 3 + 2];
            PosPoints.Add(triangleCenter(navMesh.vertices[p1], navMesh.vertices[p2], navMesh.vertices[p3]));
        }
    }
    private Vector3 triangleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return VectorFix.ReturnVector3WithGroundHeight(new Vector3((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3, (p1.z + p2.z + p3.z) / 3),0);
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < navMesh.vertices.Length; i++)
        {
            Debug.DrawLine(navMesh.vertices[i], navMesh.vertices[i] + Vector3.up * 10); //Drawing a line up at polygon corners
        }
        foreach (var v in PosPoints) //Drawing line up at center of polygons
        {
            Debug.DrawLine(v, v + Vector3.up * 10, Color.blue);
        }
        for (int i = 0; i < navMesh.areas.Length; i++) //Drawing the polygons
        {
            var p1 = navMesh.indices[i * 3];
            var p2 = navMesh.indices[i * 3 + 1];
            var p3 = navMesh.indices[i * 3 + 2];

            Debug.DrawLine(navMesh.vertices[p1], navMesh.vertices[p2], Color.green);
            Debug.DrawLine(navMesh.vertices[p2], navMesh.vertices[p3], Color.yellow);
            Debug.DrawLine(navMesh.vertices[p3], navMesh.vertices[p1], Color.black);
        }
    }
}
