using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationTriggerScript : MonoBehaviour
{
    [System.Serializable]
    public class NamedEvent
    {
        public string eventName;
        public UnityEvent unityEvent;
    }

    [SerializeField] private List<NamedEvent> namedEvents = new List<NamedEvent>();

    public void TriggerEventByName(string eventName)
    {
        var namedEvent = namedEvents.Find(e => e.eventName == eventName);
        if (namedEvent != null)
        {
            namedEvent.unityEvent.Invoke();
        }
        else
        {
            Debug.LogWarning($"No event found with name {eventName}");
        }
    }
}
