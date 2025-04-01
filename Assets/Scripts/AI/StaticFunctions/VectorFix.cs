using UnityEngine;

public static class VectorFix
{
    /// <summary>
    /// Returns a Vector3 where .y = 0.
    /// </summary>
    public static Vector3 returnVector3With0Y(Vector3 input)
    {
        return new Vector3(input.x, 0, input.z);
    }
    public static Vector3 returnVector3With1Y(Vector3 input)
    {
        return new Vector3(input.x, 1, input.z);
    }
}
