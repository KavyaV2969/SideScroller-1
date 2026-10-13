using UnityEngine;

public class AINPCDialogue : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    [SerializeField] private string npcId;
    [SerializeField] private string displayName;

    [Header("Dialogue")]
    [SerializeField] private DialogueText entryDialogue;
    [SerializeField] private DialogueText fallbackDialogue;

    [Header("References")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private AIDialogueService aiDialogueService;

    public string NpcId => npcId;
    public string DisplayName => displayName;
    public DialogueText EntryDialogue => entryDialogue;
    public DialogueText FallbackDialogue => fallbackDialogue;

    public void Interact()
    {
        if (dialogueController == null || aiDialogueService == null)
        {
            Debug.LogWarning($"{name} is missing dialogue references.");
            return;
        }

        if (dialogueController.IsConversationActive)
        {
            return;
        }

        if (entryDialogue == null)
        {
            Debug.LogWarning($"{name} has no entry dialogue assigned.");
            aiDialogueService.PlayFallback(this);
            return;
        }

        dialogueController.OnFreeTextSubmitted -= HandleFreeTextSubmitted;
        dialogueController.OnFreeTextSubmitted += HandleFreeTextSubmitted;

        dialogueController.OnGeneratedResponseRequested -= HandleGeneratedResponseRequested;
        dialogueController.OnGeneratedResponseRequested += HandleGeneratedResponseRequested;

        dialogueController.OnConversationEnded -= HandleConversationEnded;
        dialogueController.OnConversationEnded += HandleConversationEnded;

        dialogueController.StartDialogue(entryDialogue);
    }

    private void HandleFreeTextSubmitted(string playerInput, DialogueText dialogueText, DialogueNode node)
    {
        aiDialogueService.SubmitPlayerText(this, playerInput, dialogueText, node);
    }

    private void HandleGeneratedResponseRequested(
        DialogueText dialogueText,
        DialogueNode node,
        string playerInput)
    {
        aiDialogueService.RequestGeneratedResponse(this, dialogueText, node, playerInput);
    }

    private void HandleConversationEnded()
    {
        Unsubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (aiDialogueService != null) aiDialogueService.CancelPendingRequest(this);
        if (dialogueController == null)
        {
            return;
        }

        dialogueController.OnFreeTextSubmitted -= HandleFreeTextSubmitted;
        dialogueController.OnGeneratedResponseRequested -= HandleGeneratedResponseRequested;
        dialogueController.OnConversationEnded -= HandleConversationEnded;
    }
}
