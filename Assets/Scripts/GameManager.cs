using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject startMenuUI; // Declare the variable
    public bool isPaused = true;

    void Update()
    {
        if (isPaused)
        {
            Time.timeScale = 0; // Pause game time
        }
        else
        {
            Time.timeScale = 1; // Resume game time
        }
    }

    public void StartGame()
    {
        isPaused = false;
        startMenuUI.SetActive(false); // Hide the start menu
    }

    public void PauseGame()
    {
        isPaused = true;
        startMenuUI.SetActive(true); // Show the start menu
    }
}