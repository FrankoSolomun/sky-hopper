using UnityEngine;
using TMPro;

public class StarManager : MonoBehaviour
{
    public static StarManager Instance;  // Singleton reference

    // Assign this in the Inspector to your TextMeshProUGUI field
    public TextMeshProUGUI starCounterText;

    private int totalStars = 0;

    void Awake()
    {
        // Standard singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // (Optional) keep this across scene loads
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize the UI text to "Stars: 0" at game start
        if (starCounterText)
        {
            starCounterText.text = "Stars: 0";
        }
    }

    public void AddStars(int amount)
    {
        totalStars += amount;

        if (starCounterText)
        {
            starCounterText.text = "Stars: " + totalStars;
        }
    }

    // (Optional) Provide a way to reset stars to 0:
    public void ResetStars()
    {
        totalStars = 0;
        if (starCounterText)
        {
            starCounterText.text = "Stars: 0";
        }
    }
}