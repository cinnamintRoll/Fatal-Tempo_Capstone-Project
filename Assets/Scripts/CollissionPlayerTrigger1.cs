using UnityEngine;
using UnityEngine.Events;

public class CollisionPlayerTrigger : MonoBehaviour
{
    [Header("Settings")]
    public bool useTrigger = true;

    [Header("Object Layer")]
    public string layer = "Player";

    [Header("Event")]
    public UnityEvent onEventTriggered;

    private int layerToCheck;

    private void Awake()
    {
        layerToCheck = LayerMask.NameToLayer(layer);
        if (layerToCheck == -1)
        {
            Debug.LogWarning("Layer name is invalid: " + layer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if (other.gameObject.layer != layerToCheck) return;

        Debug.Log("Trigger entered by: " + other.name);
        onEventTriggered?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        if (collision.gameObject.layer != layerToCheck) return;

        Debug.Log("Collision with: " + collision.gameObject.name);
        onEventTriggered?.Invoke();
    }
}
