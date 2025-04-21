using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public bool occupied = false;
    public GameObject occupant = null;
    public bool respawn = false;

    public void Respawn(GameObject player)
    {
        occupant = player;
        occupied = true;
        player.transform.position = transform.position;
    }

    private void Update()
    {
        if (respawn && occupant != null)
        {
            respawn = false;
            Respawn(occupant);
        }
    }
}
