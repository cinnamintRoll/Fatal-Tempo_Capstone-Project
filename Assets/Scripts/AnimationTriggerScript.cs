using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationTriggerScript : MonoBehaviour
{
    [SerializeField] private UnityEvent unityEvent;
    
    public void triggerEvent()
    {
            unityEvent.Invoke();
    }
}
