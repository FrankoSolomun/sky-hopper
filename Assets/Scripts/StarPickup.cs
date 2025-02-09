using UnityEngine;

public class StarPickup : MonoBehaviour
{
    public int starValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure "Player" is the correct tag on your player object
        if (other.CompareTag("Player"))
        {
            // Add to star count
            if (StarManager.Instance != null)
            {
                StarManager.Instance.AddStars(starValue);
            }

            // Destroy the star
            Destroy(gameObject);
        }
    }
}