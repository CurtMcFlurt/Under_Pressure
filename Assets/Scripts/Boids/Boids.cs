using UnityEngine;

public class Boids : MonoBehaviour
{
    public BoidSettings BoidSettings;

    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;

    private Vector3 velocity;

    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;


    private Material material;
    private Transform cachedTransform;
    public Transform target;


    void Awake()
    {
        material = transform.GetComponentInChildren<MeshRenderer>().material;
        cachedTransform = transform;
    }

    public void Initialize(BoidSettings settings, Transform target)
    {
        this.target = target;
        this.BoidSettings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity=transform.forward*startSpeed;

    }
    public void SetColour(Color colour)
    {
        if (material != null)
        {
                material.color = colour;
        }
    }

    public void UpdateBoid()
    {
        Vector3 acceleration = Vector3.zero;

        if (target != null)
        {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards(offsetToTarget) * BoidSettings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards(avgFlockHeading) * BoidSettings.alignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * BoidSettings.cohesionWeight;
            var seperationForce = SteerTowards(avgAvoidanceHeading) * BoidSettings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * BoidSettings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, BoidSettings.minSpeed, BoidSettings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }
    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, BoidSettings.boundsRadius, forward, out hit, BoidSettings.collisionAvoidDst, BoidSettings.obstacleMask))
        {
            return true;
        }
        else { }
        return false;
    }
    public void TeleportBoid(Vector3 position)
    {
       transform.position = position;
        cachedTransform = transform;
        position = cachedTransform.position;
    }
    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, dir);
            if (!Physics.SphereCast(ray, BoidSettings.boundsRadius, BoidSettings.collisionAvoidDst, BoidSettings.obstacleMask))
            {
                return dir;
            }
        }

        return forward;
    }
    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * BoidSettings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, BoidSettings.maxSteerForce);
    }
}
 

