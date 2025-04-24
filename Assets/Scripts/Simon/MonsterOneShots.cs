using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterOneShots", menuName = "Scriptable Objects/MonsterOneShots")]
public class MonsterOneShots : ScriptableObject
{

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
