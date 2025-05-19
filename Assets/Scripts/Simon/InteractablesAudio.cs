using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu(fileName = "InteractablesAudio", menuName = "Scriptable Objects/InteractablesAudio")]
public class InteractablesAudio : ScriptableObject
{
    public EventReference pickupCogEvent;

    public void PickupCogPlay()
    {
        RuntimeManager.PlayOneShot(pickupCogEvent);
    }
}
