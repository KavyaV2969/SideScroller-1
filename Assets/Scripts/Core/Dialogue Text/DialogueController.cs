using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private Player player;
    private bool conversationEnded;
    private string paragraph;
    private Queue<string> paragraphs = new Queue<string>();

    public void DisplayNextDialogueText(DialogueText dialogueText)
    {
        //If there is nothing in the queue
        if(paragraphs.Count == 0)
        {
            if (!conversationEnded)
            {
                StartConversation(dialogueText);
            }
            else
            {
                EndConversation();
                return;
            }
        }

        //If there is something in the queue
        paragraph = paragraphs.Dequeue();
        NPCDialogueText.text = paragraph;

        if (paragraphs.Count == 0)
        {
            conversationEnded = true;
        }
    }

    private void StartConversation(DialogueText dialogueText)
    {
        SetPlayerMovement(false);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        NPCNameText.text = dialogueText.speakerName;

        for (int i = 0; i < dialogueText.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
        }
    }

    private void EndConversation()
    {
        paragraphs.Clear();

        conversationEnded = false;
        SetPlayerMovement(true);
        
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (conversationEnded || paragraphs.Count > 0)
        {
            SetPlayerMovement(true);
        }
    }

    private void SetPlayerMovement(bool enabled)
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (player != null)
        {
            player.SetMovementEnabled(enabled);
        }
    }
}
