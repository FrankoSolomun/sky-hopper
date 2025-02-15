using UnityEngine;

public class SpecialPlatformEffect : MonoBehaviour
{
    public float pushForce = 5f; // Force applied to the player
    public float movementResistance = 2f; // Slows down player movement

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player is in the trigger area
        if (other.CompareTag("Player"))
        {
            Move playerMove = other.GetComponent<Move>();
            if (playerMove != null)
            {
                playerMove.EnterSpecialPlatformVicinity(movementResistance, pushForce);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset player movement when they leave the trigger area
        if (other.CompareTag("Player"))
        {
            Move playerMove = other.GetComponent<Move>();
            if (playerMove != null)
            {
                playerMove.ExitSpecialPlatformVicinity();
            }
        }
    }
}