using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Move : MonoBehaviour
{
    public ZoneManager zoneManager;
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public float spaceZoneJumpForceMultiplier = 2f; // Increase jump force in SpaceZone

    public Text platformCounterText;
    public Font newFont; // Add a public field for the new font
    private Rigidbody2D rb;
    private Animator animator;
    public static bool isGrounded = false;
    public AudioSource jumpSoundEffect;

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

    // Variables for special platform effect
    private bool isNearSpecialPlatform = false;
    private float movementResistance = 1f; // Default resistance (no effect)
    private float pushForce = 0f; // Default push force (no effect)
    private Coroutine resetEffectsCoroutine; // Coroutine to delay resetting effects

    public float normalGravityScale = 2.5f; // Normal gravity scale
    public float spaceZoneGravityScale = 0.5f; // Gravity scale in SpaceZone
    private bool isInSpaceZone = false; // Track if the player is in SpaceZone
    public static bool isPaused = false; // Track if the game is paused
    public GameObject gameOverUI; // Assign the Game Over Panel in the Inspector
    private bool isDead = false;
    


    void Start()
    {
        // Reset the pause state
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.ResumeGame(); // Ensure the game is not paused
        }
        
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = normalGravityScale; // Increases falling speed

        // Start score at zero
        platformCounterText.text = "Score: 0";

        // Change the font if a new font is assigned
        if (newFont != null)
        {
            platformCounterText.font = newFont;
        }
        
        // Initialize lastScoreY with the starting y-position of the player.
        lastScoreY = transform.position.y;
    }

    void Update()
    {
        // If the player is dead, do nothing
        if(isDead)
            return;

        // If the game is paused, do nothing
        if (isPaused)
            return;

        // Update animations based on input and state
        HandleAnimations();

        // Allow jumping continuously while holding Space if the player is grounded
        if (Input.GetButton("Jump") && isGrounded)
        {
            Jump();
        }

        // Restart game if player falls below the safe threshold
        if (transform.position.y < PlatformSpawner.lowestPlatformY - 2f)
        {
            Debug.Log("Player is falling below safe threshold. Checking for shield...");
            ShieldPowerUp shield = FindObjectOfType<ShieldPowerUp>();
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
                Debug.Log("Shield not active or no last platform saved. Showing Game Over UI.");
                FreezePlayer(true);
                ShowGameOverUI();
            }
        }

        // Check the current zone and handle SpaceZone logic
        ZoneDefinition currentZone = zoneManager.GetCurrentZone();
        if (currentZone != null)
        {
            if (currentZone.zoneName == "SpaceZone" && !isInSpaceZone)
            {
                EnterSpaceZone(); // Enter SpaceZone
            }
                else if (currentZone.zoneName != "SpaceZone" && isInSpaceZone)
            {
                ExitSpaceZone(); // Exit SpaceZone
            }
        }
    }

    void FixedUpdate()
    {
        // If the game is paused, do nothing
        if (isPaused)
            return;

        // Handle movement in FixedUpdate for smoother physics
        HandleMovement();

        // Apply push force if near a special platform
        if (isNearSpecialPlatform)
        {
            rb.AddForce(Vector2.up * pushForce, ForceMode2D.Force);
        }
    }

    // Method to show the Game Over UI
    // Method to show the Game Over UI
private void ShowGameOverUI()
{
    if (gameOverUI != null)
    {
        gameOverUI.SetActive(true); // Show the Game Over UI

        // Get all Text components in the Game Over UI
        Text[] textComponents = gameOverUI.GetComponentsInChildren<Text>(true);

        if (textComponents.Length >= 2) // Ensure there are at least 2 Text components
        {
            // Assume:
            // textComponents[0] is the star counter
            // textComponents[1] is the platform counter

            // Update the star counter text with the current star count
            if (StarManager.Instance != null)
            {
                textComponents[0].text = "Stars: " + StarManager.Instance.GetStarCount();
            }
            else
            {
                Debug.LogError("StarManager instance is missing.");
            }

            // Update the platform counter text with the current platform count
            textComponents[1].text = "Score: " + platformsJumped;
        }
        else
        {
            Debug.LogError("Not enough Text components found in Game Over UI. Expected at least 2.");
        }
    }
    else
    {
        Debug.LogError("Game Over UI is not assigned in the Move script.");
    }
}

    private void HandleMovement()
    {
        // Get raw input for immediate response
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * moveSpeed / movementResistance; // Apply movement resistance
        rb.linearVelocity = velocity;

        // Flip the player horizontally based on movement direction
        if (horizontalInput > 0.1f) // Moving right
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Face right
        }
        else if (horizontalInput < -0.1f) // Moving left
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // Face left
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

        // If the player has jetpack and its active, update the score based on vertical distance traveled
        JetpackPowerUp jetpack = GetComponent<JetpackPowerUp>();
        if (jetpack != null & jetpack.IsJetpackActive())
        {
            float delta = transform.position.y - lastScoreY;
            if (delta >= scoreSpacing)
            {
                // Calculate how many "platform units" have been paused
                int count = Mathf.FloorToInt(delta / scoreSpacing);
                platformsJumped += count;
                lastScoreY += count * scoreSpacing;
                platformCounterText.text = "Score" + platformsJumped;
                Debug.Log("Jetpack scring: increased score by " + count + "to" + platformsJumped);
            }
        }
    }

    private void HandleAnimations()
    {

        // If the game is paused, stop animations
        if (isPaused)
        {
            animator.speed = 0; // Pause animations
            return;
        }
        else
        {
            animator.speed = 1; // Resume animations
        }
        // Update animation parameters
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Set the xVelocity parameter based on horizontal movement
        animator.SetFloat("xVelocity", Mathf.Abs(horizontalInput));

        // Set the yVelocity parameter based on vertical movement
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        // Set the isJumping parameter based on whether the player is grounded
        animator.SetBool("isJumping", !isGrounded);

        // Set the isRunning parameter based on horizontal movement
        // Allow running animation to play even when jumping
        animator.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.1f && isGrounded);

        // Add a new parameter to blend running and jumping animations
        animator.SetBool("isRunningWhileJumping", Mathf.Abs(horizontalInput) > 0.1f && !isGrounded);
    }

    private void Jump()
    {
        float effectiveJumpForce = jumpForce;

        // Increase jump force in SpaceZone
        if (isInSpaceZone)
        {
            effectiveJumpForce *= spaceZoneJumpForceMultiplier;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, effectiveJumpForce);
        isGrounded = false;
        
        if(jumpSoundEffect != null)
        {
            jumpSoundEffect.Play();
        }
        animator.SetBool("isJumping", true);

        // If we jump, un-parent from any moving platform
        if (transform.parent != null)
        {
            transform.parent = null;
        }
    }

    // Method to go back to the home screen
    public void BackToHome()
    {
        FreezePlayer(false); //Unfreeze the player
        
        // Hide the Game Over UI a show the start menu
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null && pauseManager.startMenuUI != null)
        {
            pauseManager.startMenuUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        FreezePlayer(false); // Unfreeze the player
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
                    animator.SetBool("isJumping", false);

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
            animator.SetBool("isJumping", true);
            if (transform.parent == collision.transform)
            {
                transform.parent = null;
            }
        }
    }

    // Called when the player enters the vicinity of a special platform
    public void EnterSpecialPlatformVicinity(float resistance, float force)
    {
        isNearSpecialPlatform = true;
        movementResistance = resistance;
        pushForce = force;

        // Stop any existing reset coroutine
        if (resetEffectsCoroutine != null)
        {
            StopCoroutine(resetEffectsCoroutine);
        }
    }

    // Called when the player exits the vicinity of a special platform
    public void ExitSpecialPlatformVicinity()
    {
        // Start a coroutine to reset effects after 2 seconds
        resetEffectsCoroutine = StartCoroutine(ResetEffectsAfterDelay(2f));
    }

    // Coroutine to reset effects after a delay
    private IEnumerator ResetEffectsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset effects
        isNearSpecialPlatform = false;
        movementResistance = 1f; // Reset to default
        pushForce = 0f; // Reset to default
    }

    // Called when entering SpaceZone
    public void EnterSpaceZone()
    {
        isInSpaceZone = true;
        rb.gravityScale = spaceZoneGravityScale;
        Debug.Log("Entered SpaceZone. Gravity scale changed to: " + spaceZoneGravityScale);
    }

    // Called when exiting SpaceZone
    public void ExitSpaceZone()
    {
        isInSpaceZone = false;
        rb.gravityScale = normalGravityScale;
        Debug.Log("Exited SpaceZone. Gravity scale restored to: " + normalGravityScale);
    }

    public void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            rb.linearVelocity = Vector2.zero; // Stop all movement
            rb.isKinematic = true; // Disable physics

            // Freeze the camera
            if (cameraFollow != null)
            {
                cameraFollow.FreezeCamera();
            }
            else
            {
                Debug.LogError("CameraFollow script is not assigned in the Move script.");
            }
        }
        else
        {
            rb.isKinematic = false; // Re-enable physics
        }
    }
     // Add these methods to handle pause/resume
    public static void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0; // Freeze the game
    }

    public static void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1; // Resume the game
    }

}