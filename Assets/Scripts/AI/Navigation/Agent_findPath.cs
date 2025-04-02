using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Agent_findPath : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Movement spped of the player, default 1000")]
    private float minSpeed;
    [SerializeField]
    [Tooltip("Movement spped of the player, default 1000")]
    private float maxSpeed;

    [Tooltip("Reference to own rigidbody")]
    private Rigidbody rb3d;

    [Tooltip("Reference to own NavMeshAgent")]
    private NavMeshAgent nma;

    [Tooltip("The closer to 0 the more points in the path for the agent to follow")]
    [Range(.001f, 10)]
    public float pathIntervalLeangth = .1f;

    [SerializeField]
    private List<Vector3> Path = new List<Vector3>();

    [Tooltip("1 is nextPoint, 0 is averagePoint")]

    public int maxLookahead;
    public GameObject goal;
    public float curentSpeed;
    public LayerMask LineOfSightLayers;
    public List<Vector3> debugPath;
    void Start()
    {
        rb3d = GetComponent<Rigidbody>();
      
        
        GeneratePath();
    }


    public void GeneratePath()
    {
        Path.Clear();
        Path = new Theta_Star().GeneratePathPhiStar(VectorFix.returnVector3With1Y(transform.position), goal.transform.position, LineOfSightLayers);
        debugPath.AddRange(Path);
        List<Vector3> tempA = new List<Vector3>();
        tempA.AddRange(Path);
        Path = Bezier.BezirPath(tempA, pathIntervalLeangth, LineOfSightLayers, transform.localScale.x);
        PathSegment = 0;
    }


    // Update is called once per frame
    void Update()
    {
        if (PathSegment < Path.Count-2) MoveAlongPath();
    }
    public int PathSegment;
    private float TValue=0;
    private void MoveAlongPath()
    {
       var curSpeed=Mathf.Lerp(maxSpeed,minSpeed,PredictiveMovement());
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
        transform.position =new Vector3(lerpPos.x,transform.position.y,lerpPos.z);
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


            Debug.Log(Vector3.Dot(prevDir, curDir) + " dot" + i);

            totalAngle += 1-Mathf.Abs(Vector3.Dot(prevDir, curDir));
            if(totalAngle > angleConstant) {break;}

        }

      
       
       Debug.Log(totalAngle+" angle");

        return totalAngle;
    }





    public void OnDrawGizmos()
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

      
    }
}
