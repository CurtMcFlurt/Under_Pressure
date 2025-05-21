using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class AttackScript : NetworkBehaviour
{
    public SphereCollider hitPlayerCol;
    public GameObject playerMurderSphere;
    public float attackTimer;
    public NetworkVariable<bool> isAttacking;
    private DeepWalkerLogic logic;
    private Agent_findPath pather;
    public int maxAttacks = 7;
    private int currentAttacks;
    private float timer = 0;
    private float waitfor = 2.5f;
    private AttackOrbHandler attackOrbHandler;
    public LayerMask playerLayer;

    private void OnEnable()
    {
        logic = GetComponent<DeepWalkerLogic>();
        pather = GetComponent<Agent_findPath>();
        attackOrbHandler = playerMurderSphere.GetComponent<AttackOrbHandler>();
        attackTimer = attackOrbHandler.timeAttackIsActive + attackOrbHandler.timeToDeployAttack;
    }

    private void FixedUpdate()
    {
        // Skip logic if not authoritative
        if (!HasAuthority)
        {
            playerMurderSphere.SetActive(isAttacking.Value);
            return;
        }

        // Already hunting a player
        if (logic.TrackingObject != null)
        {
            if (hitPlayerCol.bounds.Contains(logic.TrackingObject.transform.position))
            {
                // Handle attack
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
        }
        else
        {
            // Try to acquire new target
            Collider[] hits = Physics.OverlapSphere(hitPlayerCol.transform.position, hitPlayerCol.radius * transform.localScale.x, playerLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var deathComponent = hit.GetComponent<PlayerDeath>();
                    if (deathComponent != null && !deathComponent.IsDead)
                    {
                        logic.TrackingObject = hit.gameObject;
                        logic.AngerInfluence(1);
                        logic.AlertnessInfluence(1);
                        Debug.Log("Target Acquired: " + hit.name);
                        break;
                    }
                }
            }
        }

        // Reset cooldown if attack disabled
        if (!hitPlayerCol.enabled)
        {
            timer += Time.fixedDeltaTime;
            if (timer > waitfor)
            {
                hitPlayerCol.enabled = true;
                timer = 0;
                currentAttacks = 0;
            }
        }

        // Reset visual
        isAttacking.Value = false;
    }
}
