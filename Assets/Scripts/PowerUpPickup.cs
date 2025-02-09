using UnityEngine;
using System.Collections;

public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public float powerUpDuration = 10f;
    public Vector3 playerHeadOffset = new Vector3(-0.5f, 0f, 0f);

    public MagnetPowerUp magnetScript;
    public ShieldPowerUp shieldScript;

    private bool isPickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;
            Debug.Log("Power-Up collected!");

            // Attach to player
            transform.SetParent(other.transform, true);
            transform.localScale = Vector3.one * 2f;
            transform.localPosition = playerHeadOffset;

            // Disable collider
            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;

            // Activate magnet
            if (magnetScript != null)
            {
                Debug.Log("Activating magnet...");
                magnetScript.ActivateMagnet();
            }
            else if (shieldScript != null)
            {
                Debug.Log("Activating shield...");
                shieldScript.ActivateShield();
            }
            else
            {
                Debug.LogError("No power-up script assigned!");
            }

            StartCoroutine(PowerUpActiveRoutine());
        }
    }

    private IEnumerator PowerUpActiveRoutine()
    {
        yield return new WaitForSeconds(powerUpDuration);

        if (magnetScript != null)
        {
            magnetScript.DisableMagnet();
        }
        else if (shieldScript != null)
        {
            shieldScript.DisableShield();
        }

        Destroy(gameObject);
    }
}