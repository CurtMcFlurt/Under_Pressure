using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Theta_Star : Pathfinding
{
    [Tooltip("List of centers and vertex of polygons created by navmesh")]
    public List<Vector3> polygonCenters = new List<Vector3>();

    [Tooltip("List of points the path takes")]
    public List<Vector3> phiStarPath = new List<Vector3>();

    public Dictionary<Vector3, PointClass> points = new Dictionary<Vector3, PointClass>();

    private Dictionary<Vector3, double> costFromStart = new Dictionary<Vector3, double>();

    private Dictionary<Vector3, double> costToGoal = new Dictionary<Vector3, double>();

    [Tooltip("This gives info on what the raycast hit")]
    private RaycastHit whatIHit;
  
    bool firstFallBackBool = false;

    public override void GetPath(Vector3 start, Vector3 end, LayerMask mask)
    {
        base.GetPath(start, end, mask);
    }

    public Dictionary<Vector3, PointClass> CreatePointsNavMesh(LayerMask rayCastLayer)
    {
        #region Neighbor of Centers
        polygonCenters = GetPolygonCenters();
        List<PointClass> temp = new List<PointClass>();
        Dictionary<Vector3, PointClass> InstancedPoints = new Dictionary<Vector3, PointClass>();
        for (int i = 0; i < polygonCenters.Count; i++)
        {
            HashSet<Vector3> hashOfNeighborsCenter = new HashSet<Vector3>();

            for (int j = 0; j < polygonCenters.Count; j++)
            {
                if (polygonCenters[i] == polygonCenters[j])
                {
                    continue;
                }

               
                  if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i],4), 1.25f, (VectorFix.ReturnVector3WithGroundHeight(polygonCenters[j]) - VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i])).normalized, out whatIHit, ((VectorFix.ReturnVector3WithGroundHeight(polygonCenters[j]) - VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i]))).magnitude, rayCastLayer))
                    {
                        continue;
                    }
                
           

                //Debug.LogWarning("Hit");
                hashOfNeighborsCenter.Add(polygonCenters[j]);
            }

            temp.Add(new PointClass(polygonCenters[i], null, hashOfNeighborsCenter));
        }
        foreach (var p in temp)
        {
            try
            {
                InstancedPoints.Add(p.position, new PointClass(p.position, p.neighborsVertex, p.neighborCenters));
            }
            catch
            {
                Debug.LogWarning("catch when adding point to dict");
            }
        }

        return InstancedPoints;
        #endregion
    }
   
               
    public PointClass AddPos(Vector3 Position, Dictionary<Vector3, PointClass> InstancedPoints,LayerMask raycastLayer)
    {
        HashSet<Vector3> hashOfNeighborsCenter = new HashSet<Vector3>();
        if(InstancedPoints== null) return new PointClass(Position, null, hashOfNeighborsCenter);
       
        foreach (var p in InstancedPoints)
        {
            if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(Position, 4), 1.25f, (VectorFix.ReturnVector3WithGroundHeight(p.Key) - VectorFix.ReturnVector3WithGroundHeight(Position)).normalized, out whatIHit, ((VectorFix.ReturnVector3WithGroundHeight(p.Key) - VectorFix.ReturnVector3WithGroundHeight(Position))).magnitude, raycastLayer))
            {
                continue;
            }

            hashOfNeighborsCenter.Add(p.Key);
        }
        return  new PointClass(Position, null, hashOfNeighborsCenter);
    }
    public Dictionary<Vector3, PointClass> AddEnd(Vector3 Position, Dictionary<Vector3, PointClass> ActivePoints, LayerMask raycastLayer)
    {
        var temp = ActivePoints;

        foreach (var p in temp)
        {
            if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(Position, 4), 1.25f, (VectorFix.ReturnVector3WithGroundHeight(p.Key) - VectorFix.ReturnVector3WithGroundHeight(Position)).normalized, out whatIHit, ((VectorFix.ReturnVector3WithGroundHeight(p.Key) - VectorFix.ReturnVector3WithGroundHeight(Position))).magnitude, raycastLayer))
            {
                continue;
            }

            p.Value.allNeighbors.Add(Position);
        }
        temp.Add(Position, new PointClass(Position, null, null));
        return temp;
    }
    public List<Vector3> GeneratePathPhiStarNavMesh(Vector3 startPosition, Vector3 endPosition, LayerMask raycastLayer, Dictionary<Vector3, PointClass> InstancedPoints)
    {
        points.Clear();
        points = InstancedPoints;
        try
        {
            points.Add(startPosition, AddPos(startPosition, InstancedPoints, raycastLayer));
        }
        catch
        {
         //   Debug.LogWarning("startPosition double");
        }
        try
        {
            points= AddEnd(endPosition, points, raycastLayer);
        }
        catch
        {
           // Debug.LogWarning("endPosition double");
        }
        costFromStart.Clear();
        costToGoal.Clear();

        List<PointClass> pointsNotChecked = new List<PointClass>();
        List<Vector3> pointsChecked = new List<Vector3>();
        
        pointsNotChecked.Add(points[startPosition]);
      
        costFromStart.Add(pointsNotChecked[0].position, 0);
        costToGoal.Add(pointsNotChecked[0].position, Heuristic.Distance(pointsNotChecked[0].position, endPosition));

        phiStarPath.Clear();

        int panic = 0;
        int panic2 = 0;

        while (pointsNotChecked.Count > 0)
        {
            if (panic > 400)
            {
                Debug.LogError("panic");
                break;
            }
            panic++;

            var current = pointsNotChecked.OrderBy(point => costFromStart[point.position] + costToGoal[point.position]).First();

            //endPosition is always last in list
            if (current.position == endPosition)
            {
                Debug.LogWarning("finds endpoint");
                Vector3 active = current.position;

                phiStarPath.Add(endPosition);

                while (active != startPosition)
                {
                    if (panic2 > 100)
                    {
                        Debug.LogError("panic");
                        break;
                    }
                    panic2++;

                    active = points[active].cameFrom;
                    phiStarPath.Add(active);
                }

                phiStarPath.Reverse();
                return phiStarPath;
            }

            pointsNotChecked.Remove(current);
            pointsChecked.Add(current.position);

            List<Vector3> tempCurrentNeighbors = current.allNeighbors.ToList();
            bool tempBool = false;
            List<Vector3> allNeighborsChecked = new List<Vector3>();
            allNeighborsChecked.Clear();
            firstFallBackBool = false;

            if (tempCurrentNeighbors.Contains(endPosition))
            {
                allNeighborsChecked.Add(endPosition);

                double tentativeScore = costFromStart[current.position] + (endPosition - current.position).magnitude;

                if (!costFromStart.ContainsKey(endPosition) || tentativeScore < costFromStart[endPosition])
                {
                    costFromStart[endPosition] = tentativeScore;
                    costToGoal[endPosition] = Heuristic.Distance(endPosition, endPosition);

                    points[endPosition].cameFrom = current.position;

                    if (!pointsNotChecked.Contains(points[endPosition]))
                    {
                        tempBool = true;
                        pointsNotChecked.Add(points[endPosition]);
                    }
                }
            }



        NeighborLoop:
            foreach (var neigh in tempCurrentNeighbors)
            {
                var neighbor = neigh;
                if (neighbor == null || pointsChecked.Contains(neighbor))
                {
                    continue;
                }
                if (allNeighborsChecked.Contains(neigh))
                {
                    continue;
                }

              


                allNeighborsChecked.Add(neighbor);

                double tentativeScore = costFromStart[current.position] + (neighbor - current.position).magnitude;

                if (!costFromStart.ContainsKey(neighbor) || tentativeScore < costFromStart[neighbor])
                {
                    costFromStart[neighbor] = tentativeScore;
                    costToGoal[neighbor] = Heuristic.Distance(neighbor, endPosition);

                    points[neighbor].cameFrom = current.position;

                    if (!pointsNotChecked.Contains(points[neighbor]))
                    {
                        tempBool = true;
                        pointsNotChecked.Add(points[neighbor]);
                    }
                }

                if (neighbor == tempCurrentNeighbors[tempCurrentNeighbors.Count - 1])
                {
                    if (tempBool) continue;

                    if (firstFallBackBool == false)
                    {
                        firstFallBackBool = true;

                        goto NeighborLoop;
                    }


                    goto NeighborLoop;
                }
            }
        }

        Debug.LogWarning("Didn't find path");
        return phiStarPath; //Want to return a list with the full path
    }

   
}
