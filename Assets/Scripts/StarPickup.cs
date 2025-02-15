using UnityEngine;

public class StarPickup : MonoBehaviour
{
    public int starValue = 1;
    public AudioClip collectionSoundClip; // Assign this in the Prefab Editor

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Play the collection sound effect at the star's position
            if (collectionSoundClip != null)
            {
                AudioSource.PlayClipAtPoint(collectionSoundClip, transform.position);
                Debug.Log("Playing collection sound effect");
            }
            else
            {
                Debug.LogError("collectionSoundClip is not assigned!");
            }

            // Add to star count
            if (StarManager.Instance != null)
            {
                StarManager.Instance.AddStars(starValue);
            }

            // Destroy the star GameObject immediately
            Destroy(gameObject);
        }
    }
}