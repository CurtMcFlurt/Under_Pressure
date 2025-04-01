using UnityEngine;

public static class Heuristic
{


    public static double Distance(Vector3 start, Vector3 goal)
    {
        return Mathf.Abs(start.x - goal.x) + Mathf.Abs(start.z - goal.z);
    }
}
