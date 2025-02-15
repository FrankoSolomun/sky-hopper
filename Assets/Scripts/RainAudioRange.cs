using UnityEngine;

public class RainAudioRange : MonoBehaviour
{
    public float maxDistance = 5f;
    private AudioSource audioSource;
    private GameObject player;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player"); // Find player by tag
        // OR
        // player = GameObject.Find("PlayerName"); // Find player by name

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the correct tag or name.");
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            float volume = 1f - Mathf.Clamp01(distance / maxDistance);
            audioSource.volume = volume;
        }
    }
}