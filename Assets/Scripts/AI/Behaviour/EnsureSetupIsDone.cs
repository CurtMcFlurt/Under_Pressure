using Unity.Netcode;
using UnityEngine;

public class EnsureSetupIsDone : NetworkBehaviour
{
    public GameObject deeperPrefab; // Prefab with NetworkObject attached
    public HexagonalWeight currentWeight;
    public float waitForSeconds = 1;
    private float seconds;
    public bool spawned;
    void Update()
    {
        if (!HasAuthority)
        {

            return;
        }
        if ((seconds > waitForSeconds) && !spawned)
        {
            SpawnDeeper();
            spawned = true;

        }
        else
        {

            seconds += Time.deltaTime;
        }
    }

    private void SpawnDeeper()
    {
        GameObject deeperInstance = Instantiate(deeperPrefab, transform.position, Quaternion.identity);
        deeperInstance.GetComponent<NetworkObject>().Spawn(); // Spawns for all clients
        spawned = true;
        Debug.Log("Spawned deeper object");
    }
}
