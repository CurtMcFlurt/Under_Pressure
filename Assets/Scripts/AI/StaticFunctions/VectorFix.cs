using UnityEngine;

public static class VectorFix
{
    public static LayerMask groundMask = LayerMask.GetMask("Floor"); // Set the correct layer mask

    public static Vector3 returnVector3With0Y(Vector3 input)
    {
        return new Vector3(input.x, 0, input.z);
    }
    public static Vector3 returnVector3With1Y(Vector3 input)
    {
        return new Vector3(input.x, 1, input.z);
    }

    public static Vector3 ReturnVector3WithGroundHeight(Vector3 input, float heightAboveGround = 1.75f)
    {
        if (Physics.Raycast(input + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundMask))
        {
            return new Vector3(input.x, hit.point.y + heightAboveGround, input.z);
        }
        return new Vector3(input.x,  heightAboveGround, input.z); // Default to original Y if no ground found
    }
}
