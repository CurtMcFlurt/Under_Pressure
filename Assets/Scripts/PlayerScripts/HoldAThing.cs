using Unity.Netcode;
using UnityEngine;

public class HoldAThing : NetworkBehaviour
{
    public GameObject handHoldItem;
    public GameObject handAnchor_Net;
    public GameObject handAnchor_Pers;
    public DropedItemInteracting itemOrigin;


    public Vector3 offset_net;
    public Vector3 offset_pers;
    public Vector3 rot_net;
    public Vector3 rot_pers;
    public Vector3 scale_net = Vector3.one;
    public Vector3 scale_pers = Vector3.one;

    [Header("Debug Options")]
    public bool autoDetectHeldItemChange = true;

    private GameObject _lastHeldItem = null;

    void Update()
    {
        if (autoDetectHeldItemChange && IsOwner)
        {
            if (_lastHeldItem == null && handHoldItem != null)
            {
                Debug.Log("[HoldAThing] Auto-assigned held item: " + handHoldItem.name);
                SetHeldItem(handHoldItem);
            }

            _lastHeldItem = handHoldItem;
        }

        if (handHoldItem != null)
        {
            handHoldItem.SetActive(true);

            if (!IsOwner)
            {
                handHoldItem.transform.SetParent(handAnchor_Net.transform);
                handHoldItem.transform.localPosition = offset_net;
                handHoldItem.transform.localEulerAngles = rot_net;
                handHoldItem.transform.localScale = scale_net;
            }
            else
            {
                handHoldItem.transform.SetParent(handAnchor_Pers.transform);
                handHoldItem.transform.localPosition = offset_pers;
                handHoldItem.transform.localEulerAngles = rot_pers;
                handHoldItem.transform.localScale = scale_pers;
            }
        }
    }
    public void DropHeldItem()
    {
        if (handHoldItem != null)
        {
            handHoldItem.SetActive(false);
            handHoldItem.transform.SetParent(null); // Optional: unparent it
            handHoldItem = null;
            _lastHeldItem = null;

            if (IsOwner)
            {
                SendHeldItemRpc(default, OwnerClientId); // Notify others the item is dropped
            }
        }
    }

    public void SetHeldItem(GameObject item)
    {
        handHoldItem = item;

        if (IsOwner && item != null)
        {
            var netObj = item.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                SendHeldItemRpc(netObj, OwnerClientId);
            }
        }
        else if (item == null)
        {
            SendHeldItemRpc(default, OwnerClientId);
        }

        _lastHeldItem = item;
    }

    [Rpc(SendTo.Everyone)]
    private void SendHeldItemRpc(NetworkObjectReference itemRef, ulong senderId)
    {
        Debug.Log("Sending");
        if (NetworkManager.Singleton.LocalClientId == senderId)
            return;

        if (itemRef.TryGet(out var obj))
        {
            handHoldItem = obj.gameObject;
            obj.gameObject.GetComponent<DropedItemInteracting>().taken = true;
        }
        else
        {
            handHoldItem = null;
        }

        _lastHeldItem = handHoldItem;
    }
}
