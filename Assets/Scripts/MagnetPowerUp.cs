using UnityEngine;
using System.Collections;

public class MagnetPowerUp : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float magnetRange = 6f;         // How far the magnet pulls stars
    public float pullSpeed = 10f;          // How quickly stars move towards the player
    public float magnetDuration = 10f;     // How long the magnet stays active

    [Header("Icon / Visuals")]
    public GameObject magnetIconPrefab;    // Icon near player's head
    public Vector3 iconOffset = new Vector3(0, 1, 0);

    private Transform player;
    private bool isActive = false;
    private GameObject magnetIconInstance;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("MagnetPowerUp: Player not found!");
        }
    }

    public void ActivateMagnet()
    {
        if (isActive)
        {
            Debug.LogWarning("Magnet already active!");
            return;
        }

        isActive = true;
        Debug.Log("Magnet Activated!");

        // Show icon
        if (magnetIconPrefab != null && player != null)
        {
            magnetIconInstance = Instantiate(magnetIconPrefab, player.position + iconOffset, Quaternion.identity);
            magnetIconInstance.transform.SetParent(player, true);
            Debug.Log("Magnet icon spawned.");
        }
        else
        {
            Debug.LogWarning("Magnet icon prefab or player missing!");
        }

        StartCoroutine(DisableAfterTime(magnetDuration));
    }

    IEnumerator DisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        DisableMagnet();
    }

    public void DisableMagnet()
    {
        isActive = false;
        Debug.Log("Magnet Deactivated!");

        if (magnetIconInstance != null)
        {
            Destroy(magnetIconInstance);
        }
    }

    void Update()
    {
        if (!isActive || player == null) return;

        PullNearbyStars();
    }

    private void PullNearbyStars()
    {
        // Use FindObjectsOfType instead of FindObjectsByType for broader Unity version support
        StarPickup[] allStars = FindObjectsOfType<StarPickup>();
        Debug.Log($"Found {allStars.Length} stars in the scene.");

        foreach (StarPickup star in allStars)
        {
            if (!star)
            {
                Debug.LogWarning("Skipping null star.");
                continue;
            }

            float dist = Vector3.Distance(star.transform.position, player.position);
            Debug.Log($"Star distance: {dist} (Range: {magnetRange})");

            if (dist < magnetRange)
            {
                Debug.Log("Star in range! Pulling...");
                star.transform.SetParent(null); // Unparent

                if (star.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 direction = (player.position - star.transform.position).normalized;
                    rb.linearVelocity = direction * pullSpeed;
                    Debug.Log($"Star velocity set: {rb.linearVelocity}");
                }
                else
                {
                    Vector3 direction = (player.position - star.transform.position).normalized;
                    star.transform.position += direction * pullSpeed * Time.deltaTime;
                    Debug.Log($"Star position updated: {star.transform.position}");
                }
            }
        }
    }
}