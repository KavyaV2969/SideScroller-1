using UnityEngine;

public class Lady : NPC, ITalkable
{
    [SerializeField] private DialogueText dialogueText;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] Player player;

    public override void Interact()
    {
        Debug.Log("Interacting with Lady");
        Talk(dialogueText);

        if (facingDir != player.facingDir)
        {
            Flip();
        }
    }

    public void Talk(DialogueText dialogueText)
    {
        dialogueController.DisplayNextDialogueText(dialogueText);
    }
}
