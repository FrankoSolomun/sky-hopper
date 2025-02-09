using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Required for restarting the game
using System.Collections.Generic; // Required for HashSet

public class Move : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    public TextMeshProUGUI platformCounterText;
    private Rigidbody2D rb;
    public static bool isGrounded = false;

    // The total count of unique platforms landed on
    public static int platformsJumped = 0;

    // Tracks which platforms have been counted
    private HashSet<GameObject> jumpedPlatforms = new HashSet<GameObject>();

    // Reference to the CameraFollow script (assign in Inspector)
    public CameraFollow cameraFollow;

    // NEW: Whether we should ignore the very first platform
    private bool ignoreFirstLanding = true;

    private Vector3 lastPlatformPosition; // Where we last safely stood
    private bool hasLastPlatformPosition = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2.5f; // Increases falling speed
        // Start score at zero
        platformCounterText.text = "Score: 0";
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

        // Restart game if player falls below the last platform
        if (transform.position.y < PlatformSpawner.lowestPlatformY - 2f)
        {
            // check if shield is active
            ShieldPowerUp shield = FindObjectOfType<ShieldPowerUp>();
            if (shield && shield.IsShieldActive() && hasLastPlatformPosition)
            {
                // teleport back to lastPlatformPosition
                transform.position = new Vector3(
                    lastPlatformPosition.x,
                    lastPlatformPosition.y + 1f,
                    transform.position.z
                );

                // reset vertical velocity
                rb.linearVelocity = Vector2.zero;

                // disable shield so it can't be used again
                shield.DisableShield();
            }
            else
            {
                // normal death
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
                    lastPlatformPosition = collision.transform.position;
                    hasLastPlatformPosition = true;

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
        // If we leave a platform, set isGrounded=false & un-parent
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