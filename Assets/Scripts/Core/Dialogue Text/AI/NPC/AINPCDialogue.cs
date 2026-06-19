using UnityEngine;

public class AINPCDialogue : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    [SerializeField] private string npcId;
    [SerializeField] private string displayName;

    [Header("Fallback")]
    [SerializeField] private DialogueText fallbackDialogue;

    [Header("References")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private AIDialogueService aiDialogueService;

    public string NpcId => npcId;
    public string DisplayName => displayName;
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

        dialogueController.ShowFreeTextInput();
        dialogueController.OnFreeTextSubmitted += HandleFreeTextSubmitted;
    }

    private void HandleFreeTextSubmitted(string playerInput)
    {
        dialogueController.OnFreeTextSubmitted -= HandleFreeTextSubmitted;

        dialogueController.DisplayPendingLine(displayName);

        aiDialogueService.SubmitPlayerText(this, playerInput);
    }

    private void OnDisable()
    {
        if (dialogueController != null)
        {
            dialogueController.OnFreeTextSubmitted -= HandleFreeTextSubmitted;
        }
    }
}