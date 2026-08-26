using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("Forest Settings")]
    public float forestDarkness = 0.6f;

    [Header("Damage Settings")]
    public float damageInvulnerabilityTime = 1f;  // Time player is invulnerable after taking damage

    [Header("Audio Settings")]
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip runLoopSound;
    public float jumpVolume = 1f;
    public float doubleJumpVolume = 1f;
    public float runVolume = 0.6f;
    public float runSoundMinSpeed = 0.5f; // Minimum speed to trigger run loop

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    AudioSource audioSource;
    AudioSource runAudioSource;

    float moveInput;
    int jumpCount = 0;
    bool isGrounded;
    bool jumpTriggered = false; // prevents jump animation restart
    bool isInvulnerable = false; // invulnerability after taking damage
    float invulnerabilityTimer = 0f;
    bool isRunning = false; // Track if run loop is currently playing

    Vector3 originalScale;
    Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        originalScale = transform.localScale;
        originalColor = sr.color;

        // Setup main audio source for jump sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Setup separate audio source for run loop (so it can play simultaneously with jump)
        runAudioSource = gameObject.AddComponent<AudioSource>();
        runAudioSource.playOnAwake = false;
        runAudioSource.loop = true;
        if (runLoopSound != null)
        {
            runAudioSource.clip = runLoopSound;
        }
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.CanPlayerMove())
        {
            moveInput = 0;
            anim.SetFloat("Speed", 0);
            StopRunSound();
            return;
        }

        // Handle invulnerability timer
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
                sr.color = originalColor;
            }
            else
            {
                // Flash effect during invulnerability
                float flashAlpha = Mathf.Abs(Mathf.Sin(invulnerabilityTimer * 10f));
                Color flashColor = originalColor;
                flashColor.a = flashAlpha;
                sr.color = flashColor;
            }
        }

        // Horizontal movement input
        moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Flip character based on movement direction
        if (moveInput > 0.1f)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (moveInput < -0.1f)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // ----------------------
        // Run Sound Management
        // ----------------------
        if (isGrounded && Mathf.Abs(moveInput) > 0.1f && Mathf.Abs(rb.linearVelocity.x) > runSoundMinSpeed)
        {
            PlayRunSound();
        }
        else
        {
            StopRunSound();
        }

        // ----------------------
        // Jump logic
        // ----------------------
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
            isGrounded = false;

            // Play appropriate jump sound based on jump count
            if (jumpCount == 1)
            {
                PlayJumpSound();
            }
            else if (jumpCount == 2)
            {
                PlayDoubleJumpSound();
            }

            // Trigger jump animation only once per press
            if (!jumpTriggered)
            {
                anim.SetBool("IsJumping", true);
                jumpTriggered = true;
            }
        }

        // Running animation only on ground
        anim.SetFloat("Speed", isGrounded ? Mathf.Abs(rb.linearVelocity.x) : 0f);
    }

    void FixedUpdate()
    {
        // Better platformer feel - increased gravity when falling
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * Time.fixedDeltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    jumpCount = 0;

                    // Reset jump animation on landing
                    anim.SetBool("IsJumping", false);
                    jumpTriggered = false;
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Coin pickup - increases points
        if (other.CompareTag("Coin"))
        {
            if (GameManager.instance != null)
                GameManager.instance.AddPoint();
            Destroy(other.gameObject);
        }

        // Heart pickup - restores health (max 3 hearts)
        else if (other.CompareTag("Heart"))
        {
            if (GameManager.instance != null)
                GameManager.instance.AddHeart();
            Destroy(other.gameObject);
        }

        // Enemy collision - lose 1 heart
        else if (other.CompareTag("Enemy"))
        {
            // Only take damage if not invulnerable
            if (!isInvulnerable && GameManager.instance != null)
            {
                // Activate invulnerability
                isInvulnerable = true;
                invulnerabilityTimer = damageInvulnerabilityTime;

                // Lose a heart
                GameManager.instance.LoseHeart();

                // Only respawn if player still has hearts
                if (GameManager.instance.GetCurrentHearts() > 0)
                {
                    Respawn();
                }
            }
        }

        // Checkpoint - saves spawn position
        else if (other.CompareTag("checkpoint"))
        {
            if (GameManager.instance != null)
                GameManager.instance.SetCheckpoint(other.transform.position);
        }

        // Forest area - darkens player color
        else if (other.CompareTag("Forest"))
        {
            if (!isInvulnerable)
                MakePlayerDarker();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Forest"))
        {
            if (!isInvulnerable)
                RestorePlayerColor();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Keep player dark while in forest
        if (other.CompareTag("Forest") && !isInvulnerable)
        {
            MakePlayerDarker();
        }
    }

    // ========================
    // Audio Methods
    // ========================
    void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    void PlayDoubleJumpSound()
    {
        if (doubleJumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doubleJumpSound, doubleJumpVolume);
        }
        else if (jumpSound != null && audioSource != null)
        {
            // Fallback to regular jump sound if double jump sound not assigned
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    void PlayRunSound()
    {
        if (runLoopSound != null && runAudioSource != null && !isRunning)
        {
            runAudioSource.volume = runVolume;
            runAudioSource.Play();
            isRunning = true;
        }
    }

    void StopRunSound()
    {
        if (runAudioSource != null && isRunning)
        {
            runAudioSource.Stop();
            isRunning = false;
        }
    }

    // ========================
    // Color Management Methods
    // ========================
    void MakePlayerDarker()
    {
        sr.color = new Color(
            originalColor.r * forestDarkness,
            originalColor.g * forestDarkness,
            originalColor.b * forestDarkness,
            originalColor.a
        );
    }

    void RestorePlayerColor()
    {
        sr.color = originalColor;
    }

    // ========================
    // Respawn Method
    // ========================
    void Respawn()
    {
        if (GameManager.instance != null)
        {
            transform.position = GameManager.instance.GetCheckpoint();
            rb.linearVelocity = Vector2.zero;
            GameManager.instance.ResetAllClouds();
        }

        jumpCount = 0;
        jumpTriggered = false;
        anim.SetBool("IsJumping", false);

        // Reset forest color effect if respawning outside forest
        RestorePlayerColor();

        // Stop run sound on respawn
        StopRunSound();
    }

    // ========================
    // Getter Methods
    // ========================
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}