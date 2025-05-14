using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public bool debugDie = false;
    public bool debugRespawn = false;
    public bool IsDead = false;

    public Vector3 debugRespawnPoint;
    private CapsuleCollider myCollider;
    private float originalHeight;
    private  float myTimer;
    public float deathTimer;
    public void OnEnable()
    {
        debugRespawnPoint = transform.position;
        debugRespawnPoint += Vector3.up * 3;
        if (myCollider == null) myCollider = GetComponent<CapsuleCollider>();
        originalHeight = myCollider.height;
    }

    public void Update()
    {
        if (debugDie && !IsDead)
        {
            debugDie = false;
            Die();
        }
        if(debugRespawn && IsDead)
        {
            debugRespawn = false;
            Revive(debugRespawnPoint);
        }
      
            GetComponent<BoidFollowTarget>().enabled = !IsDead;
        if (IsDead)
        {
            myTimer += Time.deltaTime;
            if (myTimer > deathTimer)
            {
                Revive(debugRespawnPoint);
            }
        }
        
    }
    public void Die()
    {
        IsDead = true;
        myCollider.height = .5f;
        myCollider.center = VectorFix.returnVector3With1Y(myCollider.center);
    }

    public void Revive(Vector3 respawnPoint)
    {
        myTimer=0;
        IsDead = false;
        myCollider.height = originalHeight;
        myCollider.center = VectorFix.returnVector3With0Y(myCollider.center);
        transform.position = respawnPoint;
    }

}
