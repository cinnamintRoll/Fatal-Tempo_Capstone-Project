using UnityEngine;

public class ScaleByDistance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Distance Scaling Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;

    [Header("Smooth Transition")]
    [SerializeField] private float scaleSpeed = 5f;

    private void Start()
    {
        playerTransform = GameManager.Instance.PlayerTransform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance); // 0 = close, 1 = far
        float targetScale = Mathf.Lerp(minScale, maxScale, t); // closer = smaller, farther = bigger

        Vector3 newScale = Vector3.one * targetScale;
        transform.localScale = Vector3.Lerp(transform.localScale, newScale, Time.deltaTime * scaleSpeed);
    }
}
