using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu(fileName = "Monster Audio", menuName = "Scriptables/Audio/MonsterAudio", order = 2)]
public class MonsterOneShots : ScriptableObject
{
    // Deklarerar en public variabel för att komma åt ett specifikt FMOD event.
    public EventReference deepWalkerListenEvent;
    public EventReference deepWalkerFoundYouEvent;
    public EventReference deepWalkerKillEvent;

    public void MonsterListeningPlay(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(deepWalkerListenEvent, sender);
    }

    public void MonsterFoundYouPlay(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(deepWalkerFoundYouEvent, sender);
    }

    public void MonsterKillPlay(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(deepWalkerKillEvent, sender);
    }
}
