using UnityEngine;

public class AIDialogueService : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private AIDialogueClient client;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private GameStateProvider gameStateProvider;

    private int requestVersion;
    private AINPCDialogue requestingNpc;

    public void SubmitPlayerText(AINPCDialogue npc, string playerInput, DialogueText dialogue, DialogueNode node) =>
        SendRequest(npc, playerInput, dialogue, node, false);

    public void RequestGeneratedResponse(AINPCDialogue npc, DialogueText dialogue, DialogueNode node, string playerInput) =>
        SendRequest(npc, playerInput, dialogue, node, true);

    private async void SendRequest(
        AINPCDialogue npc, string input, DialogueText dialogue, DialogueNode node, bool generated)
    {
        if (npc == null || dialogueController == null)
        {
            Debug.LogWarning("AI dialogue requires an NPC and DialogueController.");
            return;
        }

        int version = ++requestVersion;
        int revision = dialogueController.Revision;
        requestingNpc = npc;
        AIDialogueResponse response = null;
        if (client != null && (generated ? node != null : !string.IsNullOrWhiteSpace(input)))
        {
            var request = new AIDialogueRequest
            {
                npcId = npc.NpcId,
                playerInput = input ?? "",
                requestedMode = generated ? "free_response" : "intent_route",
                currentDialogueId = dialogue != null ? dialogue.dialogueId : "",
                currentNodeId = node?.nodeId ?? "",
                generatedResponseRequestId = generated ? node?.generatedResponseRequestId : "",
                playerState = gameStateProvider != null
                    ? gameStateProvider.BuildSnapshotForNPC(npc.NpcId) : new PlayerStateSnapshot()
            };
            try
            {
                response = await client.SendDialogueRequest(request);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"AI dialogue request failed: {exception.Message}");
            }
        }

        // Async replies belong to one display revision, even if the next conversation uses the same asset.
        if (this == null || !isActiveAndEnabled || npc == null || !npc.isActiveAndEnabled ||
            version != requestVersion || dialogueController == null ||
            !dialogueController.IsConversationActive || revision != dialogueController.Revision) return;
        requestingNpc = null;

        bool valid = response != null && response.npcId == npc.NpcId &&
            !(response.safety?.blocked ?? false) &&
            (response.proposedActions == null || response.proposedActions.Length == 0);
        if (generated && node != null)
        {
            valid &= node.contentMode == DialogueNodeContentMode.GeneratedResponse &&
                response?.responseType == "generated_response" && !string.IsNullOrWhiteSpace(response?.freeChatText);
            string text = valid ? response.freeChatText.Trim() : node.generatedFallbackText;
            if (valid && text.Length > Mathf.Max(0, node.maxGeneratedCharacters))
                text = text.Substring(0, Mathf.Max(0, node.maxGeneratedCharacters));
            dialogueController.DisplayGeneratedResponseLine(npc.DisplayName, text);
        }
        else if (!valid || response.responseType != "intent_route" || string.IsNullOrWhiteSpace(response.intent) ||
            !dialogueController.TryRouteCurrentNodeByIntent(response.intent, gameStateProvider))
        {
            PlayFallback(npc);
        }
    }

    public void PlayFallback(AINPCDialogue npc)
    {
        if (npc == null || dialogueController == null) return;
        if (npc.FallbackDialogue != null) dialogueController.StartDialogue(npc.FallbackDialogue);
        else dialogueController.DisplayFreeChatLine(npc.DisplayName, "I do not know what to say to that.");
    }

    public void CancelPendingRequest(AINPCDialogue npc)
    {
        if (requestingNpc != npc) return;
        requestVersion++;
        requestingNpc = null;
    }

    private void OnDisable()
    {
        requestVersion++;
        requestingNpc = null;
    }
}
