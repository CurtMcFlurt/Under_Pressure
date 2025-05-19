using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Agent_findPath : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Movement spped of the player, default 1000")]
    public float minSpeed;
    [SerializeField]
    [Tooltip("movement speed of player at max")]
    public float maxSpeed;


    [SerializeField]
    [Tooltip("Movement spped of the Path object, default 1000")]
    private float FollowMinSpeed;
    [SerializeField]
    [Tooltip("Movement spped of the Path object, default 1000")]
    private float FollowMaxSpeed;

    private Vector3 startPos;

    [SerializeField]
    [Tooltip("Reference to own rigidbody")]
    private Rigidbody rb3d;

    [Tooltip("Reference to own NavMeshAgent")]
    private NavMeshAgent nma;

    [Tooltip("The closer to 0 the more points in the path for the agent to follow")]
    [Range(.001f, 10)]
    public float pathIntervalLeangth = .1f;

    [SerializeField]
    public List<Vector3> Path = new List<Vector3>();

    [Tooltip("1 is nextPoint, 0 is averagePoint")]
    private Dictionary<Vector3, PointClass> InstancedPoints = new Dictionary<Vector3, PointClass>();
    [SerializeField]
    public float followDist = 0;
    public float maxDist = 5;
    public int maxLookahead;
    public GameObject goal;
    public float curentSpeed;
    public LayerMask LineOfSightLayers;
    public LayerMask BezierLayers;
    public List<Vector3> debugPath;
    public bool RecalculatePath;
    void OnEnable()
    {
        rb3d = GetComponent<Rigidbody>();
        GenerateNavdata();
        GeneratePath();
        startPos = transform.position;
    }

    public void GenerateNavdata()
    {
        InstancedPoints = new Theta_Star().CreatePointsNavMesh(LineOfSightLayers,transform.localScale.x);
        Debug.Log(InstancedPoints.Count + " instanced");
    }


    public void GeneratePath()
    {
        debugPath.Clear();
        Path.Clear();

        // Sample both the start and goal positions to ensure they're on the NavMesh
        Vector3 rawStartPos = VectorFix.ReturnVector3WithGroundHeight(transform.position);
        Vector3 rawGoalPos = VectorFix.ReturnVector3WithGroundHeight(goal.transform.position);

        if (!NavMesh.SamplePosition(rawStartPos, out NavMeshHit startHit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Start position is not on NavMesh.");
            RecalculatePath = true;
            return;
        }

        if (!NavMesh.SamplePosition(rawGoalPos, out NavMeshHit goalHit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Goal position is not on NavMesh.");
            RecalculatePath = true;
            return;
        }

        Vector3 validStartPos = startHit.position;
        Vector3 validGoalPos = goalHit.position;

        // Use spherecast for line of sight check
        if (Physics.SphereCast(validStartPos, 1.25f, (validGoalPos - validStartPos).normalized,
            out RaycastHit whatIHit, (validGoalPos - validStartPos).magnitude, LineOfSightLayers))
        {
            Path = new Theta_Star().GeneratePathPhiStarNavMesh(validStartPos, validGoalPos, LineOfSightLayers, InstancedPoints);
        }
        else
        {
            Path.Add(validStartPos);
            Path.Add(validGoalPos);
        }

        debugPath.AddRange(Path);

        try
        {
            List<Vector3> tempA = new List<Vector3>(Path);
            Path[0] = VectorFix.ReturnVector3WithGroundHeight(Path[0], 0);
            Path[Path.Count - 1] = VectorFix.ReturnVector3WithGroundHeight(Path[Path.Count - 1], 0);
            Path = Bezier.BezirPath(tempA, pathIntervalLeangth, BezierLayers, transform.localScale.x - 0.25f);
        }
        catch
        {
            Debug.LogWarning("Path smoothing failed.");
            RecalculatePath = true;
        }

        PathSegment = 0;
    }

    public void AddPointToCurve(Vector3 position)
    {
        List<Vector3> tempA = new List<Vector3>();
        List<Vector3> tempB = new List<Vector3>();
        tempA.Add(Path[Path.Count - 1]);
        tempA.Add(position);
        tempB = Bezier.BezirPath(tempA, pathIntervalLeangth, BezierLayers, transform.localScale.x);
        tempA.Remove(Path[Path.Count - 1]);
        Path.AddRange(tempA);

    }


    private float waitForTimer = 0;
    private int nudge;
    // Update is called once per frame
    void Update()
    {
        
        if (waitForTimer > 0)
        {
            waitForTimer -= Time.deltaTime;
            return;
        }
      
        if (RecalculatePath)
        {
            var cachedPath = Path;
            int cachedSegment = PathSegment;
            
            GeneratePath();
            if (Path.Count == 0)
            {

                NudgeAwayFromWall(nudge, .5f);
                nudge++;
                Debug.Log("nudge");
                return;
            }
            else nudge = 0;
            RecalculatePath = false;

            // Cache current path and segment
           

            // If new path is valid and not dramatically worse:
            if (Path != null && Path.Count > 1)
            {
                // Optionally: check if first few segments match, then resume from last position
                // Otherwise: reset segment
                PathSegment = 0;
                TValue = 0;
            }
            else
            {
                // If new path fails, fallback to old
                Path = cachedPath;
                PathSegment = cachedSegment;
               
            }

            
        }

        if (PathSegment < Path.Count - 1 && followDist < maxDist && Path.Count != 0) { MoveAlongPath(); } else curentSpeed = 0;
    }
    public int PathSegment;
    private float TValue=0;
    [SerializeField] private float targetLeadDistance = 2.0f; // Desired distance ahead
    private void MoveAlongPath()
    {
        float curSpeed = Mathf.Lerp(maxSpeed, minSpeed, PredictiveMovement());
        curentSpeed = curSpeed;
        TValue += Time.deltaTime * curSpeed;

        // Loop forward until we're far enough ahead or out of path
      
            Vector3 current = Path[PathSegment];
            Vector3 next = Path[PathSegment + 1];

            Vector3 lerpPos = Vector3.Lerp(current, next, TValue);
            Vector3 projectedPos = new Vector3(lerpPos.x, transform.position.y, lerpPos.z);

            // If we're far enough ahead, stop
    
           
                transform.position = projectedPos;
        ApplyRotation((next - transform.position), 8);

            if (TValue >= 1f)
            {
                TValue = 0f;
                PathSegment++;

               
            }
    }

    //private void MoveTowardsFollow()
    //{
    //    Vector3 targetDirection = followObject.transform.position - transform.position;
    //    ApplyRotation(targetDirection, 8);
    //    Vector3 dir = (followObject.transform.position - transform.position);
    //    float tValie = Mathf.Clamp01(followDist / maxDist);
    //    float streangth = Mathf.Lerp(minSpeed, maxSpeed, tValie);
    //    dir *= streangth;
    //    Vector3 currentVelocity = rb3d.linearVelocity;
    //    dir = dir - currentVelocity;
    //    dir = VectorFix.returnVector3With0Y(dir).normalized;
    //    rb3d.AddForce(dir * streangth*20000 * Time.deltaTime);
    //}
    //private void breakMovementToReach()
    //{
    //    Debug.Log("breakMove");

    //    transform.position = Vector3.Lerp(transform.position, followObject.transform.position, Time.deltaTime);
    //}

    public void ApplyRotation(Vector3 targetDirection,float rotationAmmount)
    {
        targetDirection.y = 0; // Ignore vertical for flat rotation
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            float rotationSpeed = rotationAmmount; // Adjust to control rotation speed

            // SmoothStep over time
            float t = Mathf.SmoothStep(0, 1, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
        }
    }



    private float PredictiveMovement()
    {
        //int futurePointsCount = (int)(lookAheadDistance / pathIntervalLeangth);
        //float totalWeights = 0;

        //Used for both

 

       Vector3 nextPoint = Path[PathSegment];

        //-----------

        int maxFuturePointsCount = Mathf.CeilToInt(maxLookahead/pathIntervalLeangth);
        Vector3 curDir = (Path[PathSegment] - Path[PathSegment+1]).normalized;
        Vector3 prevDir = Vector3.zero;

        float totalAngle = 0;
        float angleConstant = 1;

        for (int i = 1; i < maxFuturePointsCount; i++)
        {
            int index = Mathf.Min(PathSegment + i, Path.Count - 1);

            prevDir = curDir;

            try
            {
                if (Path[index + 1] == Path[index])
                {
                    continue;
                }

                curDir = (Path[index + 1] - Path[index]).normalized;
            }
            catch
            {
                curDir = (Path[Path.Count - 1] - Path[Path.Count - 2]).normalized;
            }


            if (prevDir == curDir)
            {
                continue;
            }



            totalAngle += 1-Mathf.Abs(Vector3.Dot(prevDir, curDir));
            if(totalAngle > angleConstant) {break;}

        }

      
       

        return totalAngle;
    }
    public void NudgeAwayFromWall(int wichNudge, float nudgeDistance = 0.2f)
    {
        //Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, LineOfSightLayers);

        //if (hits.Length > 0)
        //{
        //    Collider closest = hits
        //        .Where(c => c is BoxCollider || c is SphereCollider || c is CapsuleCollider || (c is MeshCollider mc && mc.convex))
        //        .OrderBy(h => Vector3.Distance(transform.position, h.ClosestPoint(transform.position)))
        //        .FirstOrDefault();

        //    if (closest != null)
        //    {
        //        Vector3 closestPoint = closest.ClosestPoint(transform.position);
        //        Vector3 away = (transform.position - closestPoint).normalized;

        //        Debug.DrawLine(transform.position, transform.position + away * nudgeDistance, Color.red, 1f);
        //        Debug.LogWarning("Nudging: " + transform.position + " → " + (transform.position + away * nudgeDistance));

        //        transform.position += away * nudgeDistance;
        //        return;
        //    }
        //}

        //// No valid collider found, apply small random nudge
        //transform.position += Random.onUnitSphere * nudgeDistance;
        //Debug.LogWarning("Nudging randomly - no close wall.");
        switch (wichNudge)
        {
            case 0:
                {

                    transform.position = transform.position + Vector3.up * nudgeDistance;
                    break;
                }case 1:
                {

                    transform.position = transform.position + Vector3.forward * nudgeDistance;
                    break;
                }case 2:
                {

                    transform.position = transform.position + Vector3.back*2 * nudgeDistance;
                    break;
                }case 3:
                {

                    transform.position = transform.position + Vector3.left * nudgeDistance;
                    break;
                }case 4:
                {

                    transform.position = transform.position + Vector3.right*2 * nudgeDistance;
                    break;
                }case 5:
                {

                    transform.position = startPos;
                    Debug.LogWarning("resetPos");
                    break;
                }
        }



    }

    public void AddTimeToWait(float time)
    {
        waitForTimer = time;
    }


    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        for (int i = 0; i < debugPath.Count - 1; i++)
        {
            Gizmos.DrawLine(debugPath[i] + Vector3.up * 1.5f, debugPath[i + 1] + Vector3.up * 1.5f);
        }

        //Path
        Gizmos.color = Color.blue;
        for (int i = 0; i < Path.Count - 1; i++)
        {
            Gizmos.DrawLine(Path[i] + Vector3.up * 1.5f, Path[i + 1] + Vector3.up * 1.5f);
        }

        Gizmos.color = Color.white;

     //   Gizmos.DrawCube(followObject.transform.position, new Vector3(1, 2, 1));
   
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position+ rb3d.linearVelocity);
    }
}
