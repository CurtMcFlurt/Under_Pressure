using UnityEngine;

public class WeightChangers : MonoBehaviour
{
    public float falloff;
    public int range;
    public HeatMapValues myHeat;
    public bool OneOff = false;
    public bool Player = false;
    public HexCell myHex;
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(myHeat.food, myHeat.safety, myHeat.sound);

        Gizmos.DrawSphere(transform.position, 1);
    }


}
