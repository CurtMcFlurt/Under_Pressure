using Unity.Netcode;
using UnityEngine;

public class SyncInteractableNetwork : NetworkBehaviour
{

    public NetworkVariable<bool> inUse = new NetworkVariable<bool>();

}
