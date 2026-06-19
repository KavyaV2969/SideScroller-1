using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private List<string> startingFlags = new List<string>();

    private readonly HashSet<string> activeFlags = new HashSet<string>();

    private void Awake()
    {
        foreach (string flag in startingFlags)
        {
            if (!string.IsNullOrWhiteSpace(flag))
            {
                activeFlags.Add(flag);
            }
        }
    }

    public void SetFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            Debug.LogWarning("Tried to set an empty quest flag.");
            return;
        }

        activeFlags.Add(flagId);
    }

    public bool HasFlag(string flagId)
    {
        return !string.IsNullOrWhiteSpace(flagId) && activeFlags.Contains(flagId);
    }

    public string[] GetActiveFlags()
    {
        string[] flags = new string[activeFlags.Count];
        activeFlags.CopyTo(flags);
        return flags;
    }

    public string[] GetCompletedFlags()
    {
        return new string[0];
    }

    public void ClearFlag(string flagId)
    {
        if (!string.IsNullOrWhiteSpace(flagId))
        {
            activeFlags.Remove(flagId);
        }
    }
}
