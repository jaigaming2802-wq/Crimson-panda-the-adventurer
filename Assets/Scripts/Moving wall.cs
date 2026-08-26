using UnityEngine;

public class OneTimeMovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public float upperY = 4f;
    public float lowerY = 1f;

    bool shouldMove = false;
    bool goingUp = true;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!shouldMove) return;

        Vector2 newPosition = rb.position;

        if (goingUp)
        {
            newPosition.y += speed * Time.fixedDeltaTime;

            if (newPosition.y >= upperY)
            {
                newPosition.y = upperY;
                goingUp = false; // start going down
            }
        }
        else
        {
            newPosition.y -= speed * Time.fixedDeltaTime;

            if (newPosition.y <= lowerY)
            {
                newPosition.y = lowerY;

                rb.MovePosition(newPosition);

                Destroy(gameObject); // 🔥 DESTROY HERE
                return;
            }
        }

        rb.MovePosition(newPosition);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            shouldMove = true;
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}