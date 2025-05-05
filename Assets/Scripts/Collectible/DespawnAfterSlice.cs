using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnAfterSlice : MonoBehaviour
{
    public float shrinkDuration = 2f;    // How long it takes to fully shrink
    public float despawnThreshold = 0.01f;  // Threshold at which the object is considered despawned
    public float delayBeforeShrink = 1f; // Optional delay before starting to shrink

    void Start()
    {
        // Start the despawn process after the specified delay
        Invoke(nameof(StartShrinking), delayBeforeShrink);

        // Set the layer to "Slicable" after 0.5 seconds
        Invoke(nameof(SetSlicableLayer), 0.3f);
    }

    void SetSlicableLayer()
    {
        // Set the gameObject to the "Slicable" layer (assuming the layer is already defined in the project)
        gameObject.layer = LayerMask.NameToLayer("Slicable");
    }

    void StartShrinking()
    {
        StartCoroutine(ShrinkAndDespawn());
    }

    IEnumerator ShrinkAndDespawn()
    {
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            // Calculate the scale reduction over time
            float t = elapsedTime / shrinkDuration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            // Check if the object has shrunk below the threshold
            if (transform.localScale.magnitude <= despawnThreshold)
            {
                Destroy(gameObject); // Destroy the object once it's small enough
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the object is destroyed after shrinking completely
        Destroy(gameObject);
    }
}
