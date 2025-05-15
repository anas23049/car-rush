using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpaceShooter : MonoBehaviour
{
    public Transform destination; // The target destination for the spaceship
    public Transform[] players; // Array of player transforms
    public GameObject projectilePrefab; // The projectile to shoot
    public Transform shootPoint; // Where the projectile will spawn
    public float shootDistance = 100f; // Distance within which the spaceship will shoot the player
    public float fireRate = 2f; // Time between each shot
    public float projectileSpeed = 20f; // Speed of the projectile

    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    private Vector3 startingPosition; // The starting position of the spaceship
    private bool movingToDestination = true; // Tracks the current target (true = destination, false = start)
    private float nextFireTime = 0f; // Keeps track of the next time the spaceship can fire
    [SerializeField] private AudioClip engineSound; // Engine sound clip
    private AudioSource engineAudioSource;
    void Start()
    {
        engineAudioSource = gameObject.AddComponent<AudioSource>();
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
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Save the starting position
        startingPosition = transform.position;

        // Set the initial destination
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(destination.position);
        }
        else
        {
            Debug.LogError("NavMeshAgent is not on the NavMesh!");
        }
    }

    void Update()
    {
        // Check if the AI has reached the current target
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Toggle between destination and starting position
            if (movingToDestination)
            {
                agent.SetDestination(startingPosition);
            }
            else
            {
                agent.SetDestination(destination.position);
            }

            movingToDestination = !movingToDestination; // Switch the target
        }

        // Find the closest player and check distance to the closest one
        Transform closestPlayer = GetClosestPlayer();
        if (closestPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.position);
            if (distanceToPlayer <= shootDistance && Time.time >= nextFireTime)
            {
                ShootAtPlayer(closestPlayer);
                nextFireTime = Time.time + fireRate; // Update the next fire time
            }
        }
    }

    // Method to find the closest player
    Transform GetClosestPlayer()
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Transform player in players)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < closestDistance)
            {
                closest = player;
                closestDistance = distance;
            }
        }

        return closest;
    }

    void ShootAtPlayer(Transform player)
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            // Instantiate the projectile at the shoot point
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

            // Set the projectile to continuously follow the player
            ProjectileMover mover = projectile.AddComponent<ProjectileMover>();
            mover.Initialize(player, projectileSpeed);

            Debug.Log("Shot fired at player!");
        }
        else
        {
            Debug.LogError("Projectile prefab or shoot point is missing!");
        }
    }
}

public class ProjectileMover : MonoBehaviour
{
    private Transform target; // The player or car to follow
    private float speed; // Speed of the projectile

    // Initialize the projectile with the target and speed
    public void Initialize(Transform target, float speed)
    {
        this.target = target;
        this.speed = speed;
    }

    void Update()
    {
        if (target != null)
        {
            // Move toward the target
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Face the target
            transform.LookAt(target);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has the "Car" tag
        if (collision.transform.CompareTag("car"))
        {
            Destroy(gameObject); // Destroy the projectile on collision with an object tagged "Car"
        }
    }
}


