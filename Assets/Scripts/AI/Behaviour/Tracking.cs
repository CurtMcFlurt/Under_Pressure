using UnityEngine;
[CreateAssetMenu(fileName = "Tracking", menuName = "Scriptables/DeepStalker/tracking", order = 4)]
public class Tracking :ScriptableBehaviour
{
    public override void Behave(DeepWalkerLogic logic)
    {
        base.Behave(logic);
    }

    //when a player has made a noise within the beasts hearing range it reacts, this is checked by checking the hex of the WeightChanger 
    //When a player has made a noise within the beasts hearing range 3 times it becomes sure of the persons location and locks on
    //a locked on player is hunted down untill their demise, theoretically they could escape if they stayed out of hearing range long enough but that would rarely happen.
    //a player making a noise is counted per hexagon and each hexagon has a cd, a player screaming and running past the deepwalker would trigger its hunt,a player who screams and freezes wouldnt.
}
