using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public int currentBusIndex = 0;
    public GameObject[] busModels;
    public Buying[] buses;
    public Button buyButton;
    public Button startGameButton;
    public AudioClip startsound;
    private AudioSource audioSource;
    // Store the original positions and rotations of each bus model
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;


    private void PlayClickSound()
    {
        if (startsound != null)
        {
            audioSource.PlayOneShot(startsound);
        }
    }
    void Start()
    {
        // Add or get an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Initialize bus unlock states based on price and PlayerPrefs
        foreach (Buying bus in buses)
        {
            bus.isUnlocked = bus.price == 0 || PlayerPrefs.GetInt(bus.name, 0) == 1;
        }

        // Initialize arrays to store the original positions and rotations
        originalPositions = new Vector3[busModels.Length];
        originalRotations = new Quaternion[busModels.Length];

        for (int i = 0; i < busModels.Length; i++)
        {
            // Store the original position and rotation of each model
            originalPositions[i] = busModels[i].transform.position;
            originalRotations[i] = busModels[i].transform.rotation;

            // Deactivate all models initially
            busModels[i].SetActive(false);
        }

        // Load the selected bus from PlayerPrefs
        currentBusIndex = PlayerPrefs.GetInt("Selectedbus", 0);
        ActivateBus(currentBusIndex);
    }

    public void ChangeNext()
    {
        DeactivateCurrentBus();
        PlayClickSound();
        // Increment the index
        currentBusIndex++;
        if (currentBusIndex >= busModels.Length)
        {
            currentBusIndex = 0; // Loop back to the first bus
        }

        ActivateBus(currentBusIndex);
        PlayerPrefs.SetInt("Selectedbus", currentBusIndex);
    }

    public void ChangePrevious()
    {
        PlayClickSound();
        DeactivateCurrentBus();

        // Decrement the index
        currentBusIndex--;
        if (currentBusIndex < 0)
        {
            currentBusIndex = busModels.Length - 1; // Loop back to the last bus
        }

        ActivateBus(currentBusIndex);
        PlayerPrefs.SetInt("Selectedbus", currentBusIndex);
    }

    private void DeactivateCurrentBus()
    {
        // Deactivate the current bus
        if (busModels[currentBusIndex] != null)
        {
            busModels[currentBusIndex].SetActive(false);

            // Reset its position and rotation to the original values
            busModels[currentBusIndex].transform.position = originalPositions[currentBusIndex];
            busModels[currentBusIndex].transform.rotation = originalRotations[currentBusIndex];
        }
    }

    public void StartGame()
    {
        PlayClickSound();
        SceneManager.LoadScene("maps");
    }

    private void ActivateBus(int index)
    {
        // Activate the specified bus
        if (busModels[index] != null)
        {
            busModels[index].SetActive(true);
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        Buying currentBus = buses[currentBusIndex];
        if (currentBus == null || buyButton == null || startGameButton == null) return;

        // Update Buy Button
        if (currentBus.isUnlocked)
        {
            buyButton.gameObject.SetActive(false); // Hide the buy button if unlocked
            startGameButton.interactable = true; // Enable Start Game button
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            buyButton.GetComponentInChildren<Text>().text = "Buy: " + currentBus.price;

            // Enable or disable the buy button based on available cash
            int playerCash = PlayerPrefs.GetInt("Cash", 0);
            buyButton.interactable = currentBus.price <= playerCash;

            startGameButton.interactable = false; // Disable Start Game button
        }
    }

    public void UnlockBus()
    {
        Buying currentBus = buses[currentBusIndex];
        if (currentBus == null) return;

        PlayerPrefs.SetInt(currentBus.name, 1);
        PlayerPrefs.SetInt("Selectedbus", currentBusIndex);
        currentBus.isUnlocked = true;

        int updatedCash = PlayerPrefs.GetInt("Cash", 0) - currentBus.price;
        PlayerPrefs.SetInt("Cash", updatedCash);

        UpdateUI();
    }
}
