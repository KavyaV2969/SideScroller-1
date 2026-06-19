using UnityEngine;

public class AIDialogueService : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private AIDialogueClient client;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueResponseDatabase responseDatabase;
    [SerializeField] private GameStateProvider gameStateProvider;

    [Header("Validation")]
    [SerializeField] private DialogueActionValidator actionValidator;
    [SerializeField] private DialogueActionExecutor actionExecutor;

    public async void SubmitPlayerText(AINPCDialogue npc, string playerInput)
    {
        if (npc == null)
        {
            Debug.LogWarning("AIDialogueService received null NPC.");
            return;
        }

        if (string.IsNullOrWhiteSpace(playerInput))
        {
            PlayFallback(npc);
            return;
        }

        if (client == null)
        {
            Debug.LogWarning("AIDialogueService has no AIDialogueClient assigned.");
            PlayFallback(npc);
            return;
        }

        AIDialogueRequest request = new AIDialogueRequest
        {
            npcId = npc.NpcId,
            playerInput = playerInput,
            requestedMode = "auto",
            playerState = gameStateProvider != null
                ? gameStateProvider.BuildSnapshotForNPC(npc.NpcId)
                : new PlayerStateSnapshot()
        };

        AIDialogueResponse response = await client.SendDialogueRequest(request);

        if (response == null)
        {
            PlayFallback(npc);
            return;
        }

        HandleResponse(npc, response);
    }

    private void HandleResponse(AINPCDialogue npc, AIDialogueResponse response)
    {
        if (response.safety != null && response.safety.blocked)
        {
            PlayFallback(npc);
            return;
        }

        if (response.npcId != npc.NpcId)
        {
            Debug.LogWarning($"Response NPC mismatch. Expected {npc.NpcId}, got {response.npcId}");
            PlayFallback(npc);
            return;
        }

        switch (response.responseType)
        {
            case "authored_dialogue":
                HandleAuthoredDialogueResponse(npc, response);
                break;

            case "free_chat":
                HandleFreeChatResponse(npc, response);
                break;

            case "fallback":
                PlayFallback(npc);
                break;

            default:
                Debug.LogWarning($"Unknown AI dialogue responseType: {response.responseType}");
                PlayFallback(npc);
                break;
        }
    }

    private void HandleAuthoredDialogueResponse(AINPCDialogue npc, AIDialogueResponse response)
    {
        if (responseDatabase == null)
        {
            Debug.LogWarning("No DialogueResponseDatabase assigned.");
            PlayFallback(npc);
            return;
        }

        DialogueText dialogueText = responseDatabase.GetDialogueById(response.dialogueId);

        if (dialogueText == null)
        {
            PlayFallback(npc);
            return;
        }

        if (!dialogueText.canBeSelectedByAI)
        {
            Debug.LogWarning($"DialogueText is not AI-selectable: {dialogueText.name}");
            PlayFallback(npc);
            return;
        }

        bool actionsValid = actionValidator == null || actionValidator.AreActionsValid(npc.NpcId, response);

        if (!actionsValid)
        {
            PlayFallback(npc);
            return;
        }

        dialogueController.StartDialogue(dialogueText, response.startNodeId);

        if (actionExecutor != null)
        {
            actionExecutor.Execute(response.proposedActions);
        }
    }

    private void HandleFreeChatResponse(AINPCDialogue npc, AIDialogueResponse response)
    {
        if (response.proposedActions != null && response.proposedActions.Length > 0)
        {
            Debug.LogWarning("Free-chat response included actions. Rejected.");
            PlayFallback(npc);
            return;
        }

        if (string.IsNullOrWhiteSpace(response.freeChatText))
        {
            PlayFallback(npc);
            return;
        }

        dialogueController.DisplayFreeChatLine(npc.DisplayName, response.freeChatText);
    }

    private void PlayFallback(AINPCDialogue npc)
    {
        if (npc.FallbackDialogue != null)
        {
            dialogueController.StartDialogue(npc.FallbackDialogue);
            return;
        }

        dialogueController.DisplayFreeChatLine(npc.DisplayName, "I do not know what to say to that.");
    }
}