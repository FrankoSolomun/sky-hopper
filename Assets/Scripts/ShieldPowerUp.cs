using UnityEngine;
using System.Collections;

public class ShieldPowerUp : MonoBehaviour
{
    private bool isActive = false;
    private Transform player;

    [Header("Shield Animation")]
    public GameObject shieldObject; // Assign the shield GameObject in the inspector
    public float shieldDuration = 10f; // Duration of the shield

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Ensure the shield is initially disabled
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
    }

    public void ActivateShield()
    {
        if (isActive) return; // Do nothing if the shield is already active
        isActive = true;

        // Enable the shield GameObject
        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
            Debug.Log("Shield Activated");
        }

        // Start the timer to disable the shield
        StartCoroutine(DisableAfterTime(shieldDuration));
    }

    public bool IsShieldActive()
    {
        return isActive;
    }

    public void DisableShield()
    {
        if (!isActive) return; // Do nothing if the shield is not active
        isActive = false;

        // Disable the shield GameObject
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
            Debug.Log("Shield Deactivated");
        }
    }

    private IEnumerator DisableAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        DisableShield();
    }
}