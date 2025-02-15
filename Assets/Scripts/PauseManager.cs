using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Assign the pause menu Panel in the Inspector
    public GameObject startMenuUI;
    public Move playerMoveScript; // Assign the player's Move script in the Inspector
    private bool isPaused = false;

    void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Move.isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        Move.ResumeGame(); // Resume the game and unfreeze the player
        pauseMenuUI.SetActive(false); // Hide the pause menu
    }

    void PauseGame()
    {
        Move.PauseGame(); // Pause the game and freeze the player
        pauseMenuUI.SetActive(true); // Show the pause menu

    }

    public void BackToHome()
    {
        // Hide the pause menu and show the home screen
        pauseMenuUI.SetActive(false);
        startMenuUI.SetActive(true);

        // Restart the game
        if (playerMoveScript != null)
        {
            playerMoveScript.RestartGame();
        }
        else
        {
            Debug.LogError("Player Move script is not assigned in the PauseManager.");
        }
    }
}