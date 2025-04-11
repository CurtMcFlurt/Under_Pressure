using UnityEngine;

[CreateAssetMenu(fileName = "Sleeping", menuName = "Scriptables/DeepStalker/Sleeping", order = 3)]
public class Sleeping : ScriptableBehaviour
{
    public float restTime = 5;
    public float sleepingMovementSpeed=5;
    public int sleepingHearingRange=2;
    public override void Behave(DeepWalkerLogic logic)
    {
        base.Behave(logic);
        FindSleep(logic);
    }
    public void FindSleep(DeepWalkerLogic logic)
    {
        logic.FindOptimalHex(0);
        logic.currentHexTarget = logic.optimalSafety;
        logic.pathfinder.maxSpeed = sleepingMovementSpeed;
        logic.hearingRange = sleepingHearingRange;
        logic.sleepTime=restTime;
        logic.readyForSleep = true;
    }

}
