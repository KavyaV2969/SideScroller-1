using UnityEngine;

public class AIDialogueService : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private AIDialogueClient client;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private GameStateProvider gameStateProvider;

    public async void SubmitPlayerText(
        AINPCDialogue npc,
        string playerInput,
        DialogueText currentDialogue,
        DialogueNode currentNode
    ) {
        if (npc == null)
        {
            Debug.LogWarning("AIDialogueService received null NPC.");
            return;
        }

        if (dialogueController == null)
        {
            Debug.LogWarning("AIDialogueService has no DialogueController assigned.");
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
            requestedMode = "intent_route",
            currentDialogueId = currentDialogue != null ? currentDialogue.dialogueId : "",
            currentNodeId = currentNode != null ? currentNode.nodeId : "",
            playerState = gameStateProvider != null
                ? gameStateProvider.BuildSnapshotForNPC(npc.NpcId)
                : new PlayerStateSnapshot()
        };

        try
        {
            AIDialogueResponse response = await client.SendDialogueRequest(request);

            if (response == null)
            {
                PlayFallback(npc);
                return;
            }

            HandleResponse(npc, response);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"AI dialogue flow failed: {exception.Message}");
            PlayFallback(npc);
        }
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
            case "intent_route":
                HandleIntentRouteResponse(npc, response);
                break;

            default:
                Debug.LogWarning($"Rejected non-routing AI responseType: {response.responseType}");
                PlayFallback(npc);
                break;
        }
    }

    private void PlayFallback(AINPCDialogue npc)
    {
        if (dialogueController == null)
        {
            Debug.LogWarning("Cannot play fallback because DialogueController is not assigned.");
            return;
        }

        if (npc.FallbackDialogue != null)
        {
            dialogueController.StartDialogue(npc.FallbackDialogue);
            return;
        }

        dialogueController.DisplayFreeChatLine(npc.DisplayName, "I do not know what to say to that.");
    }

    private void HandleIntentRouteResponse(AINPCDialogue npc, AIDialogueResponse response)
    {
        if (response.proposedActions != null && response.proposedActions.Length > 0)
        {
            Debug.LogWarning("Intent-route response included actions. Rejected.");
            PlayFallback(npc);
            return;
        }

        if (string.IsNullOrWhiteSpace(response.intent))
        {
            Debug.LogWarning("Intent-route response had no intent.");
            PlayFallback(npc);
            return;
        }

        bool routed = dialogueController.TryRouteCurrentNodeByIntent(
            response.intent,
            gameStateProvider
        );

        if (!routed)
        {
            PlayFallback(npc);
        }
    }
}
