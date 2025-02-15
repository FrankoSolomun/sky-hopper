using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    // Reference to ZoneManager
    public ZoneManager zoneManager;

    // Reference to StarPrefab (assign in Inspector)
    public GameObject starPrefab;
    public RuntimeAnimatorController starAnimatorController; // Assign your Animator Controller in the Inspector.

    // Reference to PowerUpPrefabs (3 prefabs: Magnet, Shield, Jetpack, etc.)
    public GameObject[] powerUpPrefabs;

    // Reference to Rain Prefab (assign in Inspector)
    public GameObject rainPrefab;

    // Reference to ElecCloud and Lightning Prefabs (assign in Inspector)
    public GameObject elecCloudPrefab; // Add this line
    public GameObject lightningPrefab; // Add this line

    public int numberOfStartingPlatforms = 5;
    public float verticalSpacing = 3f;
    public float minX = -2f, maxX = 2f;

    // This determines how far above the regular platform the special prefab should be placed.
    public float specialPlatformYOffset = 1f;

    // This determines the range for the X-axis offset of the special platform.
    public float specialPlatformXOffsetRange = 1f;

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
            Debug.LogError("ZoneManager is not assigned!");
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
        if (currentZone == null)
        {
            Debug.LogError("Current zone is null!");
            return;
        }

        Debug.Log("Current Zone: " + currentZone.zoneName); // Debug the zone name

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
                    float specialPlatformXOffset = Random.Range(-specialPlatformXOffsetRange, specialPlatformXOffsetRange);
                    Vector3 specialPlatformPos = startPosition + new Vector3(specialPlatformXOffset, specialPlatformYOffset, 0f);

                    // Instantiate the special platform at the modified start position
                    GameObject specialPlatform = Instantiate(specialPrefab, specialPlatformPos, Quaternion.identity);

                    // Apply custom offset for specific prefabs
                    if (specialPrefab.name == "antena")
                    {
                         // Add a trigger collider to the special platform
                        BoxCollider2D triggerCollider = specialPlatform.AddComponent<BoxCollider2D>();
                        triggerCollider.isTrigger = true;
                    
                        // Add the SpecialPlatformEffect script to the special platform
                        SpecialPlatformEffect specialEffect = specialPlatform.AddComponent<SpecialPlatformEffect>();
                        specialEffect.pushForce = 5f; // Adjust as needed
                        specialEffect.movementResistance = 2f; // Adjust as needed
                        specialPlatform.transform.position += new Vector3(1.1f, 1.1f, 0f); // Custom offset for this prefab
                    }

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

        // RAIN, ELECCLOUD, AND LIGHTNING LOGIC: Attach to specific platforms in zone2
        if (currentZone.zoneName == "StormZone")
        {
            // Check if the platform has a specific name
            if (newPlatform.name.Contains("02cloud 64 1"))
            {
                AttachRainToCloud(newPlatform);

                // Add a random chance to attach elecCloud (e.g., 50% chance)
                float elecCloudChance = 0.5f; // 50% chance of elecCloud
                if (Random.value < elecCloudChance)
                {
                    AttachElecCloud(newPlatform);
                }

                // Add a random chance to attach lightning (e.g., 50% chance)
                float lightningChance = 0.5f; // 50% chance of lightning
                if (Random.value < lightningChance)
                {
                    AttachLightning(newPlatform);
                }
            }
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

         // Add an Animator component to the star if it doesn't already have one.
        Animator starAnimator = star.GetComponent<Animator>();
        if (!starAnimator)
        {
            starAnimator = star.AddComponent<Animator>();
        }

        // Assign the Animator Controller.
        starAnimator.runtimeAnimatorController = starAnimatorController;

        // If you want to play the animation immediately:
        starAnimator.Play("StarAnimation");
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

    // RAIN: Attaches rain prefab to clouds in zone2
    void AttachRainToCloud(GameObject cloud)
    {
        if (!rainPrefab)
        {
            Debug.LogError("Rain Prefab is not assigned!");
            return;
        }

        Debug.Log("Attaching rain to platform: " + cloud.name);

        // Position the rain slightly below the cloud
        Vector3 rainPosition = cloud.transform.position + new Vector3(0f, -1f, 0f);
        GameObject rain = Instantiate(rainPrefab, rainPosition, Quaternion.identity);

        // Make the rain a child of the cloud so it moves with it
        rain.transform.SetParent(cloud.transform, true);
    }

    // ELECCLOUD: Attaches elecCloud prefab to clouds in zone2
    void AttachElecCloud(GameObject cloud)
    {
        if (!elecCloudPrefab)
        {
            Debug.LogError("ElecCloud Prefab is not assigned!");
            return;
        }

        Debug.Log("Attaching elecCloud to platform: " + cloud.name);

        // Position the elecCloud slightly above the cloud
        Vector3 elecCloudPosition = cloud.transform.position + new Vector3(0f, 0f, 0f);
        GameObject elecCloud = Instantiate(elecCloudPrefab, elecCloudPosition, Quaternion.identity);

        // Make the elecCloud a child of the cloud so it moves with it
        elecCloud.transform.SetParent(cloud.transform, true);
    }

    // LIGHTNING: Attaches lightning prefab to clouds in zone2
    void AttachLightning(GameObject cloud)
    {
        if (!lightningPrefab)
        {
            Debug.LogError("Lightning Prefab is not assigned!");
            return;
        }

        Debug.Log("Attaching lightning to platform: " + cloud.name);

        // Position the lightning slightly below the cloud
        Vector3 lightningPosition = cloud.transform.position + new Vector3(0f, -1f, 0f);
        GameObject lightning = Instantiate(lightningPrefab, lightningPosition, Quaternion.identity);

        // Make the lightning a child of the cloud so it moves with it
        lightning.transform.SetParent(cloud.transform, true);
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