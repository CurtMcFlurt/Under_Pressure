using UnityEngine;

public class AttackOrbHandler : MonoBehaviour
{
    public float timeAttackIsActive=.25f;
    public float timeToDeployAttack = .1f;
    public SphereCollider attackCollider;
    public LayerMask playerLayer;
    public MeshRenderer myRender;
    public bool KilledPlayer = false;
    private float LifeTimer;
    private float waitFor;
    public void OnEnable()
    {
        LifeTimer = timeAttackIsActive;
        waitFor = timeToDeployAttack;
    }

    private void FixedUpdate()
    {
        if (waitFor <= 0)
        {
            attackCollider.enabled = true;
            myRender.enabled = true;
            if (LifeTimer<= 0)
            {
                attackCollider.enabled = false;
                myRender.enabled = false;
                gameObject.SetActive(false);
            }
            else LifeTimer -= Time.fixedDeltaTime;
        }
        else waitFor -= Time.fixedDeltaTime;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerDeath>().Die();
            KilledPlayer = true;
        }
    }



}
