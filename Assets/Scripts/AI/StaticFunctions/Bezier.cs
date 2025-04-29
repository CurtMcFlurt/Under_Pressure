using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;



public static class Bezier
{
    /// <summary>
    /// Calculates a point on a quadratic Bézier curve.
    /// </summary>
    /// <param name="p0">The start point of the curve.</param>
    /// <param name="p1">The control point of the curve.</param>
    /// <param name="p2">The end point of the curve.</param>
    /// <param name="t">The parameter, where 0 <= t <= 1.</param>
    /// <returns>The point on the curve at t.</returns>


    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t); // Ensure t is within [0, 1]
        float oneMinusT = 1 - t;
        return oneMinusT * oneMinusT *p0 + 2 * oneMinusT * t * p1 + t * t * p2;
    }

    public static List<Vector3> BezirPath(List<Vector3> points, float pathIntervalLeangth, LayerMask layer, float playerThickness)
    {
        List<Vector3> originalPoints = new List<Vector3>();

        originalPoints.AddRange(points);

        List<Tuple<Vector3, int>> sphereHitTuples = new List<Tuple<Vector3, int>>();
        List<Tuple<Vector3, int, int>> hitReps = new List<Tuple<Vector3, int, int>>();

        bool sphereHit = false;

        //Create a list of tuples of vector3 and int and add startPoint
        List<Tuple<Vector3, int>> midPointsTuple = new List<Tuple<Vector3, int>>
        {
            new Tuple<Vector3, int>(originalPoints[0], 0)
        };

        int panic = 0;

        int currentMax = -1;

    MidPointLoop:


        if (panic >= 300)
        {
            Debug.LogWarning("panic");
            goto AfterMidPointLoop;
        }
        panic++;


        sphereHitTuples.Clear();
        midPointsTuple.Clear();
        midPointsTuple.Add(new Tuple<Vector3, int>(originalPoints[0], 0)); //add startPoint
        sphereHit = false;


        if (currentMax != -1)
        {

            //If Linecast doesn't find something between the phantom and next-next point, remove the 'next' point
            if (!Physics.Linecast(originalPoints[currentMax + 1], originalPoints[currentMax + 3], layer))
            {
                originalPoints.RemoveAt(currentMax + 2);
            }

        }


        //originalPoints.Count-1 so the endPoint is never i
        for (int i = 0; i < originalPoints.Count - 1; i++)
        {
            //Creates amount of midPoints between points i and i+1 based on input:ed 'List<float>'s .Count, for each originalPoint, @ List<float>[i]% of point i and i+1
            //List<Vector3> tempMidPoints = CreateMidPoints(originalPoints[i], originalPoints[i+1], new List<float> { 0.33f, 0.66f });

            int draw1 = i;
            int draw2 = i + 1;



            List<Vector3> tempMidPoints = CreateMidPointsBasedOnDistance(originalPoints[draw1], originalPoints[draw2]);


            foreach (var v in tempMidPoints)
            {
                midPointsTuple.Add(new Tuple<Vector3, int>(v, i));
            }
        }

        for (int j = 0; j < midPointsTuple.Count - 1; j++)
        {
            RaycastHit sphereHitRay;


            if (midPointsTuple[j + 1].Item2 == midPointsTuple[midPointsTuple.Count - 1].Item2)
            {
                //Right now phi-star makes sure that the path between the last point and the one before is fine, therefore it doesn't need to check here
            }
            else if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(midPointsTuple[j].Item1), playerThickness, (midPointsTuple[j + 1].Item1 - midPointsTuple[j].Item1).normalized, out sphereHitRay,

                Vector3.Distance(VectorFix.ReturnVector3WithGroundHeight(midPointsTuple[j + 1].Item1,0), VectorFix.ReturnVector3WithGroundHeight(midPointsTuple[j].Item1,0)),

                layer
            ))
            {

                bool repped = false;
                for (int i = 0; i < hitReps.Count - 1; i++)
                {
                    if (hitReps[i].Item1 == sphereHitRay.point)
                    {

                        hitReps[i] = new Tuple<Vector3, int, int>(hitReps[i].Item1, hitReps[i].Item2 + 1, hitReps[i].Item3);
                        repped = true;
                        break;
                    }
                }
                if (repped == false)
                {
                    hitReps.Add(new Tuple<Vector3, int, int>(sphereHitRay.point, 1, midPointsTuple[j].Item2));
                }

                Vector3 orignalPointGoTo = originalPoints[midPointsTuple[j].Item2 + 1];

                float myAngle = Vector3.Angle(midPointsTuple[j].Item1, sphereHitRay.point);



                Vector3 phantomPoint = new Vector3();

                float distanceForNewPoint = 2.5f;
                RaycastHit rayHit;
                //Vector3 shouldMovePhantomPointInThisDirection = new Vector3();


                //Start a tiny but away from .point
                if (Physics.Raycast(sphereHitRay.point + (sphereHitRay.normal * 0.1f), (sphereHitRay.normal).normalized, out rayHit, distanceForNewPoint, layer))
                {
                    distanceForNewPoint = rayHit.distance;
                }
                Debug.DrawRay(sphereHitRay.point + (sphereHitRay.normal * 0.01f), (sphereHitRay.normal).normalized * distanceForNewPoint, Color.black, 10f);




                Vector3 direction = (VectorFix.returnVector3With0Y(sphereHitRay.point) - VectorFix.returnVector3With0Y(midPointsTuple[j].Item1)).normalized;

                // Choose the rotation axis (e.g., Y-axis rotation)
                Vector3 rotationAxis = Vector3.up; // Adjust if you want to rotate around a different axis

                // Define the rotation angle
                float rotationAngle = Vector3.SignedAngle(sphereHitRay.point - midPointsTuple[j].Item1, originalPoints[midPointsTuple[j].Item2] - midPointsTuple[j].Item1, rotationAxis); // Adjust the rotation angle as needed

                // Rotate the direction vector around the axis
                Vector3 rotatedDirection = (Quaternion.AngleAxis(rotationAngle * 2, rotationAxis)) * direction;


                // Calculate the new position by adding the rotated vector to the original point
                phantomPoint = CreatePhantomPoint(midPointsTuple[j].Item1, rotatedDirection, distanceForNewPoint);

                Tuple<Vector3, int> tempTuple = new Tuple<Vector3, int>(phantomPoint, midPointsTuple[j].Item2);


                //While there's an obstacle between the original point and the phantom point, change where the phantom point is placed
                while (Physics.Linecast(originalPoints[midPointsTuple[j].Item2], phantomPoint, layer) && distanceForNewPoint > 0)
                {
                    distanceForNewPoint -= 0.1f;

                    phantomPoint = CreatePhantomPoint(midPointsTuple[j].Item1, rotatedDirection, distanceForNewPoint);
                }

                //The loop finishes in wo ways, either the line is legit, or distanceForNewPoint becomes equal to 0
                //If it isn't 0, proceed like normal. If it is, give an error
                if (distanceForNewPoint <= 0)
                {
                    Debug.LogError("distanceForNewPoint is equal to or lesss than 0");
                }
                else
                {
                    tempTuple = new Tuple<Vector3, int>(VectorFix.ReturnVector3WithGroundHeight(phantomPoint,0), midPointsTuple[j].Item2);

                    if (!sphereHitTuples.Contains(tempTuple))
                    {
                        sphereHitTuples.Add(tempTuple);
                    }

                    sphereHit = true;
                    currentMax = tempTuple.Item2;
                    break;
                }

               
            }

      
        }


        //Add points in originalPoints list @ index i
        List<Vector3> tempOriginalPoints = new List<Vector3>();
        tempOriginalPoints.Clear();

        tempOriginalPoints.Add(originalPoints[0]); //startpoint

        int indexOfLastOriginalPointAdded = 0;

        int previousIndex = 0;
        foreach (var sht in sphereHitTuples)
        {
            if (tempOriginalPoints.Contains(sht.Item1)) continue;

            if (sht.Item2 == 0)
            {
                tempOriginalPoints.Add(sht.Item1);
                continue;
            }

            for (int i = previousIndex; i <= sht.Item2; i++)
            {
                if (tempOriginalPoints.Contains(originalPoints[i])) continue;

              

                tempOriginalPoints.Add(originalPoints[i]);
                indexOfLastOriginalPointAdded = i;
            }

            bool temp = false;
            for (int j = 0; j < hitReps.Count; j++)
            {
                if (hitReps[j].Item2 < 2) continue;

                if (tempOriginalPoints[tempOriginalPoints.Count - 1] == originalPoints[hitReps[j].Item3])
                {
                    previousIndex = sht.Item2 + 1;
                    temp = true;
                    break;
                }
            }
            if (temp == false)
            {
                previousIndex = sht.Item2 + 1;
            }

            tempOriginalPoints.Add(sht.Item1);
        }

        foreach (var op in originalPoints)
        {
            if (tempOriginalPoints.Contains(op)) continue;
            tempOriginalPoints.Add(op);
        }

       
       
        originalPoints.Clear();
        originalPoints.AddRange(tempOriginalPoints);
        //originalPoints = tempOriginalPoints;


        if (sphereHit == true)
        {

            goto MidPointLoop;
        }
        else currentMax = -1;

        //MidPointLoop loops if something is hit

        AfterMidPointLoop:

        //As of 2025/02/25 @ 17:30: The github úpload from 4 days ago (2025/02/21) have us clear the midPointsTuple each time we add a phantomPoint, therefore the
        //below code should be unneccesary

        #region oldCode
    
        #endregion

        //Add endPoint as a midpoint, in accordance to how the loop above works (last i = .Count-2)
        midPointsTuple.Add(new Tuple<Vector3, int>(originalPoints[originalPoints.Count - 1], originalPoints.Count - 2));


        List<Vector3> returnList = new List<Vector3>
        {
            originalPoints[0], //startpoint
        };


        returnList = BreakUpPath(midPointsTuple, originalPoints, pathIntervalLeangth, layer);

        #region before breaking out BreakUpPath
        //float stepLenght = 0;
        //Vector3 currentPoint = new Vector3();
        //Vector3 previousPoint = new Vector3();


        ////midPoints.Count-1 so the endPoint is never i
        //for (int i = 0; i < midPointsTuple.Count - 1; i++)
        //{
        //    //Between point and nextPoint
        //    stepLenght = Mathf.Pow(Vector3.Distance(VectorFix.returnVector3With0Y(midPointsTuple[i].Item1), VectorFix.returnVector3With0Y(midPointsTuple[i + 1].Item1)), -1);

        //    List<Vector3> tempList = new List<Vector3>
        //    {
        //        midPointsTuple[i].Item1, //startpoint of each bezier path
        //    };


        //    for (float j = 0; j < 1 - stepLenght; j += stepLenght)
        //    {
        //        previousPoint = tempList[tempList.Count - 1];

        //        if (midPointsTuple[i].Item2 == midPointsTuple[i + 1].Item2)
        //        {
        //            Vector3 middle = (VectorFix.returnVector3With0Y(midPointsTuple[i].Item1) + VectorFix.returnVector3With0Y(midPointsTuple[i + 1].Item1)) / 2;
        //            currentPoint = GetPoint(midPointsTuple[i].Item1, middle, midPointsTuple[i + 1].Item1, j);
        //        }
        //        else
        //        {
        //            currentPoint = GetPoint(midPointsTuple[i].Item1, originalPoints[midPointsTuple[i + 1].Item2], midPointsTuple[i + 1].Item1, j);
        //        }

        //        if (Vector3.Distance(VectorFix.returnVector3With0Y(currentPoint), VectorFix.returnVector3With0Y(previousPoint)) > pathIntervalLeangth && !tempList.Contains(currentPoint))
        //        {
        //            tempList.Add(currentPoint);
        //        }
        //    }

        //    foreach (var v in tempList)
        //    {
        //        if (returnList.Contains(v))
        //        {
        //            continue;
        //        }

        //        returnList.Add(v);
        //    }
        //}
        #endregion

        returnList.Add(originalPoints[originalPoints.Count - 1]);
        return returnList;
    }

    static Vector3 CreatePhantomPoint(Vector3 midPointTheHitCameFrom, Vector3 rotatedDirection, float distanceForNewPoint)
    {
        // Calculate the new position by adding the rotated vector to the original point
        Vector3 newPoint = midPointTheHitCameFrom + rotatedDirection.normalized * distanceForNewPoint;

        newPoint = VectorFix.ReturnVector3WithGroundHeight(newPoint,0);

        return newPoint;
    }

    public static List<Vector3> BreakUpPath(List<Tuple<Vector3, int>> inputList, List<Vector3> originalPoints, float pathIntervalLeangth, LayerMask layer)
    {
        float stepLenght = 0;
        Vector3 currentPoint = new Vector3();
        Vector3 previousPoint = new Vector3();

        List<Vector3> returnList = new List<Vector3>
        {
            originalPoints[0], //startpoint
        };

        //midPoints.Count-1 so the endPoint is never i
        for (int i = 0; i < inputList.Count - 1; i++)
        {
            //Between point and nextPoint

            stepLenght = Mathf.Pow(Vector3.Distance(VectorFix.returnVector3With0Y(inputList[i].Item1), VectorFix.returnVector3With0Y(inputList[i + 1].Item1)), -1);
            List<Vector3> tempList = new List<Vector3>
            {
                inputList[i].Item1, //startpoint of each bezier path
            };

            for (float j = 0; j < 1 - stepLenght; j += stepLenght)
            {
                previousPoint = tempList[tempList.Count - 1];

                if (inputList[i].Item2 == inputList[i + 1].Item2)
                {
                    Vector3 middle = (VectorFix.returnVector3With0Y(inputList[i].Item1) + VectorFix.returnVector3With0Y(inputList[i + 1].Item1)) / 2;
                    //currentPoint = GetPoint(inputList[i].Item1, middle, inputList[i + 1].Item1, j);
                    currentPoint = Vector3.Lerp(inputList[i].Item1, inputList[i + 1].Item1, j);

                }
                else
                {
                    currentPoint = GetPoint(inputList[i].Item1, originalPoints[inputList[i + 1].Item2], inputList[i + 1].Item1, j);
                }

                if (Vector3.Distance(VectorFix.returnVector3With0Y(currentPoint), VectorFix.returnVector3With0Y(previousPoint)) > pathIntervalLeangth && !tempList.Contains(currentPoint))
                {
                    tempList.Add(currentPoint);
                }
            }

            foreach (var v in tempList)
            {
                if (returnList.Contains(v))
                {
                    continue;
                }

                returnList.Add(v);
            }
        }

        return returnList;
    }
    private static List<Vector3> CreateMidPoints(Vector3 p1, Vector3 p2, List<float> tValues)
    {
        List<Vector3> midPointsL = new List<Vector3>();

        for (int i = 0; i < tValues.Count; i++)
        {
            tValues[i] = Mathf.Clamp01(tValues[i]); // Ensure t is within [0, 1]
        }

        tValues.OrderBy(t => t); //Should order values from smallest to largest

        foreach (float t in tValues)
        {
            midPointsL.Add(Vector3.Lerp(p1, p2, t));
        }

        return midPointsL;
    }
    private static List<Vector3> CreateMidPointsBasedOnDistance(Vector3 p1, Vector3 p2)
    {
        List<Vector3> midPointsL = new List<Vector3>();


        int distance = (int)Vector3.Distance(VectorFix.ReturnVector3WithGroundHeight(p1,0), VectorFix.ReturnVector3WithGroundHeight(p2,0));


        float minDis = 2f;

        int amountOfTValues = 0;

        List<float> tValues = new List<float>();

        //From really quick testing, 10 seems good. Test more
        //After more testing, 5 is better
        distance = distance / 5;

        if (distance < minDis)
        {
            amountOfTValues = 2;
        }
        else
        {
            amountOfTValues = distance;
        }

        for (int i = 0; i < amountOfTValues; i++)
        {
            double t = (100 / (amountOfTValues + 1)) * (i + 1);
            t /= 100;

            tValues.Add((float)t);
        }



        for (int i = 0; i < tValues.Count; i++)
        {
            tValues[i] = Mathf.Clamp01(tValues[i]); // Ensure t is within [0, 1]
        }

        tValues.OrderBy(t => t); //Should order values from smallest to largest

        foreach (float t in tValues)
        {
            midPointsL.Add(Vector3.Lerp(p1, p2, t));
        }

        return midPointsL;
    }
}