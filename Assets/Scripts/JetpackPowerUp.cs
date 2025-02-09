using UnityEngine;
using System.Collections;

public class JetpackPowerUp : MonoBehaviour
{
    [Header("Jetpack Settings")]
    public float jetpackDuration = 10f;    // Duration for which the jetpack is active
    public float upwardForce = 15f;        // Upward force applied each frame
    public float maxHeightGain = 20f;      // Maximum height gain relative to activation point (units)

    private bool isActive = false;
    private Transform player;
    private float activationY;             // The player's Y position when the jetpack is activated

    [Header("Icon / Visuals")]
    public GameObject jetpackIconPrefab;    // Optional: icon to display on the player when active
    // Dedicated offset for the jetpack icon so that it appears centered horizontally
    // on the player and slightly below, so you can see the flames.
    public Vector3 jetpackIconOffset = new Vector3(0f, -0.5f, 0f);
    private GameObject jetpackIconInstance;

    void Start()
    {
        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("JetpackPowerUp: Player not found!");
        }
    }

    public void ActivateJetpack()
    {
        if (isActive)
        {
            Debug.LogWarning("Jetpack already active!");
            return;
        }
        isActive = true;
        activationY = player.position.y;
        Debug.Log("Jetpack Activated!");

        // Show the jetpack icon using its dedicated offset.
        if (jetpackIconPrefab != null && player != null)
        {
            // Instantiate the icon at the player's position plus the offset.
            jetpackIconInstance = Instantiate(jetpackIconPrefab, player.position + jetpackIconOffset, Quaternion.identity);
            // Set the parent with worldPositionStays = false so that local coordinates are used.
            jetpackIconInstance.transform.SetParent(player, false);
            // Explicitly set the local position to the desired offset.
            jetpackIconInstance.transform.localPosition = jetpackIconOffset;
        }

        StartCoroutine(JetpackRoutine());
    }

    IEnumerator JetpackRoutine()
    {
        float elapsed = 0f;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("JetpackPowerUp: No Rigidbody2D found on player!");
            yield break;
        }
        while (elapsed < jetpackDuration && (player.position.y - activationY < maxHeightGain))
        {
            // Apply upward force continuously.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, upwardForce);
            elapsed += Time.deltaTime;
            yield return null;
        }

        DisableJetpack();
    }

    public void DisableJetpack()
    {
        if (!isActive) return;
        isActive = false;
        Debug.Log("Jetpack Deactivated!");

        if (jetpackIconInstance != null)
        {
            Destroy(jetpackIconInstance);
        }
    }
}