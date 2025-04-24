using UnityEngine;
using FMODUnity;
public class FootstepsTest : MonoBehaviour
{
    [SerializeField] private EventReference playerfootsteps;
    
    bool playerismoving;
    public float walkingspeed;
    


    void Update () 
    {
        if (Input.GetAxis ("Vertical") >= 0.01f || Input.GetAxis ("Horizontal") >= 0.01f || Input.GetAxis ("Vertical") <= -0.01f || Input.GetAxis ("Horizontal") <= -0.01f) 
        {
            //Debug.Log ("Player is moving");
            playerismoving = true;
        } 
        else if (Input.GetAxis ("Vertical") == 0 || Input.GetAxis ("Horizontal") == 0) 
        {
            //Debug.Log ("Player is not moving");
            playerismoving = false;
        }
    }


    void CallFootsteps()
    {
        if (playerismoving == true)
        {
            RuntimeManager.PlayOneShot(playerfootsteps);
        }
    }


    void Start ()
    {
        InvokeRepeating ("CallFootsteps", 0, walkingspeed);
       
    }
        

    void OnDisable ()
    {
        playerismoving = false;
    }
}