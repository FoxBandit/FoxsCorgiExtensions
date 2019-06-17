using System;
using UnityEngine;
using UnityEngine.Events;

public class MultipleEventTrigger : MonoBehaviour
{
    public int numberOfTriggers = 0;
    [Sirenix.OdinInspector.ReadOnly] public int triggerCount = 0;
    public UnityEvent keyAction;

    public void IncrementTrigger()
    {
        triggerCount += 1;
        CheckTriggerConditions();
    }

    public void DecrementTrigger()
    {
        triggerCount -= 1;
        CheckTriggerConditions();
    }

    private void CheckTriggerConditions()
    {
        if (triggerCount == numberOfTriggers)
            keyAction.Invoke();
    }
}