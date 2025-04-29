using UnityEngine;

public class DeepAnimHandler : MonoBehaviour
{
    public Animator anim;
    public DeepWalkerLogic logic;
    public Agent_findPath findPath;
    public AttackScript attack;
    public MonsterOneShots audioEvent;
    public float timeForAttack;
    public float timeForScream;
    public float timeForEat;
    public float timeForListen;
    public bool debugSceam, debugAttack, debugEat, debugListen;
    private bool StillSearching;
    private bool StillHunting;
    
    // Update is called once per frame
    void Update()
    {
        if(logic.myBehaviour == ActiveBehaviour.hunting && logic.TrackingObject!=null && !StillHunting||debugSceam)
        {
            debugSceam = false;
            StillHunting = true;
            Screaming();
        }

        if(StillHunting && logic.TrackingObject == null)
        {
            StillHunting = false;
        }
        if (attack.isAttacking|| debugAttack)
        {
            debugAttack = false;
            AttackIng();
        }

        if (logic.mood.alertness > 0.9f && !StillSearching ||debugListen) 
        {
            StillSearching = true;
            debugListen = false;
            Listening();
        }

        if (logic.myBehaviour == ActiveBehaviour.roaming)
        {
            StillSearching = false;
        }
        if(logic.myBehaviour==ActiveBehaviour.feeding && logic.myHex.hexCoords == logic.optimalFood.hexCoords || debugEat)
        {
            debugEat = false;
            Eating();
        }
        anim.SetFloat("CurrentSpeed", findPath.curentSpeed);
    }

    private void AttackIng()
    {
   
        //audioEvent.Raise(this,attackSound);
        anim.SetTrigger("Attack");

        audioEvent.MonsterListeningPlay(gameObject);
        findPath.AddTimeToWait(timeForAttack);
    }
    private void Screaming()
    {

        anim.SetTrigger("FoundFool");

        audioEvent.MonsterFoundYouPlay(gameObject);
        findPath.AddTimeToWait(timeForScream);
    }
    private void Eating()
    {

        anim.SetTrigger("IsEating");

        audioEvent.MonsterHuntingPlay(gameObject);
        findPath.AddTimeToWait(timeForEat);
    }
    private void Listening()
    {
        
        Debug.Log("Listening");
        anim.SetTrigger("HeardSound");
        audioEvent.MonsterListeningPlay(gameObject);
        findPath.AddTimeToWait(timeForListen);
    }



}
