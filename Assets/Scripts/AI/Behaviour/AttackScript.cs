using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public SphereCollider hitPlayerCol;
    public GameObject playerMurderSphere;
    public float attackTimer;
    public bool isAttacking;
    private DeepWalkerLogic logic;
    private Agent_findPath pather;
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
            return;
        }
        isAttacking = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.tag == "Player" && logic.TrackingObject==null)
        {
          if( !other.GetComponent<PlayerDeath>().IsDead)
            {
                logic.TrackingObject = other.gameObject;
                logic.AngerInfluence(1);
                logic.AlertnessInfluence(1);
            }
   
        }
    }

}
