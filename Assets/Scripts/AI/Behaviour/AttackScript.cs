using System.Threading;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public SphereCollider hitPlayerCol;
    public GameObject playerMurderSphere;
    public float attackTimer;
    public bool isAttacking;
    private DeepWalkerLogic logic;
    private Agent_findPath pather;
    public int maxAttacks=7;
    private int currentAttacks;
    private float timer = 0;
    private float waitfor = 3;
    public void OnEnable()
    {
        logic = GetComponent<DeepWalkerLogic>();
        pather = GetComponent<Agent_findPath>();
        var hand = GetComponent<AttackOrbHandler>();
        attackTimer = hand.timeAttackIsActive + hand.timeToDeployAttack;
    }
    public void FixedUpdate()
    {
       
        if (logic.TrackingObject != null && hitPlayerCol.bounds.Contains(logic.TrackingObject.transform.position))
        {
            isAttacking = true;
            playerMurderSphere.SetActive(true);
            pather.ApplyRotation(logic.TrackingObject.transform.position - transform.position, 25);
            currentAttacks++;
            if (currentAttacks > maxAttacks) 
            {
                logic.TrackingObject = null;
                hitPlayerCol.enabled = false;
            }
            return;
        }


        if (hitPlayerCol.enabled == false)
        {
            timer += Time.fixedDeltaTime;
            if (timer > waitfor) {
                hitPlayerCol.enabled = true;
                timer= 0;
                currentAttacks = 0;
                    
                    }
        }
        isAttacking = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.tag == "Player" && logic.TrackingObject==null)
        {
          if( !other.GetComponent<PlayerDeath>().IsDead && other.GetComponent<PlayerDeath>() != null)
            {
                logic.TrackingObject = other.gameObject;
                logic.AngerInfluence(1);
                logic.AlertnessInfluence(1);
            }
   
        }
    }

}
