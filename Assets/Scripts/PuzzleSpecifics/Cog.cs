using UnityEngine;

public class Cog : MonoBehaviour
{
    public GameObject ActiveSocket;
    public Vector3 SpawnLocation;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(ActiveSocket != null)
        {
            transform.position = ActiveSocket.transform.position;

        }
    }

    public void Reset()
    {
        ActiveSocket = null;
        transform.position = SpawnLocation;
    }
}
