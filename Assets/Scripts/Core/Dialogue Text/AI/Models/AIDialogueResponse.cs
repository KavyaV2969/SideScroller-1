[System.Serializable]
public class AIDialogueResponse
{
    public string responseType;
    public string npcId;
    public string intent;
    public string dialogueId;
    public string startNodeId;
    public string freeChatText;
    public AIDialogueAction[] proposedActions;
    public AISafetyInfo safety;
}

[System.Serializable]
public class AIDialogueAction
{
    public string actionType;
    public string itemId;
    public int quantity;
    public string flagId;
    public string questId;
}

[System.Serializable]
public class AISafetyInfo
{
    public bool blocked;
    public string reason;
}