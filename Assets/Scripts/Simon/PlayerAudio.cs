using UnityEngine;
using FMOD.Studio;
using FMODUnity;
[CreateAssetMenu(fileName = "Player Audio", menuName = "Scriptables/Audio/PlayerAudio", order = 1)]
public class PlayerAudio : ScriptableObject
{
    [SerializeField]
    private EventReference playerFootstepEvent;
}
