using UnityEngine;
public enum ActiveBehaviour
{
    scouting,
    sleeping,
    feeding,
    tracking,
    hunting,
    roaming
}
public class BehaviourHandler : MonoBehaviour
{

    public ActiveBehaviour wichBehaviour;

    public ScriptableBehaviour scouting;
    public ScriptableBehaviour sleeping;
    public ScriptableBehaviour feeding;
    public ScriptableBehaviour tracking;
    public ScriptableBehaviour hunting;
   
}
