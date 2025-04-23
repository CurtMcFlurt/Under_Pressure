using UnityEngine;

public class BoidFollowTarget : MonoBehaviour
{
    public LayerMask fishbowlLayer;
    public float sphereCheckRadius=5;
    private Collider fishBowlCollider;
    public bool checkIfInside; 
   private BoidManager boidManager;
    public int boidCounter;
    public GameObject huntTransform;
    public int MaximumFishAround=75;
    public float TimeTillDeath=3;
    private float currentDeath = 0;
    private PlayerDeath myDeath;

    private void OnDisable()
    {
        if (checkIfInside) {
          
            boidManager.targets.Remove(huntTransform);
        }
    }
    private void OnEnable()
    {
        myDeath = GetComponent<PlayerDeath>();
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

        if (fishBowlCollider != null && !myDeath.IsDead)
        {
            fishBowlCollider.gameObject.TryGetComponent<BoidManager>(out var M);

            boidManager = M;
            if (fishBowlCollider.bounds.Contains(transform.position))
            {
                checkIfInside = true;

                boidManager.targets.Add(huntTransform);

                foreach (var c in boidManager.boids)
                {
                    if ((c.position - transform.position).magnitude < sphereCheckRadius)
                    {
                        boidCounter++;
                    }
                }
                if (boidCounter > MaximumFishAround)
                {
                    currentDeath += Time.deltaTime;
                }
                if (currentDeath > TimeTillDeath)
                {
                    myDeath.Die();
                }
            }
            else if (checkIfInside)
            {

                checkIfInside = false;
                Debug.LogWarning("Left The Box");
                boidManager.targets.Remove(huntTransform);

                boidManager = null;
            }

        }
        else if(currentDeath>0) currentDeath -= Time.deltaTime;
    }
}
