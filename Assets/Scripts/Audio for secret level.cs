using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    public AudioClip secretLevelAudio;  // Secret Level audio
    public AudioClip danger1Audio;      // Danger 1 audio
    public AudioClip danger2Audio;      // Danger 2 audio
    public GameObject audioOffObject;   // Drag the object here to turn off audio

    AudioSource audioSource;
    bool audioPlayed = false; // Play only once

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // If player touches the audioOffObject, stop the audio
            if (other.gameObject == audioOffObject)
            {
                audioSource.Stop();
                audioPlayed = false; // Reset so audio can play again
                return;
            }

            // Otherwise, play audio if not played yet
            if (!audioPlayed)
            {
                audioPlayed = true;

                // Check this gameobject's tag and play corresponding audio
                if (gameObject.CompareTag("Secret level") && secretLevelAudio != null)
                {
                    audioSource.PlayOneShot(secretLevelAudio);
                }
                else if (gameObject.CompareTag("Danger1") && danger1Audio != null)
                {
                    audioSource.PlayOneShot(danger1Audio);
                }
                else if (gameObject.CompareTag("Danger2") && danger2Audio != null)
                {
                    audioSource.PlayOneShot(danger2Audio);
                }
            }
        }
    }
}