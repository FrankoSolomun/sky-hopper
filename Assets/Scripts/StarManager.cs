using UnityEngine;
using UnityEngine.UI; // Add this namespace for UnityEngine.UI.Text

public class StarManager : MonoBehaviour
{
    public static StarManager Instance;  // Singleton reference

    // Assign this in the Inspector to your Text field
    public Text starCounterText;

    // Add a public field for the new font
    public Font newFont;

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
    }

    void Start()
    {
        // Initialize the UI text to "Stars: 0" at game start
        if (starCounterText)
        {
            starCounterText.text = "Stars: 0";

            // Change the font if a new font is assigned
            if (newFont != null)
            {
                starCounterText.font = newFont;
            }
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

    public void ResetStars()
    {
        totalStars = 0;
        if (starCounterText)
        {
            starCounterText.text = "Stars: 0";
        }
    }

    // Method to get the current star count
    public int GetStarCount()
    {
        return totalStars;
    }
}