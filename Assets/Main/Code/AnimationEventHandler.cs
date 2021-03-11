using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationEvent
    {
        public string name;
        public UnityEvent OnAnimationEvent;
    }
    public AnimationEvent[] animationEvents;
    public void CallAnimationEvent(string name)
    {
        bool animationEventFound = false;
        for (int i = 0; i < animationEvents.Length; i++)
        {
            ref AnimationEvent animationEvent = ref animationEvents[i];
            if (animationEvents[i].name == name)
            {
                animationEvents[i].OnAnimationEvent.Invoke();
                animationEventFound = true;
            }
        }
        if (!animationEventFound)
        {
            Debug.LogWarning("The animation event name was not found.");
        }
    }
}