using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[CreateAssetMenu(fileName = "Player Audio", menuName = "Scriptables/Audio/PlayerAudio", order = 1)]
public class PlayerAudio : ScriptableObject
{
    public EventReference playerFootstepEvent;
    public EventReference playerDieEvent;

    public void PlayerFootstepWalking(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(playerFootstepEvent, sender);
 
    }

    public void PlayerDies(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(playerDieEvent, sender);
    }

    public void TestINGAnim()
    {
        Debug.Log("Happens");
    }
    
    
}
