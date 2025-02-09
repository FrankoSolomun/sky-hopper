using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Assign Player in Inspector
    public float smoothSpeed = 5f; // How smoothly the camera follows
    private bool startFollowing = false; // Determines when camera starts following

    void Update()
    {
        if (player == null)
        {
            return;
        }

        // Follow player only after following is enabled
        if (startFollowing)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }

    public void EnableFollow()
    {
        if (!startFollowing)
        {
            startFollowing = true;
        }
    }
}