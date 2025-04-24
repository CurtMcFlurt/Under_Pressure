using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterOneShots", menuName = "Scriptable Objects/MonsterOneShots")]
public class MonsterOneShots : ScriptableObject
{

    public EventReference deepWalkerListenEvent;

    public void MonsterListeningPlay()
    {
        RuntimeManager.PlayOneShot(deepWalkerListenEvent);
    }
}
