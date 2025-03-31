using UnityEngine;

public class TurnOffInWater : MonoBehaviour
{
    public BoidFollowTarget playerTarget;
    
    public Collider Collider;
    // Update is called once per frame
    void Update()
    {
    if(playerTarget != null)
        {
            if (playerTarget.checkIfInside)
            {
                Collider.enabled = false;
            }else Collider.enabled = true;
        }    
    }
}
