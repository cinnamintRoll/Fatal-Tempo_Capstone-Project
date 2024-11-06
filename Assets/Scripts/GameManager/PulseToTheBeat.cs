using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class PulseToTheBeat : MonoBehaviour
{
    [SerializeField] private bool _useMusicManager = true; // Toggle for using MusicManager
    [SerializeField] private bool _useTestBeat = false; // Toggle for using test beat
    [SerializeField] private float _pulseSize = 1.15f;
    [SerializeField] private float _returnSpeed = 5f;
    private Vector3 _startSize;

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

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _startSize, Time.deltaTime * _returnSpeed);
    }

    public void Pulse()
    {
        transform.localScale = _startSize * _pulseSize;
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


