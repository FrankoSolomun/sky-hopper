using UnityEngine;
using System.Collections;

// Define the type of power-up this pickup represents
public enum PowerUpType { Magnet, Shield }

public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public float powerUpDuration = 10f;
    public Vector3 playerHeadOffset = new Vector3(-0.5f, 0f, 0f);

    // Select the type of power-up in the Inspector (Magnet or Shield)
    public PowerUpType powerUpType;

    public MagnetPowerUp magnetScript;
    public ShieldPowerUp shieldScript;

    private bool isPickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;
            Debug.Log("Power-Up collected! Type: " + powerUpType);

            // Attach to player
            transform.SetParent(other.transform, true);
            transform.localScale = Vector3.one * 2f;
            transform.localPosition = playerHeadOffset;

            // Disable collider
            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;

            // Activate the appropriate power-up based on the specified type
            if (powerUpType == PowerUpType.Magnet)
            {
                if (magnetScript == null)
                {
                    magnetScript = other.GetComponent<MagnetPowerUp>();
                    Debug.Log("Auto-assigned magnetScript: " + (magnetScript != null));
                }
                if (magnetScript != null)
                {
                    Debug.Log("Activating magnet...");
                    magnetScript.ActivateMagnet();
                }
                else
                {
                    Debug.LogError("No magnet script assigned!");
                }
            }
            else if (powerUpType == PowerUpType.Shield)
            {
                if (shieldScript == null)
                {
                    shieldScript = other.GetComponent<ShieldPowerUp>();
                    Debug.Log("Auto-assigned shieldScript: " + (shieldScript != null));
                }
                if (shieldScript != null)
                {
                    Debug.Log("Activating shield...");
                    shieldScript.ActivateShield();
                }
                else
                {
                    Debug.LogError("No shield script assigned!");
                }
            }

            StartCoroutine(PowerUpActiveRoutine());
        }
    }

    private IEnumerator PowerUpActiveRoutine()
    {
        yield return new WaitForSeconds(powerUpDuration);

        if (powerUpType == PowerUpType.Magnet)
        {
            if (magnetScript != null)
            {
                magnetScript.DisableMagnet();
            }
        }
        else if (powerUpType == PowerUpType.Shield)
        {
            if (shieldScript != null)
            {
                shieldScript.DisableShield();
            }
        }

        Destroy(gameObject);
    }
}