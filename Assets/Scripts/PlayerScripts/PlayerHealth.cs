using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Transform respawnPoint;

    public float health=100;

    private float maxHealth;
    void Start()
    {
        if (respawnPoint == null) respawnPoint = transform;     
        maxHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) Respawn();
    }

    public void Respawn()
    {
        health = maxHealth;
        transform.position = respawnPoint.position;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }
    
}
