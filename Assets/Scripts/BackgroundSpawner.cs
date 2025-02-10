using UnityEngine;
using System.Collections.Generic;

public class BackgroundSpawner : MonoBehaviour
{
    public ZoneManager zoneManager; // Manages different zones in the game

    // Number of backgrounds initially visible on the screen
    public int numberOfBackgrounds = 3;

    // The number of backgrounds to keep below the camera before removing them
    public int backgroundsToKeepBelow = 2;

    // The number of backgrounds to keep above the camera to prevent gaps
    public int backgroundsToKeepAbove = 1;

    private float backgroundHeight; // Height of each background sprite
    private List<GameObject> backgrounds = new List<GameObject>(); // List of active background objects
    private Transform player; // Reference to the player's position

    // Zone logic to track which backgrounds have been used in different zones
    private Dictionary<ZoneDefinition, bool> zoneUsedFirstBackground = new Dictionary<ZoneDefinition, bool>();
    private Dictionary<ZoneDefinition, int> zoneNextRepeatedIndex = new Dictionary<ZoneDefinition, int>();

    private int nextPrefabIndex = 0; // Index for cycling background prefabs

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Calculate camera dimensions in world units
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Screen.width / (float)Screen.height;

        // Set background height to the camera height
        backgroundHeight = cameraHeight;

        if (!zoneManager) return;

        ZoneDefinition currentZone = zoneManager.GetCurrentZone();
        if (currentZone == null) return;
        InitializeZoneIfNeeded(currentZone);

        // Ensure the zone has enough background prefabs
        if (currentZone.zoneBackgrounds == null || currentZone.zoneBackgrounds.Length < 3)
        {
            return;
        }

        // Positioning logic: Start placing backgrounds from the bottom of the camera
        float cameraBottomY = Camera.main.transform.position.y - (cameraHeight * 0.5f);
        float firstBGCenterY = cameraBottomY + (backgroundHeight * 0.5f);

        // Spawn initial backgrounds on the screen
        for (int i = 0; i < numberOfBackgrounds; i++)
        {
            float yPos = firstBGCenterY + i * backgroundHeight;
            Vector3 spawnPosition = new Vector3(0f, yPos, 0f);

            GameObject bgPrefab = GetNextBackgroundPrefab(currentZone);
            if (!bgPrefab) continue;

            GameObject newBG = Instantiate(bgPrefab, spawnPosition, Quaternion.identity);

            // Adjust background scale to fit camera dimensions
            SpriteRenderer sr = newBG.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float spriteW = sr.sprite.bounds.size.x;
                float spriteH = sr.sprite.bounds.size.y;
                float scaleX = cameraWidth / spriteW;
                float scaleY = backgroundHeight / spriteH;
                newBG.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            backgrounds.Add(newBG);
        }

        nextPrefabIndex = numberOfBackgrounds;
    }

    void Update()
    {
        if (!player || backgrounds.Count == 0) return;

        // Remove old backgrounds below the camera
        RemoveOldBackgroundsIfNeeded();

        // Spawn new backgrounds above the camera when necessary
        AddNewBackgroundsIfNeeded();
    }

    private void RemoveOldBackgroundsIfNeeded()
    {
        float cameraBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;

        // Remove backgrounds that have moved too far below the camera
        while (backgrounds.Count > 0)
        {
            GameObject bottomBG = backgrounds[0];
            float bottomBGCenterY = bottomBG.transform.position.y;
            float bottomBGTopY = bottomBGCenterY + (backgroundHeight * 0.5f);
            float removeThresholdY = bottomBGTopY + (backgroundHeight * backgroundsToKeepBelow);

            if (cameraBottomY > removeThresholdY)
            {
                backgrounds.RemoveAt(0);
                Destroy(bottomBG);
            }
            else
            {
                // Stop checking if we reach a background that should be kept
                break;
            }
        }
    }

    private void AddNewBackgroundsIfNeeded()
    {
        float cameraTopY = Camera.main.transform.position.y + Camera.main.orthographicSize;

        while (true)
        {
            GameObject topBG = backgrounds[backgrounds.Count - 1];
            float topBGCenterY = topBG.transform.position.y;
            float topBGTopY = topBGCenterY + (backgroundHeight * 0.5f);
            float addThresholdY = topBGTopY - (backgroundHeight * backgroundsToKeepAbove);

            if (cameraTopY > addThresholdY)
            {
                ZoneDefinition currentZone = zoneManager.GetCurrentZone();
                if (!currentZone) return;
                InitializeZoneIfNeeded(currentZone);

                GameObject newBGPrefab = GetNextBackgroundPrefab(currentZone);
                if (!newBGPrefab) return;

                float newBGCenterY = topBGTopY + (backgroundHeight * 0.5f);
                GameObject newBG = Instantiate(newBGPrefab, new Vector3(0f, newBGCenterY, 0f), Quaternion.identity);

                // Adjust new background scale to fit camera dimensions
                SpriteRenderer sr = newBG.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float cameraHeight = Camera.main.orthographicSize * 2f;
                    float cameraWidth = cameraHeight * Screen.width / (float)Screen.height;
                    float spriteW = sr.sprite.bounds.size.x;
                    float spriteH = sr.sprite.bounds.size.y;
                    float scaleX = cameraWidth / spriteW;
                    float scaleY = cameraHeight / spriteH;
                    newBG.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }

                backgrounds.Add(newBG);
            }
            else
            {
                // No need to add more backgrounds
                break;
            }
        }
    }

    private void InitializeZoneIfNeeded(ZoneDefinition zone)
    {
        if (!zoneUsedFirstBackground.ContainsKey(zone))
        {
            zoneUsedFirstBackground[zone] = false;
            zoneNextRepeatedIndex[zone] = 1;
        }
    }

    private GameObject GetNextBackgroundPrefab(ZoneDefinition zone)
    {
        if (zone.zoneBackgrounds == null || zone.zoneBackgrounds.Length < 3) return null;

        // Ensure the first background is used before cycling
        if (!zoneUsedFirstBackground[zone])
        {
            zoneUsedFirstBackground[zone] = true;
            return zone.zoneBackgrounds[0];
        }
        else
        {
            // Alternate between the second and third background
            int currentIndex = zoneNextRepeatedIndex[zone];
            GameObject bg = zone.zoneBackgrounds[currentIndex];

            zoneNextRepeatedIndex[zone] = (currentIndex == 1) ? 2 : 1;
            return bg;
        }
    }
}
