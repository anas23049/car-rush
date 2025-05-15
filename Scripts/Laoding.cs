using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField] private GameObject loadingUI; // The loading UI canvas
    [SerializeField] private GameObject mainMenu; // Main menu UI
    [SerializeField] private Slider loadingSlider; // Loading slider
    [SerializeField] private float loadingDuration = 10.0f; // Total duration for the loading screen
    [SerializeField] private AudioClip clickSound; // Sound to play on button click
    private AudioSource audioSource;
    private float originalVolume = 1.0f; // Store original volume

    void Start()
    {
        // Add or get an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure the UI starts in the correct state
        loadingUI.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Called when the load level button is clicked
    public void LoadLevelButton(string levelToLoad)
    {
        PlayClickSound(); // Play the click sound
        mainMenu.SetActive(false); // Hide the main menu
        loadingUI.SetActive(true); // Show the loading UI

        // Save the current volume and mute sounds globally
        originalVolume = AudioListener.volume;
        AudioListener.volume = 0; // Mute all sounds globally

        // Start the loading process
        StartCoroutine(LoadLevelWithTimer(levelToLoad));
    }

    private IEnumerator LoadLevelWithTimer(string levelToLoad)
    {
        // Begin loading the scene asynchronously
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false; // Prevent automatic scene activation

        float elapsedTime = 0f;

        // Simulate the slider filling over the duration
        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.deltaTime;
            float progressValue = Mathf.Clamp01(elapsedTime / loadingDuration);
            loadingSlider.value = progressValue;
            yield return null;
        }

        // Ensure the slider is full
        loadingSlider.value = 1.0f;

        // Restore the volume before activating the new scene
        AudioListener.volume = originalVolume;

        // Activate the new scene after the timer completes
        loadOperation.allowSceneActivation = true;
    }
}
