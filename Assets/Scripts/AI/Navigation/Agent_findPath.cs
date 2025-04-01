using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent_findPath : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Movement spped of the player, default 1000")]
    private float movSpeed;

    [Tooltip("Reference to own rigidbody")]
    private Rigidbody rb3d;

    [Tooltip("Reference to own NavMeshAgent")]
    private NavMeshAgent nma;

    [Tooltip("The closer to 0 the more points in the path for the agent to follow")]
    [Range(.001f, 10)]
    public float pathIntervalLeangth = .1f;

    [SerializeField]
    private List<Vector3> Path = new List<Vector3>();

    public GameObject goal;

    public LayerMask LineOfSightLayers;
    public List<Vector3> debugPath;
    void Start()
    {
        rb3d = GetComponent<Rigidbody>();
      
        if (movSpeed <= 0)
        {
            movSpeed = 1000;
        }
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

    }


    // Update is called once per frame
    void Update()
    {
        
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
