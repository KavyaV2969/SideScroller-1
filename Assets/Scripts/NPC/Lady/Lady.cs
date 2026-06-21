using UnityEngine;

public class Lady : NPC, ITalkable
{
    [Header("Normal Dialogue")]
    [SerializeField] private DialogueText dialogueText;
    [SerializeField] private DialogueController dialogueController;

    [Header("References")]
    [SerializeField] private Player player;

    private AINPCDialogue aiDialogue;

    protected override void Awake()
    {
        base.Awake();

        aiDialogue = GetComponent<AINPCDialogue>();
    }

    protected override void Start()
    {
        base.Start();

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }
    }

    public override void Interact()
    {
        Debug.Log("Interacting with Lady");

        if (dialogueController == null)
        {
            Debug.LogWarning($"{name} has no DialogueController assigned.");
            return;
        }

        // If this is an AI NPC and no conversation is currently active,
        // open the AI free-text input.
        if (aiDialogue != null && !dialogueController.IsConversationActive)
        {
            aiDialogue.Interact();
        }
        else
        {
            // For normal NPCs, this starts OR advances dialogue.
            // For active AI dialogue/free-chat, this allows continuing/closing.
            Talk(dialogueText);
        }

        FacePlayer();
    }

    public void Talk(DialogueText dialogueText)
    {
        dialogueController.DisplayNextDialogueText(dialogueText);
    }

    private void FacePlayer()
    {
        if (player == null)
        {
            return;
        }

        if (facingDir != player.facingDir)
        {
            Flip();
        }
    }
}