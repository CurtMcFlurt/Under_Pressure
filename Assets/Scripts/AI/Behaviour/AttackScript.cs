using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class AttackScript : NetworkBehaviour
{
    public SphereCollider hitPlayerCol;
    public GameObject playerMurderSphere;
    public float attackTimer;
    public NetworkVariable< bool> isAttacking;
    private DeepWalkerLogic logic;
    private Agent_findPath pather;
    public int maxAttacks=7;
    private int currentAttacks;
    private float timer = 0;
    private float waitfor = 30;
    private AttackOrbHandler attackOrbHandler;
    public void OnEnable()
    {
        logic = GetComponent<DeepWalkerLogic>();
        pather = GetComponent<Agent_findPath>();
        attackOrbHandler =playerMurderSphere.GetComponent<AttackOrbHandler>();
        attackTimer = attackOrbHandler.timeAttackIsActive + attackOrbHandler.timeToDeployAttack;
    }
    public void FixedUpdate()
    {
        if (!HasAuthority)
        {
            playerMurderSphere.SetActive(isAttacking.Value);
            return;
        }

        if (logic.TrackingObject != null && hitPlayerCol.bounds.Contains(logic.TrackingObject.transform.position))
        {
            isAttacking.Value = true;
            playerMurderSphere.SetActive(true);
            pather.ApplyRotation(logic.TrackingObject.transform.position - transform.position, 125);
            currentAttacks++;
            if (currentAttacks > maxAttacks) 
            {
                logic.TrackingObject = null;
                hitPlayerCol.enabled = false;
                logic.mood.anger = 0;
                Debug.Log("Reached Max Attacks");
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
        isAttacking.Value = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!HasAuthority)
        {
            return;
        }
        Debug.Log(other.name);
        if (other.tag == "Player" && logic.TrackingObject==null)
        {
          if( !other.GetComponent<PlayerDeath>().IsDead && other.GetComponent<PlayerDeath>() != null)
            {
                Debug.Log("foundYOU");
                logic.TrackingObject = other.gameObject;
                logic.AngerInfluence(1);
                logic.AlertnessInfluence(1);
            }
   
        }
    }

}
