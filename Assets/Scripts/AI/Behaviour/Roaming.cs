using UnityEngine;
[CreateAssetMenu(fileName = "Roam", menuName = "Scriptables/DeepStalker/Roaming", order = 2)]
public class Roaming : ScriptableBehaviour
{
    public float roamingMovementSpeed = 5;
    public int roamingHearingRange = 4;
    public override void Behave(DeepWalkerLogic logic)
    {
        base.Behave(logic);
        FindCorrectHex(logic);
    }


    private void FindCorrectHex(DeepWalkerLogic logic)
    {
        logic.optimalRoaming = logic.FindOptimalHex(2);
        logic.currentHexTarget = logic.optimalRoaming;
        logic.updateGoal(HexMath.Axial2World(logic.optimalRoaming, logic.WeightMap.cellSize));
        logic.pathfinder.maxSpeed = roamingMovementSpeed;
        logic.hearingRange = roamingHearingRange;

    }

}
