using Unity.Netcode;
using UnityEngine;

public class MultiplayerSpawner :NetworkBehaviour
{
    [SerializeField] private NetworkObject enemyPrefab;
    public void OnEnable()
    {

        Debug.Log("StartActivate");

        if (!IsServer) return; // Only server spawns it

        Debug.Log("Activate");
        SpawnMyObject();

    }
    public void SpawnMyObject()
    {
        if (!IsServer) return; // Only server spawns it
        Debug.Log("nowActive");
        if (enemyPrefab == null)
        {
            var instance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            enemyPrefab = instance.GetComponent<NetworkObject>();
            enemyPrefab.Spawn();
        }
    }
}
