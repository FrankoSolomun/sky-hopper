using UnityEngine;
using System.Collections;

// Define the type of power-up this pickup represents
public enum PowerUpType { Magnet, Shield, Jetpack }

public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public float powerUpDuration = 10f;
    public Vector3 playerHeadOffset = new Vector3(-0.5f, 0f, 0f);

    // Select the type of power-up in the Inspector (Magnet, Shield, or Jetpack)
    public PowerUpType powerUpType;

    public MagnetPowerUp magnetScript;
    public ShieldPowerUp shieldScript;
    // (No pre-assignment for Jetpack is needed since we'll auto-assign it from the player)

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
            else if (powerUpType == PowerUpType.Jetpack)
            {
                // For Jetpack, we auto-assign the JetpackPowerUp component from the player.
                JetpackPowerUp jetpackScript = other.GetComponent<JetpackPowerUp>();
                Debug.Log("Auto-assigned jetpackScript: " + (jetpackScript != null));
                if (jetpackScript != null)
                {
                    Debug.Log("Activating jetpack...");
                    jetpackScript.ActivateJetpack();
                }
                else
                {
                    Debug.LogError("No jetpack script assigned on player!");
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
        else if (powerUpType == PowerUpType.Jetpack)
        {
            // For Jetpack, disable it when the duration expires.
            JetpackPowerUp jetpackScript = GetComponentInParent<JetpackPowerUp>();
            if (jetpackScript != null)
            {
                jetpackScript.DisableJetpack();
            }
        }

        Destroy(gameObject);
    }
}