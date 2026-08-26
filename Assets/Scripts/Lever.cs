using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject wall;        // Wall / platform object
    public float moveSpeed = 2f;   // Keela vara speed
    public float moveDistance = 5f; // Evlo distance keela varanum
    public AudioClip leverPullSound; // Lever pull sound
    public AudioClip secretFoundSound; // Secret found audio
    public string coinTag = "Coin"; // Coin tag name

    Animator anim;
    AudioSource audioSource;

    bool activated = false;
    bool moveWall = false;
    bool secretLevelAudioPlayed = false; // Track if secret audio already played

    float startY;
    float targetY;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (wall != null)
        {
            startY = wall.transform.position.y;
            targetY = startY - moveDistance;   // limit set pannrom
        }
    }

    void Update()
    {
        if (moveWall && wall != null)
        {
            Vector3 pos = wall.transform.position;

            if (pos.y > targetY)
            {
                pos.y -= moveSpeed * Time.deltaTime;
                wall.transform.position = pos;
            }
            else
            {
                pos.y = targetY;   // exact limit
                wall.transform.position = pos;
                moveWall = false;  // stop movement
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Lever activation with lever sound
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;

            anim.SetTrigger("pull");  // Lever animation
            moveWall = true;         // Start wall movement

            // Play lever pull sound
            if (audioSource != null && leverPullSound != null)
            {
                audioSource.PlayOneShot(leverPullSound);
            }
        }

        // Coin touch in secret place - plays secret found sound
        if (other.CompareTag(coinTag) && activated && !secretLevelAudioPlayed)
        {
            secretLevelAudioPlayed = true;

            // Play secret found sound
            if (audioSource != null && secretFoundSound != null)
            {
                audioSource.PlayOneShot(secretFoundSound);
            }
        }
    }
}