using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitCamera : MonoBehaviour
{
    public GameObject player; // Reference to the player object
    public float fixedHeight = 2000f; // Fixed height for the minimap camera

    // Update is called once per frame
    void LateUpdate()
    {
        if (player != null)
        {
            // Follow the player's position but maintain a fixed height
            transform.position = new Vector3(player.transform.position.x, fixedHeight, player.transform.position.z);

            // Stabilize the rotation to prevent it from following the player's rotation
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Top-down view, no tilt
        }
    }
}
