using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MagicBox : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;
    Collider2D boxCollider;
    AudioSource audioSource;

    Color originalColor;
    GameObject forestGameObject;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float winSoundVolume = 0.8f; // CHANGED: Add volume control

    private bool hasOpened = false;
    private float openTime = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        // If AudioSource doesn't exist, create one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource component added!");
        }

        // CHANGED: Configure the audio source properly
        audioSource.spatialBlend = 0f; // 2D sound (no 3D positioning)
        audioSource.volume = winSoundVolume; // Set appropriate volume
        audioSource.loop = false; // Don't loop the win sound

        originalColor = sr.color;

        forestGameObject = GameObject.FindWithTag("Forest");

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void Update()
    {
        if (forestGameObject != null && boxCollider != null)
        {
            Collider2D forestCollider = forestGameObject.GetComponent<Collider2D>();
            if (forestCollider != null && forestCollider.bounds.Contains(transform.position))
                MakeBoxDarker();
            else
                RestoreBoxColor();
        }

        // Check if animation is complete
        if (hasOpened && Time.time - openTime >= animationDuration)
        {
            hasOpened = false;
            ShowWinPanel();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasOpened)
        {
            hasOpened = true;
            openTime = Time.time;
            anim.SetTrigger("open");
            Debug.Log("Box opening animation started!");
        }
    }

    void ShowWinPanel()
    {
        Debug.Log("Animation finished! Showing win screen...");

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            // Play the win sound
            if (winSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(winSound, winSoundVolume);
                Debug.Log("Win sound playing at volume: " + winSoundVolume);
            }
            else if (winSound == null)
            {
                Debug.LogWarning("Win sound clip is not assigned!");
            }

            StartCoroutine(FadeInPanel());
        }
    }

    IEnumerator FadeInPanel()
    {
        CanvasGroup canvasGroup = winPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = winPanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.5f);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        Debug.Log("Win screen fully visible!");
    }

    void MakeBoxDarker()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            sr.color = new Color(
                originalColor.r * player.forestDarkness,
                originalColor.g * player.forestDarkness,
                originalColor.b * player.forestDarkness,
                originalColor.a
            );
        }
    }

    void RestoreBoxColor()
    {
        sr.color = originalColor;
    }
}