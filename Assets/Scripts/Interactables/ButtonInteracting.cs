using UnityEngine;

public class ButtonInteracting : Interactable
{
    public GameEvent buttonEvent;
    public string sendString;
    public override void StartInteraction(GameObject sender)
    {
        base.StartInteraction(sender);
        Debug.Log(sender.name);
        buttonEvent.Raise(this,sendString);
    }
}
