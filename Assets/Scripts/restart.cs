using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static bool shouldAutoStart = false;  // Flag to auto-start after restart

    [Header("Clouds")]
    public ShakeAndBreakCloud[] breakClouds;
    public FallingCloud[] fallingClouds;

    [Header("UI")]
    public Text pointsText;
    public Image[] heartImages;     // Drag 3 heart images here
    public GameObject startPanel;   // Drag your start menu panel here
    public GameObject gameOverPanel; // Optional: drag game over panel here

    [Header("Health Settings")]
    public int maxHearts = 3;
    public int currentHearts = 3;

    [Header("Audio")]
    public AudioClip coinSound;     // Optional: coin pickup sound
    public AudioClip heartSound;    // Optional: heart pickup sound
    public AudioClip damageSound;   // Optional: damage sound
    public AudioClip gameOverSound; // Optional: game over sound
    public AudioClip homeScreenMusic; // Home screen music
    public AudioClip gameplayMusic;   // Gameplay music

    Vector3 checkpoint;

    int points = 0;
    bool canMove = false;
    bool isGameOver = false;
    AudioSource audioSource;
    AudioSource coinAudioSource;    // Dedicated audio source for coin sounds (faster response)
    AudioSource musicAudioSource;   // For background music

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Get main audio source for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Create dedicated audio source for coins (faster response, no delay)
        coinAudioSource = gameObject.AddComponent<AudioSource>();
        coinAudioSource.clip = coinSound;
        coinAudioSource.playOnAwake = false;

        // Create dedicated audio source for background music
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        musicAudioSource.playOnAwake = false;

        // Pre-load all audio clips to avoid delay
        if (coinSound != null)
        {
            Resources.LoadAsync<AudioClip>(coinSound.name);
        }

        currentHearts = maxHearts;
        UpdateUI();

        // Check if we should auto-start (from Retry button)
        if (shouldAutoStart)
        {
            shouldAutoStart = false;  // Reset flag
            Time.timeScale = 1f;
            canMove = true;
            if (startPanel != null)
                startPanel.SetActive(false);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            PlayGameplayMusic();
            Debug.Log("Game restarted via Retry - Auto starting!");
        }
        else
        {
            // Normal start: pause game and show start panel
            Time.timeScale = 0f;
            if (startPanel != null)
                startPanel.SetActive(true);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            PlayHomeScreenMusic();
            Debug.Log("Game started normally - Showing start panel");
        }
    }

    // ========================
    // Game State Methods
    // ========================

    /// <summary>
    /// Called when Play button is pressed
    /// </summary>
    public void PlayGame()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        // Immediately stop home screen music with zero delay
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = null;
        }

        // Immediately start gameplay music
        PlayGameplayMusic();

        Time.timeScale = 1f;
        canMove = true;
        Debug.Log("Play button pressed - Game started!");
    }

    /// <summary>
    /// Checks if player can move
    /// </summary>
    public bool CanPlayerMove()
    {
        return canMove && !isGameOver;
    }

    /// <summary>
    /// Restarts the current game level (Retry button)
    /// </summary>
    public void RestartGameLevel()
    {
        Debug.Log("Restart button pressed");

        Time.timeScale = 1f;
        isGameOver = false;
        currentHearts = maxHearts;
        points = 0;
        UpdateUI();

        // Stop all audio
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = null;
        }

        // Stop coin audio source too
        if (coinAudioSource != null)
        {
            coinAudioSource.Stop();
        }

        // Set flag to auto-start after scene reload
        shouldAutoStart = true;

        // Reload current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
        Debug.Log("Reloading scene: " + currentSceneName);
    }

    /// <summary>
    /// Returns to menu (Menu button)
    /// Reloads SampleScene which shows HomeScreen panel
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("Return to menu button pressed");

        Time.timeScale = 1f;
        isGameOver = false;
        currentHearts = maxHearts;
        points = 0;

        // Stop all audio
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = null;
        }

        if (coinAudioSource != null)
        {
            coinAudioSource.Stop();
        }

        // Reset the auto-start flag
        shouldAutoStart = false;

        // Reload SampleScene to show HomeScreen panel
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Returning to HomeScreen");
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // ========================
    // Checkpoint System
    // ========================

    /// <summary>
    /// Sets the checkpoint position
    /// </summary>
    public void SetCheckpoint(Vector3 pos)
    {
        checkpoint = pos;
        Debug.Log("Checkpoint set at: " + pos);
    }

    /// <summary>
    /// Gets the checkpoint position
    /// </summary>
    public Vector3 GetCheckpoint()
    {
        return checkpoint;
    }

    // ========================
    // Cloud Reset System
    // ========================

    /// <summary>
    /// Resets all clouds in the level
    /// </summary>
    public void ResetAllClouds()
    {
        if (breakClouds != null)
        {
            foreach (var cloud in breakClouds)
            {
                if (cloud != null)
                    cloud.ResetCloud();
            }
        }

        if (fallingClouds != null)
        {
            foreach (var cloud in fallingClouds)
            {
                if (cloud != null)
                    cloud.ResetCloud();
            }
        }
    }

    // ========================
    // Points System
    // ========================

    /// <summary>
    /// Adds points when coin is collected
    /// </summary>
    public void AddPoint()
    {
        points++;
        UpdateUI();
        PlayCoinSound();  // Use dedicated coin sound method
        Debug.Log("Points: " + points);
    }

    public int GetPoints()
    {
        return points;
    }

    // ========================
    // Health/Heart System
    // ========================

    /// <summary>
    /// Removes one heart when player takes damage
    /// </summary>
    public void LoseHeart()
    {
        if (currentHearts > 0)
        {
            currentHearts--;
            UpdateHeartUI();
            PlaySound(damageSound);
            Debug.Log("Heart lost! Hearts remaining: " + currentHearts);

            if (currentHearts <= 0)
            {
                GameOver();
            }
        }
    }

    /// <summary>
    /// Adds one heart when heart item is collected (max 3)
    /// </summary>
    public void AddHeart()
    {
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            UpdateHeartUI();
            PlaySound(heartSound);
            Debug.Log("Heart added! Current hearts: " + currentHearts);
        }
    }

    /// <summary>
    /// Gets current heart count
    /// </summary>
    public int GetCurrentHearts()
    {
        return currentHearts;
    }

    /// <summary>
    /// Sets heart count directly (for testing or special scenarios)
    /// </summary>
    public void SetHearts(int hearts)
    {
        currentHearts = Mathf.Clamp(hearts, 0, maxHearts);
        UpdateHeartUI();
    }

    // ========================
    // UI Update Methods
    // ========================

    /// <summary>
    /// Updates heart UI display
    /// Shows/hides heart images based on current heart count
    /// </summary>
    void UpdateHeartUI()
    {
        if (heartImages == null || heartImages.Length == 0)
            return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].enabled = (i < currentHearts);
            }
        }
    }

    /// <summary>
    /// Updates all UI elements (points and hearts)
    /// </summary>
    void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = points.ToString();

        UpdateHeartUI();
    }

    // ========================
    // Game Over
    // ========================

    /// <summary>
    /// Called when player runs out of hearts
    /// </summary>
    void GameOver()
    {
        isGameOver = true;

        // Stop gameplay music
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }

        // Play game over sound
        PlaySound(gameOverSound);

        Time.timeScale = 0f;
        Debug.Log("GAME OVER!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    // ========================
    // Audio Methods
    // ========================

    /// <summary>
    /// Plays home screen music
    /// </summary>
    void PlayHomeScreenMusic()
    {
        if (homeScreenMusic != null && musicAudioSource != null)
        {
            musicAudioSource.clip = homeScreenMusic;
            musicAudioSource.Play();
            Debug.Log("Playing home screen music");
        }
    }

    /// <summary>
    /// Plays gameplay music
    /// </summary>
    void PlayGameplayMusic()
    {
        if (gameplayMusic != null && musicAudioSource != null)
        {
            musicAudioSource.clip = gameplayMusic;
            musicAudioSource.Play();
            Debug.Log("Playing gameplay music");
        }
    }

    /// <summary>
    /// Stops home screen music
    /// </summary>
    void StopHomeScreenMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }
    }

    /// <summary>
    /// Plays coin sound with NO DELAY (uses dedicated audio source)
    /// </summary>
    void PlayCoinSound()
    {
        if (coinSound != null && coinAudioSource != null)
        {
            coinAudioSource.volume = 1f;
            coinAudioSource.PlayOneShot(coinSound, 1f);
        }
    }

    /// <summary>
    /// Plays other sound effects (heart, damage, game over)
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.volume = 1f;
            audioSource.PlayOneShot(clip, 1f);
        }
    }

    // ========================
    // Debug/Testing Methods
    // ========================

    /// <summary>
    /// For testing: instantly lose a heart
    /// </summary>
    public void DebugLoseHeart()
    {
        LoseHeart();
    }

    /// <summary>
    /// For testing: instantly add a heart
    /// </summary>
    public void DebugAddHeart()
    {
        AddHeart();
    }

    /// <summary>
    /// For testing: add 10 points
    /// </summary>
    public void DebugAddPoints()
    {
        points += 10;
        UpdateUI();
    }
}