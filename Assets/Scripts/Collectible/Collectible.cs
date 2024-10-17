using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [Tooltip("This is the value of the item when collected.")]
    public int CoinValue = 1;

    [Tooltip("Set to true if you want to destroy this object when it is collected.")]
    public bool DestroyOnCollect = true;

    [Tooltip("If true, the item will be reactivated after RespawnTime.")]
    public bool Respawn = false;

    [Tooltip("If Respawn is true, this GameObject will reactivate after RespawnTime. In seconds.")]
    public float RespawnTime = 10f;

    [Header("Rotation Settings")]
    [Tooltip("Enable or disable spinning of the visuals.")]
    public bool Spin = true;

    [Tooltip("The speed at which the visuals spin.")]
    public float SpinSpeed = 100f;

    [Tooltip("The direction of the spin using an angle direction vector.")]
    public Vector3 SpinDirection = new Vector3(0, 1, 0); // Default to Y-axis spin

    [Tooltip("The child GameObject that will rotate.")]
    public GameObject VisualChild; // The visuals child object to spin

    [Tooltip("The Visuals of this Collectible")]
    [SerializeField] private List<GameObject> gameVisuals;

    [Header("Events")]
    [Tooltip("Optional event to be called when the item is collected.")]
    public UnityEvent onCollected;

    [Header("Sound Settings")]
    [Tooltip("List of audio clips to randomly choose from when collected.")]
    public List<AudioClip> grabSounds;

    [Tooltip("Audio source for playing the grab sound.")]
    public AudioSource audioSource;

    private bool isCollected = false; // Track if the item has been collected

    private void Start()
    {
        // Set the collider to trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Check if the VisualChild is assigned
        if (VisualChild == null)
        {
            Debug.LogWarning("VisualChild not assigned! Please assign the child object to spin.");
        }
    }

    private void Update()
    {
        // Rotate the child visuals if Spin is enabled and VisualChild is assigned
        if (Spin && VisualChild != null)
        {
            // Use the normalized direction vector for consistent spin
            VisualChild.transform.Rotate(SpinDirection.normalized, SpinSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only process if not already collected
        if (isCollected) return;

        Collector collector = other.GetComponent<Collector>();
        if (collector != null)
        {
            foreach (GameObject obj in gameVisuals)
            {
                obj.SetActive(false);
            }
            Collect(collector); // Call the Collect method if it's a valid collector
        }
    }

    public void Collect(Collector collector)
    {
        if (isCollected) return; // Prevent double collection

        isCollected = true; // Mark as collected

        // Play a random grab sound
        PlayRandomGrabSound();

        // Invoke the collected event
        onCollected?.Invoke();
        Debug.Log("Collected Item!");

        // Award points to the collector
        collector.CollectItem(CoinValue);

        // Destroy or respawn logic
        if (DestroyOnCollect)
        {
            // Delay the destruction until the sound finishes playing, if a clip is selected
            if (audioSource != null && audioSource.clip != null)
            {
                Destroy(gameObject, audioSource.clip.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if (Respawn)
        {
            StartCoroutine(RespawnCoin(RespawnTime));
        }
    }

    private void PlayRandomGrabSound()
    {
        if (grabSounds.Count > 0 && audioSource != null)
        {
            // Choose a random clip from the list
            AudioClip randomClip = grabSounds[Random.Range(0, grabSounds.Count)];

            // Assign it to the audio source and play it
            audioSource.clip = randomClip;
            audioSource.Play();
        }
    }

    private IEnumerator RespawnCoin(float seconds)
    {
        // Hide the coin for a while
        gameObject.SetActive(false);
        yield return new WaitForSeconds(seconds);

        // Reset state and reactivate
        isCollected = false;
        gameObject.SetActive(true);
    }
}
