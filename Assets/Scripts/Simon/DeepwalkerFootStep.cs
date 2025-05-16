using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class DeepwalkerFootStep : MonoBehaviour
{
    
    [SerializeField]
    private EventReference monsterSteppingEvent;

    public void MonsterStepping()
    {
        Debug.Log("Stepping");
        EventInstance eventInstance = RuntimeManager.CreateInstance(monsterSteppingEvent);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, this.transform);
        eventInstance.start();
        eventInstance.release();
    }
}
