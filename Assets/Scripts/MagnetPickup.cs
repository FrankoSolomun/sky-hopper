using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    // Option A: Drag the MagnetPowerUp from your scene into this field via Inspector
    public MagnetPowerUp magnetScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only pick up if the player collides
        if (other.CompareTag("Player"))
        {
            // If you haven't assigned magnetScript in Inspector, you can do:
            // magnetScript = FindObjectOfType<MagnetPowerUp>();

            if (magnetScript)
            {
                magnetScript.ActivateMagnet();
            }
            else
            {
                Debug.LogWarning("[MagnetPickup] No MagnetPowerUp reference found!");
            }

            // Remove this pickup object so it can't be picked again
            Destroy(gameObject);
        }
    }
}