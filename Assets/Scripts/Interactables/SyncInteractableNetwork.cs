using Unity.Netcode;
using UnityEngine;

public class SyncInteractableNetwork : NetworkBehaviour
{

    public NetworkVariable<bool> inUse = new(
      false,
       NetworkVariableReadPermission.Everyone, // default is Everyone
      NetworkVariableWritePermission.Owner
  );

}
