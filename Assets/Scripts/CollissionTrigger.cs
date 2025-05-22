using UnityEngine;
using UnityEngine.Events;

public class CollisionTrigger : MonoBehaviour
{
    [Header("Settings")]
    public bool useTrigger = true;

    [Header("Event")]
    public UnityEvent onEventTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        Debug.Log("Trigger entered by: " + other.name);
        onEventTriggered?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        Debug.Log("Collision with: " + collision.gameObject.name);
        onEventTriggered?.Invoke();
    }
}
