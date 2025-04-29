using UnityEngine;

public class CogInteracting : Interactable
{
    public Cog cog;
    public void OnEnable()
    {
        cog = GetComponent<Cog>();
    }

    public override void StartInteraction(GameObject sender)
    {
       var inter=sender.GetComponent<InteractWithInteractable>();
        if (cog.ActiveSocket != inter.aimIndicator) { cog.ActiveSocket = inter.aimIndicator; }
    }

    
}
