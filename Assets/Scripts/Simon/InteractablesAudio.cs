using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu(fileName = "InteractablesAudio", menuName = "Scriptable Objects/InteractablesAudio")]
public class InteractablesAudio : ScriptableObject
{
    public EventReference pickupCogEvent;
    public EventReference completePuzzleEvent;

    public void PickupCogPlay()
    {
        RuntimeManager.PlayOneShot(pickupCogEvent);
    }
    
    public void CompletePuzzlePlay()
    {
        RuntimeManager.PlayOneShot(completePuzzleEvent);
    }
}
