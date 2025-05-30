using Unity.Netcode;
using UnityEngine;

public class DropedItemInteracting :Interactable
{
    public MeshRenderer _renderer;
    public SphereCollider SphereCollider;
    public GameObject objectToGrab;
    public HoldAThing grabber;
    public override void StartInteraction(GameObject sender)
    {
        base.StartInteraction(sender);
        Debug.Log("cogPickup");
        if(taken)return;
        taken=true;
       grabber= sender.GetComponent<HoldAThing>();
        grabber.itemOrigin = this;
        grabber.handHoldItem = objectToGrab;
        grabber.OnGainedOwnership();
        GetComponent<NetworkObject>().ChangeOwnership(sender.GetComponent<NetworkObject>().OwnerClientId);
    }

    void Update()
    {
        if (!GetComponent<NetworkObject>().HasAuthority)
        {
            taken = GetComponent<SyncInteractableNetwork>().inUse.Value;
        }
        else GetComponent<SyncInteractableNetwork>().inUse.Value =taken;

        _renderer.enabled = !taken;
        SphereCollider.enabled = !taken;
        if (!taken) 
        { 
            objectToGrab.transform.localPosition = Vector3.zero;
            objectToGrab.transform.parent = this.transform;
        }
        if (grabber == null) { taken = false; }
    }
}
