using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    
    [Header("Identity")]
    public string dialogueId;

    public string speakerName;

    public bool canBeSelectedByAI = true;

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

    [Header("Flow")]
    public string nextNodeId;
    public bool endsConversationAfterThisLine;

    [Header("Trigger")]
    public string triggerId;

    [Header("Choices")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Header("AI Routing")]
    public bool canBeSelectedByAI;
}

[Serializable]
public class DialogueChoice
{
    public string optionText;
    public string triggerId;
    public string nextNodeId;
    public bool endsConversationAfterThisLine;
}
