using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform (assign in Inspector)
    public float smoothSpeed = 5f; // Speed factor for smooth camera movement

    private bool startFollowing = false; // Flag to control when the camera starts following

    void Update()
    {
        // Ensure the player reference exists before proceeding
        if (player == null)
        {
            return;
        }

        // Camera follows the player only after following is enabled
        if (startFollowing)
        {
            // Target position maintains the camera's X and Z but updates Y to follow the player
            Vector3 targetPosition = new Vector3(transform.position.x, player.position.y, transform.position.z);

            // Smoothly interpolate the camera's position towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }

    // Public method to enable camera following (can be triggered by game events)
    public void EnableFollow()
    {
        if (!startFollowing)
        {
            startFollowing = true;
        }
    }
}