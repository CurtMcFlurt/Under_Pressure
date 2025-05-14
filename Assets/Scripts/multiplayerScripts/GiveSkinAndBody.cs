using Unity.Netcode;
using UnityEngine;

public class GiveSkinAndBody : NetworkBehaviour
{
    // This variable is synced from the server to clients
     public NetworkVariable<int> appearanceIndex = new NetworkVariable<int>();
    public Mesh[] meshes;
    public Material[] materials;

    public SkinnedMeshRenderer skinnedRenderer;
    public override void OnNetworkSpawn()
    {
        appearanceIndex.Value = (NetworkManager.Singleton.ConnectedClients.Count-1)%meshes.Length;

         OnAppearanceChanged(appearanceIndex.Value);
     

    }



    private void OnAppearanceChanged(int newIndex)
    {
        if (newIndex < 0 || newIndex >= meshes.Length) return;

        skinnedRenderer.sharedMesh = meshes[newIndex];
        skinnedRenderer.material = materials[newIndex];
    }

}
