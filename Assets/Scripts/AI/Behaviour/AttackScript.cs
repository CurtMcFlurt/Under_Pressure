using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public SphereCollider hitPlayerCol;
    public GameObject playerMurderSphere;
    public float attackTimer;
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
            pather.AddTimeToWait(attackTimer);
            playerMurderSphere.SetActive(true);
            return;
        }
        
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
