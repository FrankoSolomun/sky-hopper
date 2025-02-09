using UnityEngine;
using System.Collections.Generic;

public class BackgroundSpawner : MonoBehaviour
{
    public ZoneManager zoneManager;

    // How many backgrounds to have visible on screen at the very start
    public int numberOfBackgrounds = 3;

    // Keep this many backgrounds below the camera before removing
    public int backgroundsToKeepBelow = 2;

    // Keep this many backgrounds above the camera (pre-spawn) so you never see a gap
    public int backgroundsToKeepAbove = 1;

    private float backgroundHeight;
    private List<GameObject> backgrounds = new List<GameObject>();
    private Transform player;

    // Zone logic
    private Dictionary<ZoneDefinition, bool> zoneUsedFirstBackground = new Dictionary<ZoneDefinition, bool>();
    private Dictionary<ZoneDefinition, int> zoneNextRepeatedIndex = new Dictionary<ZoneDefinition, int>();

    private int nextPrefabIndex = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Camera dimension in world units
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Screen.width / (float)Screen.height;

        // We'll treat "backgroundHeight" as the vertical size we stack by
        backgroundHeight = cameraHeight;


        if (!zoneManager) return;

        ZoneDefinition currentZone = zoneManager.GetCurrentZone();
        if (currentZone == null) return;
        InitializeZoneIfNeeded(currentZone);

        if (currentZone.zoneBackgrounds == null || currentZone.zoneBackgrounds.Length < 3)
        {
            return;
        }

        // Positioning:
        float cameraBottomY = Camera.main.transform.position.y - (cameraHeight * 0.5f);
        float firstBGCenterY = cameraBottomY + (backgroundHeight * 0.5f);

        // Spawn the initial "numberOfBackgrounds"
        for (int i = 0; i < numberOfBackgrounds; i++)
        {
            float yPos = firstBGCenterY + i * backgroundHeight;
            Vector3 spawnPosition = new Vector3(0f, yPos, 0f);

            GameObject bgPrefab = GetNextBackgroundPrefab(currentZone);
            if (!bgPrefab) continue;

            GameObject newBG = Instantiate(bgPrefab, spawnPosition, Quaternion.identity);

            // Force the background to match camera dimensions exactly:
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

        // 1) Remove old backgrounds if camera is too far above them
        RemoveOldBackgroundsIfNeeded();

        // 2) Spawn new backgrounds above if camera is near the top
        AddNewBackgroundsIfNeeded();
    }

    private void RemoveOldBackgroundsIfNeeded()
    {
        float cameraBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;

        // We'll remove the bottom background only if the camera is above
        // (that background's top + backgroundsToKeepBelow * backgroundHeight)
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
                // The bottom background is still within the keep-below range
                break;
            }
        }
    }

    private void AddNewBackgroundsIfNeeded()
    {
        // We'll ensure there's at least 'backgroundsToKeepAbove' backgrounds above the camera top
        float cameraTopY = Camera.main.transform.position.y + Camera.main.orthographicSize;

        // We check from the topmost background upward
        while (true)
        {
            GameObject topBG = backgrounds[backgrounds.Count - 1];
            float topBGCenterY = topBG.transform.position.y;
            float topBGTopY = topBGCenterY + (backgroundHeight * 0.5f);

            // If the camera top is close to or above (topBGTopY - backgroundsToKeepAbove * backgroundHeight),
            // we spawn another background above it to avoid any gap.
            float addThresholdY = topBGTopY - (backgroundHeight * backgroundsToKeepAbove);

            if (cameraTopY > addThresholdY)
            {
                ZoneDefinition currentZone = zoneManager.GetCurrentZone();
                if (!currentZone) return;
                InitializeZoneIfNeeded(currentZone);

                GameObject newBGPrefab = GetNextBackgroundPrefab(currentZone);
                if (!newBGPrefab) return;

                // The new background's center is topBGTopY + (backgroundHeight * 0.5f)
                float newBGCenterY = topBGTopY + (backgroundHeight * 0.5f);
                GameObject newBG = Instantiate(newBGPrefab, new Vector3(0f, newBGCenterY, 0f), Quaternion.identity);

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
                // We have enough backgrounds above
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

        // If we haven't used the first BG (#0) yet, do it once
        if (!zoneUsedFirstBackground[zone])
        {
            zoneUsedFirstBackground[zone] = true;
            return zone.zoneBackgrounds[0];
        }
        else
        {
            // Cycle #1 and #2
            int currentIndex = zoneNextRepeatedIndex[zone]; // 1 or 2
            GameObject bg = zone.zoneBackgrounds[currentIndex];

            zoneNextRepeatedIndex[zone] = (currentIndex == 1) ? 2 : 1;
            return bg;
        }
    }
}