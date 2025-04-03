using UnityEngine;

public class WeightChangers : MonoBehaviour
{
    public float falloff;
    public int range;
    public HeatMapValues myHeat;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(myHeat.food, myHeat.safety, myHeat.sound);

        Gizmos.DrawSphere(transform.position, (range*1.5f) / (1+falloff));
    }


}
