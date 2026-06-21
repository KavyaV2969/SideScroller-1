[System.Serializable]
public class AIDialogueRequest
{
    public string npcId;
    public string playerInput;
    public string requestedMode;

    public string currentDialogueId;
    public string currentNodeId;

    public PlayerStateSnapshot playerState;
}