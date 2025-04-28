using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu(fileName = "MonsterOneShots", menuName = "Scriptable Objects/MonsterOneShots")]
public class MonsterOneShots : ScriptableObject
{
    // Deklarerar en public variabel för att komma åt ett specifikt FMOD event.
    public EventReference deepWalkerListenEvent;
    public EventReference deepWalkerHuntingEvent;
    public EventReference deepWalkerFoundYouEvent;

    public void MonsterListeningPlay()
    {
        RuntimeManager.PlayOneShot(deepWalkerListenEvent);
    }

    public void MonsterHuntingPlay()
    {
        RuntimeManager.PlayOneShot(deepWalkerHuntingEvent);
    }

    public void MonsterFoundYouPlay()
    {
        RuntimeManager.PlayOneShot(deepWalkerFoundYouEvent);
    }
}
