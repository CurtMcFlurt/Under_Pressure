using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PointClass
{
    public Vector3 position;

    public HashSet<Vector3> neighborsVertex = new HashSet<Vector3>();
    public HashSet<Vector3> neighborCenters = new HashSet<Vector3>();
    public HashSet<Vector3> allNeighbors = new HashSet<Vector3>();

    public Vector3 cameFrom = new Vector3();

    public PointClass(Vector3 pos, HashSet<Vector3> vertx, HashSet<Vector3> cent)
    {
        position = pos;
        neighborsVertex = vertx;
        neighborCenters = cent;

        try
        {
            //allNeighbors = vertx; //Comment to only use polygon centerPoints
            allNeighbors.UnionWith(cent);
        }
        catch
        {
            Debug.LogWarning("All neighbor = null. If there is only one of these warnings, high chance it's the endpoint, that has 0 neighbors purposfully");
        }

    }
}
