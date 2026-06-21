using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    
    [Header("Identity")]
    public string dialogueId;

    public string speakerName;

    public bool canBeSelectedByAI = false;

    [Tooltip("Optional. If empty, dialogue starts at the first node.")]
    public string startNodeId;

    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[Serializable]
public class DialogueNode
{
    [Header("Node")]
    public string nodeId;

    public string speakerNameOverride;

    [TextArea(3, 8)]
    public string text;

    [Header("Input Mode")]
    public DialogueNodeInputMode inputMode = DialogueNodeInputMode.Continue;

    [Tooltip("Shown when this node expects typed player input.")]
    public string freeTextPrompt = "What do you say?";

    [Header("Flow")]
    public string nextNodeId;
    public bool endsConversationAfterThisLine;

    [Header("Trigger")]
    public string triggerId;

    [Header("Choices")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Header("AI Intent Routes")]
    public List<DialogueIntentRoute> intentRoutes = new List<DialogueIntentRoute>();

    [Tooltip("Fallback node if classifier returns unknown or no route matches.")]
    public string unknownIntentNodeId;
}

[Serializable]
public class DialogueChoice
{
    public string optionText;
    public string triggerId;
    public string nextNodeId;
    public bool endsConversationAfterThisLine;
}

public enum DialogueNodeInputMode
{
    Continue,
    Choices,
    FreeText
}

[Serializable]
public class DialogueIntentRoute
{
    public string intent;
    public string nextNodeId;

    [Header("Optional Route Conditions")]
    public string requiredFlag;
    public string blockedByFlag;
}
