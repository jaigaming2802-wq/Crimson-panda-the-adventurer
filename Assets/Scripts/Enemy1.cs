using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    // Patrol Settings
    public float speed = 2f;
    public float upperLimit = 4f;
    public float lowerLimit = 1f;

    // Aggro Settings
    public float aggroRadius = 5f;
    public float chaseSpeed = 5f;
    public float minDistanceToPlayer = 0.5f;

    // State Variables
    private Transform player;
    private Vector2 startPosition;
    private bool isChasing = false;
    private bool isReturning = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        startPosition = transform.position;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < aggroRadius)
        {
            isChasing = true;
            isReturning = false;
            ChasePlayer();
        }
        else if (isChasing)
        {
            // Player escaped, return to start
            isChasing = false;
            isReturning = true;
        }

        if (isReturning)
        {
            ReturnToStart();
        }
        else if (!isChasing)
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // Smooth hover using PingPong relative to startPosition
        float newY = Mathf.PingPong(Time.time * speed, upperLimit - lowerLimit) + lowerLimit;
        transform.position = new Vector2(startPosition.x, newY);

        // Gentle flip left/right for realism
        float flipCycle = Mathf.Sin(Time.time * speed);
        if (flipCycle > 0)
            transform.localScale = new Vector3(1, 1, 1);   // facing right
        else
            transform.localScale = new Vector3(-1, 1, 1);  // facing left
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > minDistanceToPlayer)
        {
            transform.position += (Vector3)(direction * chaseSpeed * Time.deltaTime);
        }

        // Flip to face player
        transform.localScale = direction.x > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    void ReturnToStart()
    {
        Vector2 direction = (startPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);

        // Flip based on actual return direction
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);   // facing right
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);  // facing left

        if (Vector2.Distance(transform.position, startPosition) < 0.05f)
        {
            isReturning = false; // Done returning, resume patrol
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Call GameManager to handle player damage and respawn
            if (GameManager.instance != null)
            {
                GameManager.instance.LoseHeart();

                // Respawn player at checkpoint
                Vector3 checkpointPos = GameManager.instance.GetCheckpoint();
                other.transform.position = checkpointPos;

                // Reset all clouds in the level
                GameManager.instance.ResetAllClouds();

                Debug.Log("Player hit by Bee! Respawned at checkpoint: " + checkpointPos);
            }
            else
            {
                // Fallback if GameManager not found
                Debug.LogError("GameManager instance not found!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }

    // Reset method for checkpoint system
    public void ResetEnemy()
    {
        transform.position = startPosition;
        isChasing = false;
        isReturning = false;
    }
}