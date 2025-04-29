using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu(fileName = "Monster Audio", menuName = "Scriptables/Audio/MonsterAudio", order = 2)]
public class MonsterOneShots : ScriptableObject
{
    // Deklarerar en public variabel för att komma åt ett specifikt FMOD event.
    public EventReference deepWalkerListenEvent;
    public EventReference deepWalkerHuntingEvent;
    public EventReference deepWalkerFoundYouEvent;

    public void MonsterListeningPlay(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(deepWalkerListenEvent,sender);
    }

    public void MonsterHuntingPlay(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(deepWalkerHuntingEvent,sender);
    }

    public void MonsterFoundYouPlay(GameObject sender)
    {
        RuntimeManager.PlayOneShotAttached(deepWalkerFoundYouEvent,sender);
    }
}
