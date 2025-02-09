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

    private float highestPlatformY;
    public static float lowestPlatformY;
    private Transform player;
    private List<GameObject> platforms = new List<GameObject>();

    // STAR LOGIC:
    private int platformCounterSinceLastStar = 0;
    private int nextStarThreshold; // randomly chosen between 7..13

    // POWER-UP LOGIC:
    private int platformCounterSinceLastPowerUp = 0;
    private const int powerUpSpawnInterval = 3; // spawn a power-up every 25 platforms

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

        // Initialize the first random threshold for star spawning (7..13)
        nextStarThreshold = Random.Range(2, 5);

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

        // Update the lowest platform if needed
        UpdateLowestPlatform();

        //-----------------------------
        // STAR LOGIC
        //-----------------------------
        platformCounterSinceLastStar++;
        if (platformCounterSinceLastStar >= nextStarThreshold)
        {
            AttachStar(newPlatform);
            // Reset the count and choose a new threshold
            platformCounterSinceLastStar = 0;
            nextStarThreshold = Random.Range(2, 5);
        }

        //-----------------------------
        // POWER-UP LOGIC
        //-----------------------------
        platformCounterSinceLastPowerUp++;
        if (platformCounterSinceLastPowerUp >= powerUpSpawnInterval)
        {
            AttachRandomPowerUp(newPlatform);
            platformCounterSinceLastPowerUp = 0;
        }
    }

    // STAR: Positions a star in the center (X) of the platform,
    // slightly above so it doesn't overlap
    void AttachStar(GameObject platform)
    {
        if (!starPrefab) return;

        Vector3 starPosition = platform.transform.position + new Vector3(0f, 0.6f, 0f);

        GameObject star = Instantiate(starPrefab, starPosition, Quaternion.identity);

        // Make the star a child of the platform so it moves if the platform moves
        star.transform.SetParent(platform.transform, true);
    }

    // POWER-UP: Picks a random power-up from your array (Magnet, Shield, Jetpack, etc.)
    // and spawns it above the platform
    void AttachRandomPowerUp(GameObject platform)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        int rndIndex = Random.Range(0, powerUpPrefabs.Length);
        GameObject powerUpPrefab = powerUpPrefabs[rndIndex];
        if (!powerUpPrefab) return;

        // Position the power-up 1 unit above the platform
        Vector3 powerUpPos = platform.transform.position + new Vector3(0f, 1f, 0f);

        GameObject newPowerUp = Instantiate(powerUpPrefab, powerUpPos, Quaternion.identity);

        // parent it so if the platform moves, the power-up follows
        newPowerUp.transform.SetParent(platform.transform, true);
    }

    void RemoveOldPlatforms()
    {
        for (int i = platforms.Count - 1; i >= 0; i--)
        {
            if (platforms[i].transform.position.y < player.position.y - 10f)
            {
                // If the player is parented to this platform, un-parent
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
        if (platforms.Count > 0)
        {
            float oldLowest = lowestPlatformY;
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