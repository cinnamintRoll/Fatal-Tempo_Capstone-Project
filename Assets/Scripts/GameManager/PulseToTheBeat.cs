using System.Collections;
using UnityEngine;

public class PulseToTheBeat : MonoBehaviour
{
    [SerializeField] private bool _useMusicManager = true; // Toggle for using MusicManager
    [SerializeField] private bool _useTestBeat = false; // Toggle for using test beat
    [SerializeField] private float _pulseSize = 1.15f;
    [SerializeField] private float _returnSpeed = 5f;
    private Vector3 _startSize;
    private Coroutine _returnToStartSizeCoroutine;

    private void Start()
    {
        _startSize = transform.localScale;

        if (_useMusicManager && MusicManager.Instance != null)
        {
            MusicManager.Instance.OnIntervalPassed.AddListener(Pulse); // Subscribe to MusicManager event
        }
        else if (_useTestBeat)
        {
            StartCoroutine(TestBeat());
        }
    }

    public void Pulse()
    {
        // Set the object to the pulsed size
        transform.localScale = _startSize * _pulseSize;

        // If a coroutine is already running to return to the original size, stop it
        if (_returnToStartSizeCoroutine != null)
        {
            StopCoroutine(_returnToStartSizeCoroutine);
        }

        // Start a new coroutine to return to the original size
        _returnToStartSizeCoroutine = StartCoroutine(ReturnToStartSize());
    }

    private IEnumerator ReturnToStartSize()
    {
        while (transform.localScale != _startSize)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _startSize, Time.deltaTime * _returnSpeed);

            // Break out of the loop if scale is close enough to _startSize to prevent continuous adjustments
            if (Vector3.Distance(transform.localScale, _startSize) < 0.01f)
            {
                transform.localScale = _startSize;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator TestBeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Pulse();
        }
    }

    private void OnDestroy()
    {
        if (_useMusicManager && MusicManager.Instance != null)
        {
            MusicManager.Instance.OnIntervalPassed.RemoveListener(Pulse); // Unsubscribe when destroyed
        }
    }
}
