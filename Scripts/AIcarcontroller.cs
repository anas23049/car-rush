using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AIcarcontroller : MonoBehaviour
{
    [SerializeField] private Transform destination; // The target destination for the AI car
    private NavMeshAgent navMeshAgent;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private float boosterForceMultiplier = 6f; // Multiplier for speed boost
    [SerializeField] private float boosterDuration = 20f; // Duration of the speed boost
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    // Settings
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private GameObject boosterParticlePrefab; // Prefab for booster particles
    [SerializeField] private GameObject boosterParticlePrefab2; // Prefab for booster particles
    private float originalMotorForce; // To store original motor force
    private bool isBoosting = false;
    private GameObject currentSparkEffect;
    [SerializeField] private GameObject smoke; // Prefab for spark particles

    [SerializeField] private GameObject smokep; // Prefab for spark particles
    [SerializeField] private GameObject firep; // Prefab for spark particles
    // Wheel Colliders
    [SerializeField] private AudioClip engineSound; // Engine sound clip
    private AudioSource engineAudioSource;
    private AudioSource ActionSource;
    [SerializeField] private GameObject sparkParticlePrefab; // Prefab for spark particles
    private void Start()
    {
        // Add or fetch the NavMeshAgent component
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        // Set initial target destination for the NavMeshAgent
        if (destination != null)
        {
            navMeshAgent.SetDestination(destination.position);
        }
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        ActionSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.clip = engineSound;
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = 1f; // 3D sound
        engineAudioSource.minDistance = 5f;
        engineAudioSource.maxDistance = 100f;
        engineAudioSource.rolloffMode = AudioRolloffMode.Linear;

        engineAudioSource.volume = 1f; // Ensure it's audible
        engineAudioSource.pitch = 1f;

        // Start playing
        engineAudioSource.Play();
    }

    private void FixedUpdate()
    {
        if (navMeshAgent != null && destination != null)
        {
            // Calculate the distance between the car and the destination
            float distanceToDestination = Vector3.Distance(transform.position, destination.position);

            // You can set a threshold (e.g., 1 unit) to decide when the car has reached the destination
            float threshold = 1f;

            if (distanceToDestination > threshold)
            {
                NavigateUsingNavMesh();
                UpdateWheels();
            }
            else
            {
                StopCar(); // Stop car when at the destination
                engineAudioSource.pitch = 1f; // Reset the pitch when stopped
            }
        }
    }

    private void NavigateUsingNavMesh()
    {
        // Get the direction vector from NavMeshAgent
        Vector3 localVelocity = transform.InverseTransformDirection(navMeshAgent.desiredVelocity);
        float horizontalInput = localVelocity.x / localVelocity.magnitude;
        float verticalInput = Mathf.Clamp(localVelocity.z, 0, 1);

        // Apply steering based on NavMeshAgent direction
        float currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

        // Apply motor force to the wheels
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
    }

    private void StopCar()
    {
        // Stop motor and braking force
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;

        frontLeftWheelCollider.brakeTorque = motorForce;
        frontRightWheelCollider.brakeTorque = motorForce;
        rearLeftWheelCollider.brakeTorque = motorForce;
        rearRightWheelCollider.brakeTorque = motorForce;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
     private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Booster") && !isBoosting)
        {
            Destroy(other.gameObject);
            StartCoroutine(ActivateBooster());
            StartCoroutine(ActivateBooster2());

        }
        if (other.CompareTag("finishline") && !hasFinished)
        {
            hasFinished = true; // Player has finished the race
            RaceEndSequence(); // Execute the race-end logic
            engineAudioSource.Stop(); // Start the engine sound
        }
        else if (other.CompareTag("minebomb"))
        {
            // Instantiate smoke on the mine
            GameObject a = Instantiate(smoke, other.transform.position, Quaternion.identity);

            // Instantiate smoke and fire on the car
            GameObject carSmoke = Instantiate(smokep, transform.position, Quaternion.identity);
            carSmoke.transform.SetParent(transform); // Attach smoke to the car
            GameObject c = Instantiate(firep, transform.position, Quaternion.identity);
            c.transform.SetParent(transform); // Attach fire to the car

            // Apply a shock force to the car
            Rigidbody rb = GetComponent<Rigidbody>();
            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            if (agent != null)
            {
                // Disable the NavMeshAgent temporarily to apply the force
                agent.enabled = false;
            }
            CheckAndResetCar();
            Vector3 explosionDirection = (transform.position - other.transform.position).normalized;
            float explosionForce = 10000f; // Adjust as needed
            float explosionUpwardModifier = 1f;

            rb.AddExplosionForce(explosionForce, other.transform.position, 5f, explosionUpwardModifier, ForceMode.Impulse);

            // Re-enable the NavMeshAgent after a delay
            StartCoroutine(ReenableNavMeshAgent(agent, 2f));

            // Destroy smoke and fire effects after a delay
            Destroy(carSmoke, 5f);
            Destroy(a, 5f);
            Destroy(c, 10f);

            // Destroy the mine bomb
            Destroy(other.gameObject);
        }

    }
    private IEnumerator ReenableNavMeshAgent(NavMeshAgent agent, float delay)
    {
        if (agent != null)
        {
            yield return new WaitForSeconds(delay);

            // Check for a valid NavMesh position
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                // Snap the car to the nearest valid position on the NavMesh
                transform.position = hit.position;
                transform.rotation = Quaternion.identity; // Reset rotation to upright

                // Reset Rigidbody velocity
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // Re-enable the NavMeshAgent
                agent.enabled = true;

                // Reset the destination
                if (destination != null)
                {
                    agent.SetDestination(destination.position);
                    Debug.Log("Destination reset after re-enabling NavMeshAgent.");
                }
            }
            else
            {
                Debug.LogWarning("No valid NavMesh position found near the car.");
            }
        }
    }



    private IEnumerator ActivateBooster()
    {
        isBoosting = true;
        motorForce = 10000; // Increase motor force

        if (boosterParticlePrefab != null)
        {
            // Calculate the position behind the car
            Vector3 boosterPosition = transform.position;
            boosterPosition.y += 0.7f;
            boosterPosition.x -= 0.6f;
            GameObject particles = Instantiate(boosterParticlePrefab, boosterPosition, Quaternion.identity);

            // Align the particles with the car's rotation
            particles.transform.rotation = transform.rotation;

            // Attach to the car so it moves with it
            particles.transform.parent = transform;

            // Destroy particles after boost duration
            Destroy(particles, boosterDuration);
        }

        yield return new WaitForSeconds(boosterDuration); // Wait for boost duration

        motorForce = originalMotorForce; // Reset motor force
        isBoosting = false;
    }
    private IEnumerator ActivateBooster2()
    {
        isBoosting = true;
        motorForce *= boosterForceMultiplier; // Increase motor force

        if (boosterParticlePrefab != null)
        {
            // Calculate the position behind the car
            Vector3 boosterPosition = transform.position;
            boosterPosition.y += 0.7f;
            boosterPosition.x += 0.6f;
            GameObject particles = Instantiate(boosterParticlePrefab, boosterPosition, Quaternion.identity);

            // Align the particles with the car's rotation
            particles.transform.rotation = transform.rotation;

            // Attach to the car so it moves with it
            particles.transform.parent = transform;

            // Destroy particles after boost duration
            Destroy(particles, boosterDuration);
        }

        yield return new WaitForSeconds(boosterDuration); // Wait for boost duration

        motorForce = originalMotorForce; // Reset motor force
        isBoosting = false;
    }
    private bool isColliding = false; // Flag to track collision state
    private ParticleSystem sparkParticles; // Reference to the particle system

    private void OnCollisionEnter(Collision collision)
    {
        if (sparkParticlePrefab != null && !isColliding)
        {
            // Set the collision state
            isColliding = true;

            // Spawn sparks at the point of collision
            Vector3 contactPoint = collision.contacts[0].point;
            currentSparkEffect = Instantiate(sparkParticlePrefab, contactPoint, Quaternion.identity);
            currentSparkEffect.transform.parent = transform; // Attach to the car

            // Get the particle system component
            sparkParticles = currentSparkEffect.GetComponent<ParticleSystem>();
            if (sparkParticles != null)
            {
                sparkParticles.Play(); // Start the particle system
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (currentSparkEffect != null)
        {
            // Update the spark position to the current collision contact point
            Vector3 contactPoint = collision.contacts[0].point;
            currentSparkEffect.transform.position = contactPoint;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (currentSparkEffect != null)
        {
            if (sparkParticles != null)
            {
                sparkParticles.Stop(); // Stop the particle system
            }
            Destroy(currentSparkEffect, 1f); // Allow time for particles to fade out
            currentSparkEffect = null;

            // Reset the collision state
            isColliding = false;
        }
    }
  
    [SerializeField] private float resetCooldown = 5f; // Cooldown before reset
    private float lastResetTime;
    private void CheckAndResetCar()
    {
        float angle = Vector3.Angle(transform.up, Vector3.up);

       
            lastResetTime += Time.deltaTime;
            if (lastResetTime >= resetCooldown)
            {
                ResetCarPosition();
                lastResetTime = 0f;
            }
       
    }

    private void ResetCarPosition()
    {
        Vector3 position = transform.position;
        Quaternion rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.SetPositionAndRotation(position, rotation);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }








    [SerializeField] private GameObject leaderboardCanvas; // Leaderboard UI Canvas
    private bool hasFinished = false; // Race completion flag
    [SerializeField] private AudioClip finishSound; // Race finish sound
    private Rigidbody rb;
    private void RaceEndSequence()
    {
        hasFinished = true;

        if (finishSound != null)
        {
            ActionSource.PlayOneShot(finishSound);
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        navMeshAgent.enabled = false;

        if (leaderboardCanvas != null)
            leaderboardCanvas.SetActive(true);

        StartCoroutine(WaitAndPauseGame());
    }

    private IEnumerator WaitAndPauseGame()
    {
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0;
        AudioListener.volume = 0;
    }
}

