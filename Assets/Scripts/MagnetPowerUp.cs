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
        }
    }

    public void ActivateMagnet()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;

        // Show icon
        if (magnetIconPrefab != null && player != null)
        {
            magnetIconInstance = Instantiate(magnetIconPrefab, player.position + iconOffset, Quaternion.identity);
            magnetIconInstance.transform.SetParent(player, true);
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

        foreach (StarPickup star in allStars)
        {
            if (!star)
            {
                continue;
            }

            float dist = Vector3.Distance(star.transform.position, player.position);

            if (dist < magnetRange)
            {
                star.transform.SetParent(null); // Unparent

                if (star.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 direction = (player.position - star.transform.position).normalized;
                    rb.linearVelocity = direction * pullSpeed;
                }
                else
                {
                    Vector3 direction = (player.position - star.transform.position).normalized;
                    star.transform.position += direction * pullSpeed * Time.deltaTime;
                }
            }
        }
    }
}