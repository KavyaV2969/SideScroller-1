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

    public async void RequestGeneratedResponse(
        AINPCDialogue npc,
        DialogueText currentDialogue,
        DialogueNode currentNode,
        string playerInput)
    {
        if (npc == null)
        {
            Debug.LogWarning("AIDialogueService received null NPC for generated response.");
            return;
        }

        if (currentNode == null)
        {
            Debug.LogWarning("AIDialogueService received null generated-response node.");
            PlayFallback(npc);
            return;
        }

        if (dialogueController == null)
        {
            Debug.LogWarning("AIDialogueService has no DialogueController assigned.");
            return;
        }

        if (client == null)
        {
            Debug.LogWarning("AIDialogueService has no AIDialogueClient assigned.");
            DisplayGeneratedFallback(npc, currentNode);
            return;
        }

        AIDialogueRequest request = new AIDialogueRequest
        {
            npcId = npc.NpcId,
            playerInput = playerInput ?? "",
            requestedMode = "free_response",
            currentDialogueId = currentDialogue != null ? currentDialogue.dialogueId : "",
            currentNodeId = currentNode.nodeId,
            generatedResponseRequestId = currentNode.generatedResponseRequestId,
            playerState = gameStateProvider != null
                ? gameStateProvider.BuildSnapshotForNPC(npc.NpcId)
                : new PlayerStateSnapshot()
        };

        try
        {
            AIDialogueResponse response = await client.SendDialogueRequest(request);

            if (!IsValidGeneratedResponse(response, npc, currentNode))
            {
                DisplayGeneratedFallback(npc, currentNode);
                return;
            }

            string text = response.freeChatText.Trim();
            int maxCharacters = Mathf.Max(0, currentNode.maxGeneratedCharacters);

            if (text.Length > maxCharacters)
            {
                text = text.Substring(0, maxCharacters);
            }

            dialogueController.DisplayGeneratedResponseLine(npc.DisplayName, text);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Generated dialogue flow failed: {exception.Message}");
            DisplayGeneratedFallback(npc, currentNode);
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

    private bool IsValidGeneratedResponse(
        AIDialogueResponse response,
        AINPCDialogue npc,
        DialogueNode node)
    {
        if (response == null ||
            node == null ||
            node.contentMode != DialogueNodeContentMode.GeneratedResponse)
        {
            return false;
        }

        if (response.safety != null && response.safety.blocked)
        {
            return false;
        }

        if (response.npcId != npc.NpcId)
        {
            return false;
        }

        if (response.responseType != "generated_response")
        {
            return false;
        }

        if (response.proposedActions != null && response.proposedActions.Length > 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(response.freeChatText))
        {
            return false;
        }

        return true;
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

    private void DisplayGeneratedFallback(AINPCDialogue npc, DialogueNode node)
    {
        if (dialogueController == null)
        {
            return;
        }

        dialogueController.DisplayGeneratedResponseLine(
            npc.DisplayName,
            node.generatedFallbackText);
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
