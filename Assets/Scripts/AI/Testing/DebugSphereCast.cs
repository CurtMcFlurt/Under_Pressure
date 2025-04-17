using UnityEngine;
[ExecuteAlways]
public class DebugSphereCast : MonoBehaviour
{
    public GameObject sphere;
    public GameObject Start;
    public GameObject end;
    public GameObject originalPoint;
    public LayerMask mask;
    private RaycastHit hit=new RaycastHit();
    public Vector3 angleBetween;
    // Update is called once per frame
    void Update()
    {
        if (sphere == null || Start == null || end == null) return;

        Physics.SphereCast(Start.transform.position,.8f, (end.transform.position-Start.transform.position ), out hit, (Start.transform.position - end.transform.position).magnitude,mask);



        //Vector3 newPoint = Vector3.zero;
        //float angle=Vector3.Angle(hit.point-Start.transform.position,originalPoint.transform.position-Start.transform.position);
        //Quaternion quaternion=Quaternion.FromToRotation(Start.transform.position-hit.point  , Start.transform.position- originalPoint.transform.position).normalized;
        //quaternion=Quaternion.Inverse(quaternion);
        //angleBetween = quaternion.normalized* hit.point;

        //sphere.transform.position =angleBetween;
        // Get the direction vector from originalPoint to hit.point
        Vector3 direction = hit.point - Start.transform.position;

        // Choose the rotation axis (e.g., Y-axis rotation)
        Vector3 rotationAxis = Vector3.up; // Adjust if you want to rotate around a different axis

        // Define the rotation angle
        float rotationAngle = Vector3.SignedAngle(hit.point - Start.transform.position, originalPoint.transform.position - Start.transform.position,Vector3.up); // Adjust the rotation angle as needed

        // Rotate the direction vector around the axis
        Vector3 rotatedDirection = (Quaternion.AngleAxis(rotationAngle*2, rotationAxis)) * direction;

        // Calculate the new position by adding the rotated vector to the original point
        Vector3 newPoint = Start.transform.position+ rotatedDirection.normalized*hit.distance;

        // Move the sphere to the new position
        sphere.transform.position = newPoint;

    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Start.transform.position, end.transform.position);
        Gizmos.DrawLine(Start.transform.position, sphere.transform.position);
        Gizmos.DrawLine(Start.transform.position, originalPoint.transform.position);
        Gizmos.DrawSphere(hit.point, .75f);
    
        Gizmos.color= Color.red;
        Gizmos.DrawLine(hit.point,sphere.transform.position);
    }
}
