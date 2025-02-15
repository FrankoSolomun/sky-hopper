using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject startMenuUI; // Reference to the start menu UI
    public bool isPaused = true;

    public AudioSource menuAudioSource; // Audio for the start menu
    public AudioSource inGameAudioSource; // Audio for in-game background music

    private Coroutine currentFadeCoroutine; // Track the current fade coroutine

    void Start()
    {
        // Ensure the correct audio is playing based on the initial state
        if (isPaused)
        {
            PlayMenuAudio();
        }
        else
        {
            PlayInGameAudio();
        }
    }

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

        // Example: Lower volume manually when pressing the 'V' key
        if (Input.GetKeyDown(KeyCode.V))
        {
            LowerVolumeManually(menuAudioSource, 0.2f); // Lower menu audio volume to 50%
            LowerVolumeManually(inGameAudioSource, 0.1f); // Lower in-game audio volume to 50%
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            LowerVolumeManually(menuAudioSource, 0f); // Lower menu audio volume to 50%
            LowerVolumeManually(inGameAudioSource, 0f); // Lower in-game audio volume to 50%
        }
    }

    public void StartGame()
    {
        isPaused = false;
        startMenuUI.SetActive(false); // Hide the start menu

        // Switch audio to in-game background music
        PlayInGameAudio();
    }

    public void PauseGame()
    {
        isPaused = true;
        startMenuUI.SetActive(true); // Show the start menu

        // Switch audio to menu background music
        PlayMenuAudio();
    }

    private void PlayMenuAudio()
    {
        // Stop in-game audio and play menu audio
        StartCoroutine(SwitchAudio(inGameAudioSource, menuAudioSource, 0.5f)); // 1-second fade
    }

    private void PlayInGameAudio()
    {
        // Stop menu audio and play in-game audio
        StartCoroutine(SwitchAudio(menuAudioSource, inGameAudioSource, 0.5f)); // 1-second fade
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to work even when the game is paused
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private IEnumerator SwitchAudio(AudioSource audioToFadeOut, AudioSource audioToFadeIn, float fadeDuration)
    {
        // Fade out the current audio
        if (audioToFadeOut != null && audioToFadeOut.isPlaying)
        {
            yield return StartCoroutine(FadeAudio(audioToFadeOut, 0, fadeDuration));
            audioToFadeOut.Stop();
        }

        // Fade in the new audio
        if (audioToFadeIn != null)
        {
            audioToFadeIn.volume = 0; // Start from 0 volume
            audioToFadeIn.Play();
            yield return StartCoroutine(FadeAudio(audioToFadeIn, 1, fadeDuration));
        }
    }

    // Method to manually lower the volume
    public void LowerVolumeManually(AudioSource audioSource, float targetVolume)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine); // Stop the current fade coroutine
        }

        if (audioSource != null)
        {
            audioSource.volume = targetVolume; // Set the volume directly
        }
    }
}