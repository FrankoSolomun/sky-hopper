using UnityEngine;
using System.Collections;

public class JetpackPowerUp : MonoBehaviour
{
    [Header("Jetpack Settings")]
    public float jetpackDuration = 10f;    // Total duration the jetpack remains active
    public float upwardForce = 15f;        // The force applied upward each frame
    public float maxHeightGain = 20f;      // Maximum height gain relative to activation point

    private bool isActive = false;         // Tracks if the jetpack is currently active
    private Transform player;              // Reference to the player's Transform
    private float activationY;             // Stores the Y position of the player when the jetpack is activated

    [Header("Icon / Visuals")]
    public GameObject jetpackIconPrefab;   // Optional: Icon that appears when jetpack is active
    public Vector3 jetpackIconOffset = new Vector3(0f, -0.5f, 0f); // Offset for proper icon positioning
    private GameObject jetpackIconInstance; // Instance of the jetpack icon

    void Start()
    {
        // Find the player using the "Player" tag
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Log an error if the player was not found
        if (player == null)
        {
            Debug.LogError("JetpackPowerUp: Player not found!");
        }
    }

    // Activates the jetpack, applying an upward force to the player for a limited time.
    public void ActivateJetpack()
    {
        // Prevent activation if the jetpack is already in use
        if (isActive)
        {
            Debug.LogWarning("Jetpack already active!");
            return;
        }

        isActive = true;
        activationY = player.position.y; // Store activation height
        Debug.Log("Jetpack Activated!");

        // Display the jetpack icon above the player (if assigned)
        if (jetpackIconPrefab != null && player != null)
        {
            // Create the jetpack icon at the correct position
            jetpackIconInstance = Instantiate(jetpackIconPrefab, player.position + jetpackIconOffset, Quaternion.identity);

            // Set the icon as a child of the player so it follows their movement
            jetpackIconInstance.transform.SetParent(player, false);

            // Apply the offset to ensure proper positioning
            jetpackIconInstance.transform.localPosition = jetpackIconOffset;
        }

        // Start the coroutine to handle jetpack duration and movement
        StartCoroutine(JetpackRoutine());
    }

    // Handles the jetpack behavior over time, applying upward force while it's active.
    IEnumerator JetpackRoutine()
    {
        float elapsed = 0f;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        // Ensure the player has a Rigidbody2D component before applying forces
        if (rb == null)
        {
            Debug.LogError("JetpackPowerUp: No Rigidbody2D found on player!");
            yield break;
        }

        // Continue applying force while within the duration and height limit
        while (elapsed < jetpackDuration && (player.position.y - activationY < maxHeightGain))
        {
            // Apply constant upward force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, upwardForce);

            // Track elapsed time
            elapsed += Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Disable the jetpack once conditions are met
        DisableJetpack();
    }

    // Disables the jetpack and removes its effects.
    public void DisableJetpack()
    {
        // Only disable if it's currently active
        if (!isActive) return;

        isActive = false;
        Debug.Log("Jetpack Deactivated!");

        // Remove the jetpack icon if it exists
        if (jetpackIconInstance != null)
        {
            Destroy(jetpackIconInstance);
        }
    }

    // Checks if the jetpack is currently active.
    public bool IsJetpackActive()
    {
        return isActive;
    }
}