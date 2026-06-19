[System.Serializable]
public class AIDialogueRequest
{
    public string npcId;
    public string playerInput;
    public string requestedMode;
    public PlayerStateSnapshot playerState;
}