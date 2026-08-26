using UnityEngine;

public class ShakeAndBreakCloud : MonoBehaviour
{
    public float shakeDuration = 2f;  // how long it shakes
    public float shakeAmount = 0.05f;  // shake intensity
    public float destroyDelay = 2f;    // time after which cloud "disappears"

    Vector3 originalPos;
    bool playerOnCloud = false;
    float timer = 0f;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        if (playerOnCloud)
        {
            timer += Time.deltaTime;

            // SHAKE EFFECT
            if (timer <= shakeDuration)
            {
                float offsetX = Random.Range(-shakeAmount, shakeAmount);
                transform.position = originalPos + new Vector3(offsetX, 0, 0);
            }

            // "DESTROY" CLOUD (we'll just deactivate it)
            if (timer >= destroyDelay)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void LateUpdate()
    {
        if (!playerOnCloud)
        {
            transform.position = originalPos;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnCloud = true;
        }
    }

    // RESET METHOD to respawn the cloud
    public void ResetCloud()
    {
        transform.position = originalPos;
        timer = 0f;
        playerOnCloud = false;
        gameObject.SetActive(true);
    }
}