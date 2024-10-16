using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System.Net;
using UnityEngine.Windows;
using BNG;

public class SliceObject : MonoBehaviour
{
    // Reference to the start and end points of the blade (assign in the inspector or dynamically)
    public Transform bladeStart;
    public Transform bladeEnd;
    public VelocityEstimator velocityEstimator;
    private InputBridge input;
    // Reference to a material used for the cross-section (cut faces)
    public Material crossSectionMaterial;

    public BNG.ControllerHand HandSide = ControllerHand.Right;

    // Adjust this value as needed for the force of the cut
    public float cutForce = 2000f;

    // Layer mask to filter sliceable objects
    public LayerMask sliceableLayer;

    // Thickness to extend the ray slightly to ensure collision is detected
    public float rayExtension = 0.01f;

    // Slice sound effect
    public AudioClip sliceSound;    // Assign your sound in the Inspector
    private AudioSource audioSource;   // To play the sound

    // Range for random pitch
    public float minPitch = 0.8f;  // Lower pitch for slower speed
    public float maxPitch = 1.2f;  // Higher pitch for faster speed

    void Start()
    {
        // Initialize the audio source component
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Awake()
    {
        input = InputBridge.Instance;
    }

        void FixedUpdate()
    {
        Debug.DrawLine(bladeStart.position, bladeEnd.position, Color.red);

        if (Physics.Linecast(bladeStart.position, bladeEnd.position, out RaycastHit hit, sliceableLayer))
        {
            GameObject sliceableObject = hit.collider.gameObject;
            StartCoroutine(SliceAsync(sliceableObject));
        }
    }

    IEnumerator SliceAsync(GameObject sliceableObject)
{
    yield return new WaitForEndOfFrame(); // Defer slicing to the end of the frame

    Vector3 velocity = velocityEstimator.GetVelocityEstimate();
    Vector3 planeNormal = Vector3.Cross(bladeEnd.position - bladeStart.position, velocity);
    planeNormal.Normalize();

    SlicedHull slicedObject = sliceableObject.Slice(bladeEnd.position, planeNormal, crossSectionMaterial);

    if (slicedObject != null)
    {
        input.VibrateController(0.5f, 1f, 0.05f, HandSide);

        PlaySliceSound();

        GameObject upperHull = slicedObject.CreateUpperHull(sliceableObject, crossSectionMaterial);
        GameObject lowerHull = slicedObject.CreateLowerHull(sliceableObject, crossSectionMaterial);
        upperHull.AddComponent<DespawnAfterSlice>();
        lowerHull.AddComponent<DespawnAfterSlice>();

        SetupHullObject(upperHull, sliceableObject);
        SetupHullObject(lowerHull, sliceableObject);

        Destroy(sliceableObject);
    }
}


    // Helper function to configure the newly created slice objects
    private void SetupHullObject(GameObject hull, GameObject originalObject)
    {
        // Set parent and position to match original object
        hull.transform.SetPositionAndRotation(originalObject.transform.position, originalObject.transform.rotation);
        hull.transform.localScale = originalObject.transform.localScale;

        // Add necessary components to the sliced object
        hull.AddComponent<MeshCollider>().convex = true;  // Add collider
        Rigidbody rb = hull.AddComponent<Rigidbody>();    // Add rigidbody for physics

        // Apply explosion force to the rigidbody
        rb.AddExplosionForce(cutForce, originalObject.transform.position, 1f);
    }

    // Function to play the slice sound with random pitch and speed
    private void PlaySliceSound()
    {
        if (sliceSound != null && audioSource != null)
        {
            // Set random pitch for variation
            audioSource.pitch = Random.Range(minPitch, maxPitch);

            // Play the sound with the random pitch
            audioSource.PlayOneShot(sliceSound);
        }
    }
}
