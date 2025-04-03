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

    private void CreatePointsNavMesh(Vector3 startPosition, Vector3 endPosition, LayerMask rayCastLayer)
    {
        #region Neighbor of Centers
        List<PointClass> temp = new List<PointClass>();
        for (int i = 0; i < polygonCenters.Count; i++)
        {
            HashSet<Vector3> hashOfNeighborsCenter = new HashSet<Vector3>();

            for (int j = 0; j < polygonCenters.Count; j++)
            {
                if (polygonCenters[i] == polygonCenters[j])
                {
                    continue;
                }

                if (polygonCenters[i] == endPosition || polygonCenters[j] == endPosition)
                {
                    if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i],4), 1.25f, (VectorFix.ReturnVector3WithGroundHeight(polygonCenters[j]) - VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i])).normalized, out whatIHit, ((VectorFix.ReturnVector3WithGroundHeight(polygonCenters[j]) - VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i]))).magnitude, rayCastLayer))
                    {
                        continue;
                    }
                }
                else
                {
                    if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i],4), 1.25f, (VectorFix.ReturnVector3WithGroundHeight(polygonCenters[j]) - VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i])).normalized, out whatIHit, ((VectorFix.ReturnVector3WithGroundHeight(polygonCenters[j]) - VectorFix.ReturnVector3WithGroundHeight(polygonCenters[i]))).magnitude, rayCastLayer))
                    {
                        continue;
                    }
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
                points.Add(p.position, new PointClass(p.position, p.neighborsVertex, p.neighborCenters));
            }
            catch
            {
                Debug.LogWarning("catch when adding point to dict");
            }
        }
        #endregion
    }
   



    public List<Vector3> GeneratePathPhiStar(Vector3 startPosition, Vector3 endPosition, LayerMask raycastLayer)
    {
            return GeneratePathPhiStarNavMesh(startPosition, endPosition, raycastLayer);
      
       
    }

    
    List<Vector3> GeneratePathPhiStarNavMesh(Vector3 startPosition, Vector3 endPosition, LayerMask raycastLayer)
    {
        polygonCenters.Clear();

        polygonCenters.Add(startPosition);
        polygonCenters.AddRange(base.GetPolygonCenters()); //Adds centers of navmesh polygons, Done because centers are needed in creating all possible points and neighbors
        polygonCenters.Add(endPosition);

        points.Clear(); //Clear the dict with all the points

        CreatePointsNavMesh(startPosition, endPosition, raycastLayer); //All possible points in a dict

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
