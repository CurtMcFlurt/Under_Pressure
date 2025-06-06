using Unity.Netcode;
using UnityEngine;

public class DropedItemInteracting : Interactable
{
    public MeshRenderer _renderer;
    public SphereCollider SphereCollider;
    public GameObject objectToGrab;
    public HoldAThing grabber;

    private NetworkObject netObj;
    private SyncInteractableNetwork syncNet;

    private void Awake()
    {
        netObj = GetComponent<NetworkObject>();
        syncNet = GetComponent<SyncInteractableNetwork>();
    }

    public override void StartInteraction(GameObject sender)
    {
        base.StartInteraction(sender);
        if (taken) return;

        Debug.Log("cogPickup");
        taken = true;

        grabber = sender.GetComponent<HoldAThing>();
        grabber.itemOrigin = this;
        grabber.handHoldItem = objectToGrab;
        grabber.OnGainedOwnership();

        // Ask server to transfer ownership
        if (sender.TryGetComponent(out NetworkObject senderNetObj))
        {
            RequestOwnershipRpc(senderNetObj.OwnerClientId);
        }
    }

    void Update()
    {
        if (!netObj.HasAuthority)
        {
            taken = syncNet.inUse.Value;
        }
        else
        {
            syncNet.inUse.Value = taken;
        }

        _renderer.enabled = !taken;
        SphereCollider.enabled = !taken;

        if (!taken)
        {
            objectToGrab.transform.localPosition = Vector3.zero;
            objectToGrab.transform.parent = transform;
        }

        if (grabber == null)
        {
            taken = false;
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(targetClientId))
        {
            netObj.ChangeOwnership(targetClientId);
        }
    }
}
