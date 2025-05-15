using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
public class Carcontrol : MonoBehaviour
{

    [SerializeField] private AudioClip engineSound; // Engine running sound
    [SerializeField] private AudioClip brakeSound;  // Braking sound
    [SerializeField] private AudioClip boosterSound; // Boost activation sound
    [SerializeField] private AudioClip collisionSound; // Collision sound
    [SerializeField] private AudioClip finishSound; // Race finish sound
    [SerializeField] private AudioClip bomb; // Race finish sound

    [SerializeField] private RectTransform speedneedle; // 2D speedometer pin
    [SerializeField] private float maxSpeed = 200f; // Maximum speed of the car
    [SerializeField] private float maxRotation = -270f; // Maximum rotation angle of the pin on -z axis


    private AudioSource engineAudioSource; // For engine sound
    private AudioSource actionAudioSource; // For one-time sounds like braking, boosting, etc.


    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;

    // Settings
    [SerializeField] private float motorForce = 1500f, breakForce = 3000f, maxSteerAngle = 30f;
    [SerializeField] private float rollResetThreshold = 85f; // Threshold to detect if the car is flipped
    [SerializeField] private float resetCooldown = 2f; // Cooldown before reset
    private float lastResetTime;
    [SerializeField] private float boosterForceMultiplier = 6f; // Multiplier for speed boost
    [SerializeField] private float boosterDuration = 20f; // Duration of the speed boost
    [SerializeField] private GameObject boosterParticlePrefab; // Prefab for booster particles
    [SerializeField] private GameObject boosterParticlePrefab2; // Prefab for booster particles
    private float originalMotorForce; // To store original motor force
    private bool isBoosting = false;
    private GameObject currentSparkEffect;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;
    [SerializeField] private GameObject sparkParticlePrefab; // Prefab for spark particles
    [SerializeField] private GameObject smoke; // Prefab for spark particles

    [SerializeField] private GameObject smokep; // Prefab for spark particles
    [SerializeField] private GameObject firep; // Prefab for spark particles

    [SerializeField] private GameObject leaderboardCanvas; // Leaderboard UI Canvas
    private bool hasFinished = false; // Race completion flag

    
    private int playerPosition = 1; // Assuming single-player race

    private void Start()
    {
        wheelTransform = GetComponent<RectTransform>(); // Get UI element transform
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        actionAudioSource = gameObject.AddComponent<AudioSource>();

        // Set up engine audio source
        engineAudioSource.clip = engineSound;
        engineAudioSource.loop = true; // Loop the engine sound
        engineAudioSource.volume = 0.5f; // Adjust volume
        engineAudioSource.Play(); // Start the engine sound
        originalMotorForce = motorForce; // Store original motor force
        rb = GetComponent<Rigidbody>();
        if (leaderboardCanvas != null)
        {
            leaderboardCanvas.SetActive(false); // Hide leaderboard at the start
        }

      
    }
    private void UpdateSpeedometer()
    {
        // Get current speed of the car
        float currentSpeed = rb.velocity.magnitude * 3.6f; // Convert from m/s to km/h

        // Map the speed to the needle's rotation range
        float needleRotation = Mathf.Lerp(134.44f, -110f, currentSpeed / maxSpeed);

        // Apply the rotation to the needle
        speedneedle.localRotation = Quaternion.Euler(0f, 0f, needleRotation);
    }

    private void FixedUpdate()
    {
        if (hasFinished)
        {
            return; // Ignore controls if the race is over
        }

        GetInput();
        HandleTouchInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        CheckAndResetCar();
        UpdateSpeedometer(); // Update the speedometer pin
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }
    public RectTransform wheelTransform;
    private Vector2 touchStartPos;
    private float wheelAngle = 0f;
    private bool isTouching = false;

    public float rotationSpeed = 1.5f; // Adjust sensitivity
    public float maxrotation = 180f; // Limit left/right rotation
    public bool autoReturn = true; // Auto return to center


    private Coroutine returnCoroutine;


    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 localPoint;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Convert screen point to local UI space
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelTransform, touch.position, null, out localPoint))
                    {
                        isTouching = true;
                        touchStartPos = touch.position;

                        // Stop auto-return if a new touch starts
                        if (returnCoroutine != null)
                        {
                            StopCoroutine(returnCoroutine);
                        }
                    }
                    break;

                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        float deltaX = (touch.position.x - touchStartPos.x) * rotationSpeed;
                        wheelAngle = Mathf.Clamp(deltaX, -maxRotation, maxRotation);
                        wheelTransform.localRotation = Quaternion.Euler(0, 0, -wheelAngle);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    if (autoReturn)
                    {
                        returnCoroutine = StartCoroutine(ReturnToCenter());
                    }
                    break;
            }
        }
    }

    private IEnumerator ReturnToCenter()
    {
        while (Mathf.Abs(wheelAngle) > 0.5f)
        {
            wheelAngle = Mathf.Lerp(wheelAngle, 0, Time.deltaTime * 5);
            wheelTransform.localRotation = Quaternion.Euler(0, 0, -wheelAngle);
            yield return null;
        }
        wheelAngle = 0;
        wheelTransform.localRotation = Quaternion.Euler(0, 0, 0);
    }
    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;

        ApplyBreaking();
        float speed = rb.velocity.magnitude;
        engineAudioSource.pitch = Mathf.Lerp(1f, 2f, speed / 50f); // Scale p
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
        if (isBreaking && !actionAudioSource.isPlaying)
        {
            actionAudioSource.clip = brakeSound;
            actionAudioSource.Play();
        }
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void CheckAndResetCar()
    {
        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > rollResetThreshold && transform.up.y < 0.1f)
        {
            lastResetTime += Time.deltaTime;
            if (lastResetTime >= resetCooldown)
            {
                ResetCarPosition();
                lastResetTime = 0f;
            }
        }
        else
        {
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
    public CinemachineVirtualCamera mainCamera; // The regular camera following the player
    public CinemachineVirtualCamera frontCamera; // The camera positioned in front of the car



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
        }
        else if (other.CompareTag("minebomb") || other.CompareTag("bomb"))
        {

            GameObject a = Instantiate(smoke, other.transform.position, Quaternion.identity); // Smoke on the mine
            GameObject carSmoke = Instantiate(smokep, transform.position, Quaternion.identity);
            carSmoke.transform.SetParent(transform); // Attach the smoke to the car
            GameObject c = Instantiate(firep, transform.position, Quaternion.identity);
            c.transform.SetParent(transform); // Attach the smoke to the car
            if (bomb != null)
            {
                actionAudioSource.clip = bomb;
                actionAudioSource.Play();
            }
            // Apply a shock force to the car
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 explosionDirection = (transform.position - other.transform.position).normalized;
            float explosionForce = 20000f; // Adjust as needed
            float explosionUpwardModifier = 1f;

            rb.AddExplosionForce(explosionForce, other.transform.position, 5f, explosionUpwardModifier, ForceMode.Impulse);

            // Instantiate smoke at the car's position and attach it to the car



            Destroy(carSmoke, 5f);
            Destroy(a, 5f);
            Destroy(c, 10f);
            // Destroy the mine bomb
            Destroy(other.gameObject);
        }



    }

    private IEnumerator ActivateBooster()
    {
        isBoosting = true;
        motorForce = 10000; // Increase motor force
        if (boosterSound != null)
        {
            actionAudioSource.clip = boosterSound;
            actionAudioSource.Play();
        }


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
            if (collisionSound != null)
            {
                actionAudioSource.clip = collisionSound;
                actionAudioSource.Play();
            }

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
    private Rigidbody rb;
    private void RaceEndSequence()
    {
        if (finishSound != null)
        {
            actionAudioSource.PlayOneShot(finishSound);
        }
        // Disable car movement
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable car controls
        enabled = false; // Disables this script
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
        frontLeftWheelCollider.brakeTorque = breakForce;
        frontRightWheelCollider.brakeTorque = breakForce;

        if (mainCamera != null && frontCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
            frontCamera.gameObject.SetActive(true);
        }
  

     

        if (leaderboardCanvas != null)
        {
            leaderboardCanvas.SetActive(true);

        }
        StartCoroutine(WaitAndPauseGame());
    }

    private IEnumerator WaitAndPauseGame()
    {
        yield return new WaitForSecondsRealtime(5f);
        Time.timeScale = 0;
        AudioListener.volume = 0;
    }

}
