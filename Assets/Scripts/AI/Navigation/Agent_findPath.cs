using System.Collections.Generic;
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
    public GameObject followObject;
    public float curentSpeed;
    public LayerMask LineOfSightLayers;
    public LayerMask BezierLayers;
    public List<Vector3> debugPath;
    public bool RecalculatePath;
    void OnEnable()
    {
        rb3d = GetComponent<Rigidbody>();
        followObject.transform.position = transform.position;
        GenerateNavdata();
        GeneratePath();
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
        RaycastHit whatIHit;
        if (Physics.SphereCast(VectorFix.ReturnVector3WithGroundHeight(transform.position), 1.25f, (VectorFix.ReturnVector3WithGroundHeight(goal.transform.position) - VectorFix.ReturnVector3WithGroundHeight(transform.position)).normalized, out whatIHit, ((VectorFix.ReturnVector3WithGroundHeight(goal.transform.position) - VectorFix.ReturnVector3WithGroundHeight(transform.position))).magnitude, LineOfSightLayers))
        {
            Path = new Theta_Star().GeneratePathPhiStarNavMesh(VectorFix.ReturnVector3WithGroundHeight(transform.position), VectorFix.ReturnVector3WithGroundHeight(goal.transform.position), LineOfSightLayers, InstancedPoints);

        }
        else
        {

            Path.Add(VectorFix.ReturnVector3WithGroundHeight(transform.position));
            Path.Add(VectorFix.ReturnVector3WithGroundHeight(goal.transform.position));

        }

        debugPath.AddRange(Path);
        List<Vector3> tempA = new List<Vector3>();
        tempA.AddRange(Path);
        try
        {

            Path[0] = VectorFix.ReturnVector3WithGroundHeight(Path[0], 0);
            Path[Path.Count - 1] = VectorFix.ReturnVector3WithGroundHeight(Path[Path.Count - 1], 0);
            Path = Bezier.BezirPath(tempA, pathIntervalLeangth, BezierLayers, transform.localScale.x - .25f);

        }
        catch
        {
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




    // Update is called once per frame
    void Update()
    {
        followDist = (transform.position - followObject.transform.position).magnitude;
        if (PathSegment < Path.Count-1 && followDist<maxDist) MoveAlongPath();
        if (RecalculatePath)
        {
            RecalculatePath=false;
            GeneratePath();
        }
        if(followDist>.6f) MoveTowardsFollow();
        if (followDist > maxDist - 1 && rb3d.linearVelocity.magnitude < .25f) breakMovementToReach();
    }
    public int PathSegment;
    private float TValue=0;
    private void MoveAlongPath()
    {
        var curSpeed=Mathf.Lerp(FollowMaxSpeed,FollowMinSpeed,PredictiveMovement());
        curentSpeed=curSpeed;
        TValue += Time.deltaTime * curSpeed;
        if (TValue > 1)
        {
            TValue -= 1;
            if (PathSegment < Path.Count-2)
            {
                PathSegment++;
            }else TValue = 1;
        }
       
        Vector3 acvtiveInterval = Path[PathSegment];
        Vector3 activeTarget = Path[PathSegment + 1];
    
        Vector3 lerpPos= Vector3.Lerp(acvtiveInterval, activeTarget, TValue);
        followObject.transform.position =new Vector3(lerpPos.x, transform.position.y,lerpPos.z);
        followDist=(followObject.transform.position-transform.position).magnitude;
    }

    private void MoveTowardsFollow()
    {
        Vector3 targetDirection = followObject.transform.position - transform.position;
        targetDirection.y = 0; // Ignore vertical for flat rotation
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            float rotationSpeed = 25f; // Adjust to control rotation speed

            // SmoothStep over time
            float t = Mathf.SmoothStep(0, 1, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
        }
        Vector3 dir = (followObject.transform.position - transform.position);
        float tValie = Mathf.Clamp01(followDist / maxDist);
        float streangth = Mathf.Lerp(minSpeed, maxSpeed,TValue);
        dir *= streangth;
        Vector3 currentVelocity = rb3d.linearVelocity;
        dir = dir - currentVelocity;
        dir = VectorFix.returnVector3With0Y(dir).normalized;
        rb3d.AddForce(dir * streangth*20000 * Time.deltaTime);
    }
    private void breakMovementToReach()
    {
        Debug.Log("breakMove");

        transform.position = Vector3.Lerp(transform.position, followObject.transform.position, Time.deltaTime);
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

        Gizmos.DrawCube(followObject.transform.position, new Vector3(1, 2, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position+ rb3d.linearVelocity);
    }
}
