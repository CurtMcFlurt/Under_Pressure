using UnityEngine;

public class Cog : MonoBehaviour
{
    public GameObject ActiveSocket;
    public Vector3 SpawnLocation;
    public LayerMask socketLayer;
    private Rigidbody rigid;
    public SocketInteracting mySocket;
   
    void OnEnable()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(ActiveSocket != null)
        {
            transform.position =new Vector3( ActiveSocket.transform.position.x,ActiveSocket.transform.position.y,transform.position.z);
            rigid.useGravity = false;
        }
        else rigid.useGravity = true;
    }

    public void Reset()
    {
        ActiveSocket = null;
        transform.position = SpawnLocation;
    }
    public bool IsOnAvailableSocket()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, transform.localScale.magnitude * 0.5f, socketLayer);

        foreach (var hit in hits)
        {
            SocketInteracting socket = hit.GetComponent<SocketInteracting>();
            if (socket != null)
            {
                mySocket = socket;
                Debug.Log("foundSocket");
                return true;
            }
        }

        return false;
    }
}
