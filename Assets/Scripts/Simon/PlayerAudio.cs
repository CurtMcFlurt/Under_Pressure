using UnityEngine;
using FMOD.Studio;
using FMODUnity;
[CreateAssetMenu(fileName = "Player Audio", menuName = "Scriptables/Audio/PlayerAudio", order = 1)]
public class PlayerAudio : ScriptableObject
{
    [SerializeField]
    private EventReference playerFootstepEvent;

    private void PlayerFootstepAudio(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(playerFootstepEvent, sender);
 
    }

    public void TestINGAnim()
    {
        Debug.Log("Happens");
    }
}
