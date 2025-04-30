using UnityEngine;

public class CogInteracting : Interactable
{
    public Cog cog;
    public InteractWithInteractable intermid;
    public float waitToInteract = 2;
    public float currentWait=3;
    public void OnEnable()
    {
        cog = GetComponent<Cog>();
    }

    public override void StartInteraction(GameObject sender)
    {
        if (taken || currentWait<waitToInteract) return;
        
       var inter=sender.GetComponent<InteractWithInteractable>();
        Debug.Log("Found");
        if (cog.ActiveSocket != inter.aimIndicator) { 
            cog.ActiveSocket = inter.aimIndicator;
      
            Debug.Log("Found2");
            
            intermid = inter;
            taken = true;
            inter.IsHolding = true;
            if(cog.mySocket != null)
            {
                cog.mySocket.activeCog = null;
                cog.mySocket = null;

            }
            currentWait = 0;
        }
    }

    public void Update()
    {
        if (waitToInteract > currentWait) currentWait += Time.deltaTime;
        if (intermid == null) return;
        if (intermid.retract)
        {
            Debug.Log("retract");
            cog.ActiveSocket = null;
            taken = false;
            intermid.IsHolding = false;
            intermid = null;

            return;
        }
        if (intermid.intearct && waitToInteract<currentWait)
        {

            Debug.Log("interact");
          if(cog.IsOnAvailableSocket())
            {

                if (cog.mySocket.activeCog == null)
                {
                    currentWait = 0;
                    taken = false;
                    intermid.IsHolding = false;
                    intermid = null;
                    cog.mySocket.activeCog = cog;
                    cog.ActiveSocket = cog.mySocket.gameObject;
                }
                else cog.mySocket = null;
            }
        }
    }


}
