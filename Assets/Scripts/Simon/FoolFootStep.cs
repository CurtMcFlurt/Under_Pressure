using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FoolFootStep : MonoBehaviour
{
    [SerializeField]
    private EventReference playerSteppingEvent;
    
    public void FoolRunning()
        {
            Debug.Log("Running");
            EventInstance eventInstance = RuntimeManager.CreateInstance(playerSteppingEvent);
            RuntimeManager.AttachInstanceToGameObject(eventInstance, this.transform);
            eventInstance.setParameterByName("Strenght", 0);
            eventInstance.start();
            eventInstance.release();
        }
    
    public void FoolWalking()
        {
            Debug.Log("Walking");
            EventInstance eventInstance = RuntimeManager.CreateInstance(playerSteppingEvent);
            RuntimeManager.AttachInstanceToGameObject(eventInstance, this.transform);
            eventInstance.setParameterByName("Strenght", 1);
            eventInstance.start();
            eventInstance.release();
        }
    
    public void FoolCrouching()
    {
        Debug.Log("Crouching");
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerSteppingEvent);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, this.transform);
        eventInstance.setParameterByName("Strenght", 2);
        eventInstance.start();
        eventInstance.release();
    }
}
    
