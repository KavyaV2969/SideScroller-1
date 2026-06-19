using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Response Database")]
public class DialogueResponseDatabase : ScriptableObject
{
    [SerializeField] private List<DialogueResponseEntry> responses = new List<DialogueResponseEntry>();

    private Dictionary<string, DialogueText> lookup;

    public DialogueText GetDialogueById(string dialogueId)
    {
        if (string.IsNullOrWhiteSpace(dialogueId))
        {
            Debug.LogWarning("Dialogue id was null or empty.");
            return null;
        }

        if (lookup == null)
        {
            BuildLookup();
        }

        if (lookup.TryGetValue(dialogueId, out DialogueText dialogueText))
        {
            return dialogueText;
        }

        Debug.LogWarning($"Dialogue id not found in DialogueResponseDatabase: {dialogueId}");
        return null;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, DialogueText>();

        foreach (DialogueResponseEntry entry in responses)
        {
            if (entry == null || entry.dialogueText == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.dialogueId))
            {
                Debug.LogWarning($"DialogueResponseEntry has no dialogueId: {entry.dialogueText.name}");
                continue;
            }

            if (lookup.ContainsKey(entry.dialogueId))
            {
                Debug.LogWarning($"Duplicate dialogue id in DialogueResponseDatabase: {entry.dialogueId}");
                continue;
            }

            lookup.Add(entry.dialogueId, entry.dialogueText);
        }
    }
}

[System.Serializable]
public class DialogueResponseEntry
{
    public string dialogueId;
    public DialogueText dialogueText;
}