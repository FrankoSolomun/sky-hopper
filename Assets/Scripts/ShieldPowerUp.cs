using UnityEngine;
using System.Collections;

public class ShieldPowerUp : MonoBehaviour
{
    private bool isActive = false;
    private Transform player;
    private GameObject shieldIconInstance;

    [Header("Shield Settings")]
    public GameObject shieldIconPrefab;
    public Vector3 iconOffset = new Vector3(0, 1, 0);

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void ActivateShield()
    {
        if (isActive) return;
        isActive = true;

        // Show shield icon above the player
        if (shieldIconPrefab != null)
        {
            shieldIconInstance = Instantiate(shieldIconPrefab, player.position + iconOffset, Quaternion.identity);
            shieldIconInstance.transform.SetParent(player, true);
        }

        StartCoroutine(DisableAfterTime(10f));
    }

    public bool IsShieldActive()
    {
        return isActive;
    }

    public void DisableShield()
    {
        isActive = false;

        // Remove the shield icon
        if (shieldIconInstance != null)
        {
            Destroy(shieldIconInstance);
        }
    }

    private IEnumerator DisableAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        DisableShield();
    }
}
