using UnityEngine;
[CreateAssetMenu(fileName = "Hunting", menuName = "Scriptables/DeepStalker/Hunting", order =5)]
public class Hunting : ScriptableBehaviour
{
    public float huntingMovementSpeed = 5;
    public int huntingHearingRange = 5;
    public override void Behave(DeepWalkerLogic logic)
    {
        base.Behave(logic);

    }
    //hunting will most likely be a bit complicated, DeepWalkerLogic needs to continously update untill a hex containing a player makes a noise, or the owner of the noise weight adder is a 
    //person, when that happens it goes from hunting to chasing wich means that it will be too late for whomever it locked onto, it will kill that person, eventually.
    private void hunt(DeepWalkerLogic logic)
    {
        logic.pathfinder.maxSpeed = huntingMovementSpeed;
        logic.hearingRange = huntingHearingRange;
    }

}
