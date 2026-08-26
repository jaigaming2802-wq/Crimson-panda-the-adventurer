using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public float speed = 2f;          // Movement speed
    public float moveDistance = 10f;  // Evlo distance move aaganum

    float startX;
    float targetX;

    void Start()
    {
        startX = transform.position.x;
        targetX = startX + moveDistance;  // Right side limit
    }

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        if (transform.position.x >= targetX)
        {
            Destroy(gameObject);
        }
    }
}