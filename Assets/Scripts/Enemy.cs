using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float patrolDistance = 3f;

    public float aggroRadius = 5f;
    public float chaseSpeed = 3f;
    public float minDistanceToPlayer = 0.8f;

    public float forestDarkness = 0.6f;

    private Transform player;
    private Vector2 startPos;
    private bool movingRight = true;
    private bool chasing = false;

    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        startPos = transform.position;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < aggroRadius)
        {
            chasing = true;
            ChasePlayer();
        }
        else
        {
            chasing = false;
            Patrol();
        }
    }

    void Patrol()
    {
        if (movingRight)
        {
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);

            if (transform.position.x >= startPos.x + patrolDistance)
                movingRight = false;
        }
        else
        {
            transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime);

            if (transform.position.x <= startPos.x - patrolDistance)
                movingRight = true;
        }

        Flip();
    }

    void ChasePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > minDistanceToPlayer)
        {
            float direction = player.position.x > transform.position.x ? 1 : -1;
            transform.Translate(Vector2.right * direction * chaseSpeed * Time.deltaTime);
        }

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void Flip()
    {
        if (movingRight)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // 🌲 Forest Dark Effect
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Forest"))
        {
            sr.color = new Color(
                originalColor.r * forestDarkness,
                originalColor.g * forestDarkness,
                originalColor.b * forestDarkness,
                originalColor.a
            );
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Forest"))
        {
            sr.color = originalColor;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }
}