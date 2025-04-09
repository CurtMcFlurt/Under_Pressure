using UnityEngine;
[CreateAssetMenu(fileName = "Behaviour", menuName = "Scriptables/DeepStalker/Debug_Nothing", order = 1)]
public class ScriptableBehaviour : ScriptableObject
{
    public float tickrateDrowsy=1;
    public float tickrateHunger=1;
    public float tickrateAlert=1;

    public virtual void Behave(DeepWalkerLogic logic)
    {

    }
}
