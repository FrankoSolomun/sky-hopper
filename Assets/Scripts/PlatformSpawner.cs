using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    // Reference to ZoneManager
    public ZoneManager zoneManager;

    // Reference to StarPrefab (assign in Inspector)
    public GameObject starPrefab;

    // Reference to PowerUpPrefabs (3 prefabs: Magnet, Shield, Jetpack, etc.)
    public GameObject[] powerUpPrefabs;

    public int numberOfStartingPlatforms = 5;
    public float verticalSpacing = 3f;
    public float minX = -2f, maxX = 2f;

    // This determines how far above the regular platform the special prefab should be placed.
    public float specialPlatformYOffset = 1f;

    private float highestPlatformY;
    public static float lowestPlatformY;
    private Transform player;
    private List<GameObject> platforms = new List<GameObject>();

    // STAR LOGIC:
    private int platformCounterSinceLastStar = 0;
    private int nextStarThreshold; // randomly chosen between 1..3

    // POWER-UP LOGIC:
    private int platformCounterSinceLastPowerUp = 0;
    private const int powerUpSpawnInterval = 13;

    // Count platforms to determine when to spawn a special platform
    private int platformCounterSinceLastSpecial = 0;
    // How many platforms (rows) to wait before spawning a special platform.
    // You can set this value in the Inspector (or make it random).
    public int specialPlatformSpawnInterval = 3;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Start platforms slightly below the player
        highestPlatformY = player.position.y - 2f;

        Move.platformsJumped = 0;
        lowestPlatformY = 0;

        if (!zoneManager)
        {
            return;
        }

        nextStarThreshold = Random.Range(1, 3);

        // Spawn initial platforms
        for (int i = 0; i < numberOfStartingPlatforms; i++)
        {
            SpawnPlatform();
        }

        // Initialize lowest platform
        if (platforms.Count > 0)
        {
            lowestPlatformY = platforms[0].transform.position.y;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Spawn a new platform ONLY when the player moves up enough
        if (player.position.y + 10f > highestPlatformY)
        {
            SpawnPlatform();
        }

        // Remove old platforms
        RemoveOldPlatforms();
    }

    void SpawnPlatform()
    {
        // Determine current zone and pick a platform prefab
        ZoneDefinition currentZone = zoneManager.GetCurrentZone();
        if (currentZone == null ||
            currentZone.zonePlatformPrefabs == null ||
            currentZone.zonePlatformPrefabs.Length == 0)
        {
            return;
        }

        float xPos = Random.Range(minX, maxX);
        float yPos = highestPlatformY + verticalSpacing;
        highestPlatformY = yPos;

        int randIndex = Random.Range(0, currentZone.zonePlatformPrefabs.Length);
        GameObject platformPrefab = currentZone.zonePlatformPrefabs[randIndex];

        // Instantiate the new platform
        GameObject newPlatform = Instantiate(platformPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
        platforms.Add(newPlatform);

        // Add our helper component to track item attachment.
        PlatformItemAttachment pia = newPlatform.AddComponent<PlatformItemAttachment>();
        pia.hasItem = false;

        // Update the lowest platform if needed
        UpdateLowestPlatform();

        // STAR LOGIC:
        // Only attach a star if this platform has no item already.
        if (!pia.hasItem && platformCounterSinceLastStar >= nextStarThreshold)
        {
            AttachStar(newPlatform);
            pia.hasItem = true;
            platformCounterSinceLastStar = 0;
            nextStarThreshold = Random.Range(1, 3);
        }
        else
        {
            platformCounterSinceLastStar++;
        }

        //-----------------------------
        // POWER-UP LOGIC:
        // Only attach a power-up if no item is attached.
        //-----------------------------
        if (!pia.hasItem && platformCounterSinceLastPowerUp >= powerUpSpawnInterval)
        {
            AttachRandomPowerUp(newPlatform);
            pia.hasItem = true;
            platformCounterSinceLastPowerUp = 0;
        }
        else
        {
            platformCounterSinceLastPowerUp++;
        }

        // SPECIAL PLATFORM LOGIC:
        if (!pia.hasItem && platformCounterSinceLastSpecial >= specialPlatformSpawnInterval)
        {
            // Check if current zone has special platform prefabs
            if (currentZone.zoneSpecialPlatformPrefabs != null && currentZone.zoneSpecialPlatformPrefabs.Length > 0)
            {
                int randSpecialIndex = Random.Range(0, currentZone.zoneSpecialPlatformPrefabs.Length);
                GameObject specialPrefab = currentZone.zoneSpecialPlatformPrefabs[randSpecialIndex];
                if (specialPrefab != null)
                {
                    // Use start position instead of modifying newPlatform's transform directly
                    Vector3 startPosition = newPlatform.transform.position; // Capture the initial position

                    // Calculate the special platform position using the start position
                    Vector3 specialPlatformPos = startPosition + new Vector3(0f, specialPlatformYOffset, 0f);

                    // Instantiate the special platform at the modified start position
                    GameObject specialPlatform = Instantiate(specialPrefab, specialPlatformPos, Quaternion.identity);

                    // Attach the special platform as a child of the regular platform so it moves with it.
                    specialPlatform.transform.SetParent(newPlatform.transform, true);

                    // Add the special platform to the platforms list for removal later.
                    platforms.Add(specialPlatform);
                    pia.hasItem = true;
                }
            }
            platformCounterSinceLastSpecial = 0;
        }
        else
        {
            platformCounterSinceLastSpecial++;
        }
    }

    // STAR: Positions a star in the center (X) of the platform,
    // slightly above so it doesn't overlap.
    void AttachStar(GameObject platform)
    {
        if (!starPrefab) return;

        Vector3 starPosition = platform.transform.position + new Vector3(0f, 0.6f, 0f);
        GameObject star = Instantiate(starPrefab, starPosition, Quaternion.identity);
        // Make the star a child of the platform so it moves if the platform moves.
        star.transform.SetParent(platform.transform, true);
    }

    // POWER-UP: Picks a random power-up from your array (Magnet, Shield, Jetpack, etc.)
    // and spawns it above the platform.
    void AttachRandomPowerUp(GameObject platform)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        int rndIndex = Random.Range(0, powerUpPrefabs.Length);
        GameObject powerUpPrefab = powerUpPrefabs[rndIndex];
        if (!powerUpPrefab) return;

        // Position the power-up 1 unit above the platform.
        Vector3 powerUpPos = platform.transform.position + new Vector3(0f, 1f, 0f);
        GameObject newPowerUp = Instantiate(powerUpPrefab, powerUpPos, Quaternion.identity);
        // Parent it so that if the platform moves, the power-up follows.
        newPowerUp.transform.SetParent(platform.transform, true);
    }

    void RemoveOldPlatforms()
    {
        for (int i = platforms.Count - 1; i >= 0; i--)
        {
            // Check if this platform has already been destroyed
            if (platforms[i] == null)
            {
                platforms.RemoveAt(i);
                continue;
            }

            if (platforms[i].transform.position.y < player.position.y - 10f)
            {
                // If the player is parented to this platform, un-parent.
                if (player.parent == platforms[i].transform)
                {
                    player.SetParent(null);
                }
                Destroy(platforms[i]);
                platforms.RemoveAt(i);
            }
        }
        UpdateLowestPlatform();
    }

    private void UpdateLowestPlatform()
    {
        // Remove any null entries from the list before updating the lowest platform
        platforms.RemoveAll(item => item == null);

        if (platforms.Count > 0)
        {
            lowestPlatformY = platforms[0].transform.position.y;
            foreach (GameObject platform in platforms)
            {
                if (platform.transform.position.y < lowestPlatformY)
                {
                    lowestPlatformY = platform.transform.position.y;
                }
            }
        }
    }
}