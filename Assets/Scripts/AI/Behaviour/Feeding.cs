using UnityEngine;

[CreateAssetMenu(fileName = "Feeding", menuName = "Scriptables/DeepStalker/Feeding", order = 4)]
public class Feeding : ScriptableBehaviour
{
    public float feedTime = 5;
    public float feedingMovementSpeed = 5;
    public int feedingHearingRange = 3;
    public override void Behave(DeepWalkerLogic logic)
    {
        base.Behave(logic);
        feed(logic);
    }

    private void feed(DeepWalkerLogic logic)
    {
        logic.FindOptimalHex(1);
        logic.currentHexTarget = logic.optimalFood;
        logic.pathfinder.maxSpeed = feedingMovementSpeed;
        logic.hearingRange = feedingHearingRange;
        logic.readyForEating = true;
        logic.feedTime = feedTime;
      
    }


}
