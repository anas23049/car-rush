using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class garagecar : MonoBehaviour
{
    public GameObject smokeEffect; // Smoke prefab to instantiate
    public AudioClip collisionSound; // Sound to play on collision
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the surface and involves the car tires
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.CompareTag("Surface") && IsCarTire(contact.thisCollider))
            {
                // Instantiate the smoke effect at the contact point
                GameObject smoke = Instantiate(smokeEffect, contact.point, Quaternion.identity);

                // Destroy the smoke after 2 seconds
                Destroy(smoke, 1f);

                // Play the collision sound
                audioSource.PlayOneShot(collisionSound);

                break; // Exit the loop after handling the first valid collision
            }
        }
    }

    bool IsCarTire(Collider collider)
    {
        // Check if the collider belongs to the car's tires (adjust based on your setup)
        return collider.CompareTag("Wheel"); // Ensure tires have the tag "Wheel"
    }
}
