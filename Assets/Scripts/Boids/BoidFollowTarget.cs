using UnityEngine;

public class BoidFollowTarget : MonoBehaviour
{
    public LayerMask fishbowlLayer;
    public float sphereCheckRadius=5;
    private Collider fishBowlCollider;
    public bool checkIfInside; 
   private BoidManager boidManager;
    public int boidCounter;
    private void OnDisable()
    {
        if (checkIfInside) {
          
            boidManager.targets.Remove(this.gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
       var hits= Physics.OverlapSphere(transform.position, sphereCheckRadius,fishbowlLayer);
        boidCounter = 0;
        foreach(Collider c in hits)
        {
            if (c == fishBowlCollider) continue;
            if(fishBowlCollider == null || (c.ClosestPoint(transform.position) - transform.position).magnitude < (fishBowlCollider.ClosestPoint(transform.position) - transform.position).magnitude)
            {
                fishBowlCollider = c;
            }
        }

        if (fishBowlCollider != null)
        {
            fishBowlCollider.gameObject.TryGetComponent<BoidManager>(out var M);
            boidManager = M;
            if (fishBowlCollider.bounds.Contains(transform.position))
            {
                checkIfInside = true;

                boidManager.targets.Add(this.gameObject);

                foreach(var c in boidManager.boids)
                {
                    if ((c.position - transform.position).magnitude < sphereCheckRadius)
                    {
                        boidCounter++;
                    }
                }

            }
            else if (checkIfInside) { 
            
            checkIfInside= false;
                boidManager.targets.Remove(this.gameObject);
                boidManager=null;
            }

        }
    }
}
