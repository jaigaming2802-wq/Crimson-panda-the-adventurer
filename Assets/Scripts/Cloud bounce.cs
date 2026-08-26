using UnityEngine;

public class FallingCloud : MonoBehaviour
{
    public float waitTime = 0.3f;
    public float fallSpeed = 5f;
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 40f;

    bool playerOnCloud = false;
    bool falling = false;
    float timer = 0f;

    Vector3 originalPos;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        if (playerOnCloud && !falling)
        {
            timer += Time.deltaTime;

            // Shake before falling
            if (timer >= waitTime - 0.15f)
            {
                float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
                transform.position = originalPos + new Vector3(offsetX, 0, 0);
            }

            if (timer >= waitTime)
            {
                falling = true;
            }
        }

        if (falling)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnCloud = true;
        }
    }

    // ✅ ADD THIS FUNCTION
    public void ResetCloud()
    {
        transform.position = originalPos;
        playerOnCloud = false;
        falling = false;
        timer = 0f;
    }
}