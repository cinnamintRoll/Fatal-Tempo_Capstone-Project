using UnityEngine;

public class HeadCollider : MonoBehaviour
{
    public float damageCooldown = 1.0f; // Time (in seconds) before the player can take damage again
    private bool canTakeDamage = true;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object's layer is "Obstacle" and player can take damage
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle") && canTakeDamage)
        {
            Debug.Log("Head hit an obstacle!");

            // Take damage and start cooldown
            TakeDamage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the trigger collider's layer is "Obstacle" and player can take damage
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") && canTakeDamage)
        {
            Debug.Log("Head hit an obstacle (trigger)!");

            Destroy(other.gameObject);
            // Take damage and start cooldown
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        // Access the PlayerHealth instance and call TakeDamage
        PlayerHealth.Instance.TakeDamage();

        // Set damage immunity
        canTakeDamage = false;

        // Start cooldown
        Invoke(nameof(ResetDamageCooldown), damageCooldown);
    }

    private void ResetDamageCooldown()
    {
        canTakeDamage = true;
    }
}
