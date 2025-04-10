using UnityEngine;
[CreateAssetMenu(fileName = "Behaviour", menuName = "Scriptables/DeepStalker/Scouting", order = 2)]
public class Scouting :ScriptableBehaviour
{
    public float scoutingMovementSpeed=5;
    public int scoutingHearingRange=4;

    public override void Behave(DeepWalkerLogic logic)
    {
        base.Behave(logic);
        FindCorrectHex(logic);
    }




    private void FindCorrectHex(DeepWalkerLogic logic)
    {
        logic.currentHexTarget = logic.probableVictimPosition;
        logic.updateGoal(HexMath.Axial2World(logic.optimalScouting, logic.WeightMap.cellSize));
        logic.pathfinder.maxSpeed = scoutingMovementSpeed;
        logic.hearingRange = scoutingHearingRange;

    }
}
