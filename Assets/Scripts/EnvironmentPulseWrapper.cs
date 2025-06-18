using System.Collections;
using UnityEngine;

/// <summary>
/// Automatically wraps a static environment object in a pulsing parent for beat-based scale animation.
/// </summary>
public class EnvironmentPulseWrapper : MonoBehaviour
{
    [SerializeField] private float pulseSize = 1.1f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private bool useMusicManager = true;
    [SerializeField] private bool useTestBeat = false;

    private Transform pulseParent;
    private Vector3 originalScale;
    private Coroutine returnRoutine;

    private void Awake()
    {
        // Create pulse container
        GameObject wrapper = new GameObject($"{gameObject.name}_PulseWrapper");
        pulseParent = wrapper.transform;

        // Set wrapper to match the original object
        pulseParent.position = transform.position;
        pulseParent.rotation = transform.rotation;
        pulseParent.localScale = Vector3.one;

        // Reparent the environment object
        transform.SetParent(pulseParent, true);
    }

    private void Start()
    {
        originalScale = pulseParent.localScale;

        if (useMusicManager && MusicManager.Instance != null)
        {
            MusicManager.Instance.OnIntervalPassed.AddListener(Pulse);
        }
        else if (useTestBeat)
        {
            StartCoroutine(TestBeatRoutine());
        }
    }

    private void Pulse()
    {
        if (!pulseParent.gameObject.activeInHierarchy) return;

        pulseParent.localScale = originalScale * pulseSize;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }

        returnRoutine = StartCoroutine(ReturnToOriginalScale());
    }

    private IEnumerator ReturnToOriginalScale()
    {
        while (Vector3.Distance(pulseParent.localScale, originalScale) > 0.01f)
        {
            pulseParent.localScale = Vector3.Lerp(pulseParent.localScale, originalScale, Time.deltaTime * returnSpeed);
            yield return null;
        }

        pulseParent.localScale = originalScale;
    }

    private IEnumerator TestBeatRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Pulse();
        }
    }

    private void OnDestroy()
    {
        if (useMusicManager && MusicManager.Instance != null)
        {
            MusicManager.Instance.OnIntervalPassed.RemoveListener(Pulse);
        }
    }
}
