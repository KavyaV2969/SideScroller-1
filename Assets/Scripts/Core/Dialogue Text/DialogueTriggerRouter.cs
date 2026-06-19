using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTriggerRouter : MonoBehaviour
{
    [SerializeField] private List<DialogueTriggerBinding> triggerBindings = new List<DialogueTriggerBinding>();

    public void RunTrigger(string triggerId)
    {
        if (string.IsNullOrWhiteSpace(triggerId))
        {
            return;
        }

        foreach (DialogueTriggerBinding binding in triggerBindings)
        {
            if (binding.triggerId == triggerId)
            {
                binding.response?.Invoke();
                return;
            }
        }

        Debug.LogWarning($"No dialogue trigger binding found for id: {triggerId}");
    }
}

[Serializable]
public class DialogueTriggerBinding
{
    public string triggerId;
    public UnityEvent response;
}
