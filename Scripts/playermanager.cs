using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class playermanager : MonoBehaviour
{
    // Start is called before the first frame update
    public Text coins;
    public int currectcoins=00;

    void Start()
    {
        actionAudioSource = gameObject.AddComponent<AudioSource>();
         
        if (PlayerPrefs.HasKey("Cash"))
        {
            currectcoins = PlayerPrefs.GetInt("Cash");
        }
        else
        {

        }
        coins.text = ":" + currectcoins;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [SerializeField] private AudioClip boosterSound; // Boost activation sound
    private AudioSource actionAudioSource; // For engine sound
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("coin"))
        {
            if (boosterSound != null)
            {
                actionAudioSource.clip = boosterSound;
                actionAudioSource.Play();
            }
            Destroy(other.gameObject);
            currectcoins += 1;
            PlayerPrefs.SetInt("Cash", currectcoins);
            coins.text = ":" + currectcoins;

        }
    }
}
