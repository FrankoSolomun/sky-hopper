using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    // Reference to the MagnetPowerUp script (can be assigned in the Inspector)
    public MagnetPowerUp magnetScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player collides with the pickup
        if (other.CompareTag("Player"))
        {
            // Activate the magnet effect if the reference exists
            if (magnetScript)
            {
                magnetScript.ActivateMagnet();
            }
            else
            {
                Debug.LogWarning("[MagnetPickup] No MagnetPowerUp reference found!");
            }

            // Destroy the pickup object after it's collected
            Destroy(gameObject);
        }
    }
}