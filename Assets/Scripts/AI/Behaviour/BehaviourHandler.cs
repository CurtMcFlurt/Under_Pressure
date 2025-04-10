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
    public ScriptableBehaviour roaming;
    public ScriptableBehaviour debug_Failure;

    public ScriptableBehaviour TimeToChange(ActiveBehaviour changeBehaviour)
    {
        wichBehaviour = changeBehaviour;
        switch (changeBehaviour)
        {
            case ActiveBehaviour.feeding:
            {
                    return (feeding);
             
            } 
            case ActiveBehaviour.sleeping:
            {
                    return (sleeping);
            }
            case ActiveBehaviour.tracking:
            {
                    return (tracking);
            } 
            case ActiveBehaviour.hunting:
            {
                    return (hunting);
            } 
            case ActiveBehaviour.scouting:
            {
                    return (scouting);
            }   case ActiveBehaviour.roaming:
            {
                    return (roaming);
            }
        }

        Debug.LogError("Behaviour not Recognised");
        return (debug_Failure);

    }
   
}
