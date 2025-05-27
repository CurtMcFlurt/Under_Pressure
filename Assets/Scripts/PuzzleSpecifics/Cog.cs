using UnityEngine;
[ExecuteAlways]
public class Cog : MonoBehaviour
{
    public GameObject ActiveSocket;
    public Vector3 SpawnLocation;
    public LayerMask socketLayer;
    private Rigidbody rigid;
    public SocketInteracting mySocket;
    [Range(1, 100)]
    public float Size=25;
    void OnEnable()
    {
        rigid = GetComponent<Rigidbody>();
        SpawnLocation = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(ActiveSocket != null)
        {
            //transform.position =new Vector3( ActiveSocket.transform.position.x,ActiveSocket.transform.position.y,transform.position.z);
            transform.position =new Vector3( transform.position.x,ActiveSocket.transform.position.y,ActiveSocket.transform.position.z);
            rigid.useGravity = false;
        }
        else rigid.useGravity = true;



    }
    private void Update()
    {

        transform.localScale = new Vector3(transform.localScale.x,Size / 25,  Size / 25);

    }

    public void Reset()
    {
        ActiveSocket = null;
        transform.position = SpawnLocation;
    }
    public bool IsOnAvailableSocket()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, transform.localScale.magnitude * 0.5f, socketLayer);

        SocketInteracting closestSocket = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (var hit in hits)
        {
            SocketInteracting socket = hit.GetComponent<SocketInteracting>();
            if (socket != null && socket.activeCog == null)
            {
                float distanceSqr = (socket.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestSocket = socket;
                    closestDistanceSqr = distanceSqr;
                }
            }
        }

        if (closestSocket != null)
        {
            mySocket = closestSocket;
            Debug.Log("Found closest socket");
            return true;
        }

        return false;
    }
}
