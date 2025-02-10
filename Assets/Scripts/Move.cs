using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Move : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    public TextMeshProUGUI platformCounterText;
    private Rigidbody2D rb;
    public static bool isGrounded = false;

    // The total count of unique platforms landed on (or passed)
    public static int platformsJumped = 0;

    // Tracks which platforms have been counted (via collision)
    private HashSet<GameObject> jumpedPlatforms = new HashSet<GameObject>();

    // Reference to the CameraFollow script (assign in Inspector)
    public CameraFollow cameraFollow;

    // Whether we should ignore the very first platform
    private bool ignoreFirstLanding = true;

    // Instead of storing a fixed position, store a reference to the last platform's transform.
    private Transform lastPlatform;
    private bool hasLastPlatform = false;

    // Adjustable offset when respawning with shield.
    // Tweak this value in the Inspector until the player lands safely on the platform.
    public float safeRespawnOffset = 1.2f;

    // How much vertical distance counts as one "platform" for scoring.
    public float scoreSpacing = 3f;
    // The player's y position when the score was last updated.
    private float lastScoreY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2.5f; // Increases falling speed
        // Start score at zero
        platformCounterText.text = "Score: 0";
        // Initialize lastScoreY with the starting y-position of the player.
        lastScoreY = transform.position.y;
    }

    void Update()
    {
        // Get movement input
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        transform.position += movement * Time.deltaTime * moveSpeed;

        // Allow jumping continuously while holding Space if the player is grounded
        if (Input.GetButton("Jump") && isGrounded)
        {
            Jump();
        }

        // Faster falling effect
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = 4f; // Increase gravity for falling
        }
        else
        {
            rb.gravityScale = 2f; // Normal gravity while rising
        }

        // Get camera width boundaries
        float cameraHalfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
        float leftBoundary = -cameraHalfWidth;
        float rightBoundary = cameraHalfWidth;

        // Clamp player position within screen limits
        float clampedX = Mathf.Clamp(transform.position.x, leftBoundary, rightBoundary);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // If the player has a jetpack and it's active, update the score based on vertical distance traveled.
        JetpackPowerUp jetpack = GetComponent<JetpackPowerUp>();
        if (jetpack != null && jetpack.IsJetpackActive())
        {
            float delta = transform.position.y - lastScoreY;
            if (delta >= scoreSpacing)
            {
                // Calculate how many "platform units" have been passed.
                int count = Mathf.FloorToInt(delta / scoreSpacing);
                platformsJumped += count;
                lastScoreY += count * scoreSpacing;
                platformCounterText.text = "Score: " + platformsJumped;
                Debug.Log("Jetpack scoring: increased score by " + count + " to " + platformsJumped);
            }
        }

        // Restart game if player falls below the safe threshold
        if (transform.position.y < PlatformSpawner.lowestPlatformY - 2f)
        {
            Debug.Log("Player is falling below safe threshold. Checking for shield...");
            ShieldPowerUp shield = FindObjectOfType<ShieldPowerUp>();
            if (shield != null)
            {
                Debug.Log("Found ShieldPowerUp. IsShieldActive: " + shield.IsShieldActive() + ", hasLastPlatform: " + hasLastPlatform);
            }
            else
            {
                Debug.Log("No ShieldPowerUp found on the scene.");
            }
            if (shield && shield.IsShieldActive() && hasLastPlatform)
            {
                Debug.Log("Shield is active. Teleporting player to current position of last platform: " + lastPlatform.position);
                // Use the current position of the platform plus an offset
                transform.position = new Vector3(
                    lastPlatform.position.x,
                    lastPlatform.position.y + safeRespawnOffset,
                    transform.position.z
                );

                // Reset vertical velocity
                rb.linearVelocity = Vector2.zero;

                // Disable shield so it can't be used again
                shield.DisableShield();
            }
            else
            {
                Debug.Log("Shield not active or no last platform saved. Restarting game.");
                RestartGame();
            }
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;

        // If we jump, un-parent from any moving platform
        if (transform.parent != null)
        {
            transform.parent = null;
        }
    }

    private void RestartGame()
    {
        platformsJumped = 0;
        PlatformSpawner.lowestPlatformY = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform") || collision.gameObject.CompareTag("SpecialPlatform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f) // landing from above
                {
                    // Save a reference to the platform's transform so that we can use its current position later.
                    lastPlatform = collision.transform;
                    hasLastPlatform = true;
                    Debug.Log("Landed on platform (reference saved): " + lastPlatform.name + " at position: " + lastPlatform.position);

                    isGrounded = true;

                    // If the platform is a MovingPlatform, parent the player
                    MovingPlatform movingPlat = collision.gameObject.GetComponent<MovingPlatform>();
                    if (movingPlat != null)
                    {
                        transform.parent = collision.transform;
                    }

                    // Check if we should skip the very first platform
                    if (ignoreFirstLanding)
                    {
                        // Skip incrementing the score for this first collision
                        ignoreFirstLanding = false;
                    }
                    else
                    {
                        // Normal logic: increment score for unique platforms
                        if (!jumpedPlatforms.Contains(collision.gameObject))
                        {
                            jumpedPlatforms.Add(collision.gameObject);
                            platformsJumped++;
                            platformCounterText.text = "Score: " + platformsJumped;

                            if (platformsJumped >= 2 && cameraFollow != null)
                            {
                                cameraFollow.EnableFollow();
                            }
                        }
                    }
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If we leave a platform, set isGrounded = false & un-parent if needed
        if (collision.gameObject.CompareTag("Platform") || collision.gameObject.CompareTag("SpecialPlatform"))
        {
            isGrounded = false;
            if (transform.parent == collision.transform)
            {
                transform.parent = null;
            }
        }
    }
}