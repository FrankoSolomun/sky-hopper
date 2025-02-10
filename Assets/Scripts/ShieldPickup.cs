using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    public ShieldPowerUp shieldScript; // assign or find at runtime

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (shieldScript != null)
            {
                shieldScript.ActivateShield();
            }

            Destroy(gameObject);
        }
    }
}
