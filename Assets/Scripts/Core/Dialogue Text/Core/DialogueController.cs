using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;

    [Header("Options UI")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button optionButtonPrefab;

    [Header("Player")]
    [SerializeField] private Player player;

    [Header("Triggers")]
    public UnityEvent<string> onDialogueTrigger;

    [Header("Free Text Input")]
    [SerializeField] private GameObject freeTextInputPanel;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Button submitTextButton;

    public event System.Action<string> OnFreeTextSubmitted;

    private DialogueText activeDialogue;
    private readonly Dictionary<string, int> nodeLookup = new Dictionary<string, int>();

    private int currentNodeIndex = -1;
    private bool conversationActive;
    public bool IsConversationActive => conversationActive;
    private bool waitingForChoice;

    private void Awake()
    {
        if (submitTextButton != null)
        {
            submitTextButton.onClick.AddListener(SubmitFreeTextInput);
        }

        HideFreeTextInput();
    }

    public void ShowFreeTextInput()
    {
        ClearOptions();

        if (freeTextInputPanel != null)
        {
            freeTextInputPanel.SetActive(true);
        }

        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }

        SetPlayerMovement(false);
    }

    public void HideFreeTextInput()
    {
        if (freeTextInputPanel != null)
        {
            freeTextInputPanel.SetActive(false);
        }
    }

    private void SubmitFreeTextInput()
    {
        if (playerInputField == null)
        {
            return;
        }

        string input = playerInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        HideFreeTextInput();

        OnFreeTextSubmitted?.Invoke(input);
    }

    public void DisplayNextDialogueText(DialogueText dialogueText)
    {
        if (!conversationActive)
        {
            StartConversation(dialogueText);
            return;
        }

        if (waitingForChoice)
        {
            return;
        }

        AdvanceFromCurrentNode();
    }

    public void StartDialogue(DialogueText dialogueText)
    {
        StartDialogue(dialogueText, null);
    }

    public void StartDialogue(DialogueText dialogueText, string overrideStartNodeId)
    {
        if (dialogueText == null)
        {
            Debug.LogWarning("Tried to start null dialogue.");
            return;
        }

        activeDialogue = dialogueText;
        conversationActive = true;
        waitingForChoice = false;

        BuildNodeLookup();

        SetPlayerMovement(false);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        int startIndex = GetNodeIndexOrFallback(overrideStartNodeId);
        ShowNode(startIndex);
    }

    private int GetNodeIndexOrFallback(string nodeId)
{
    if (!string.IsNullOrWhiteSpace(nodeId) &&
        nodeLookup.TryGetValue(nodeId, out int index))
    {
        return index;
    }

    return GetStartNodeIndex();
}

    private void StartConversation(DialogueText dialogueText)
    {
        if (dialogueText == null || dialogueText.nodes == null || dialogueText.nodes.Count == 0)
        {
            Debug.LogWarning("DialogueText is empty or missing nodes.");
            return;
        }

        activeDialogue = dialogueText;
        conversationActive = true;
        waitingForChoice = false;

        BuildNodeLookup();

        SetPlayerMovement(false);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        int startIndex = GetStartNodeIndex();
        ShowNode(startIndex);
    }

    private void BuildNodeLookup()
    {
        nodeLookup.Clear();

        for (int i = 0; i < activeDialogue.nodes.Count; i++)
        {
            string id = activeDialogue.nodes[i].nodeId;

            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            if (nodeLookup.ContainsKey(id))
            {
                Debug.LogWarning($"Duplicate dialogue node id found: {id}");
                continue;
            }

            nodeLookup.Add(id, i);
        }
    }

    private int GetStartNodeIndex()
    {
        if (!string.IsNullOrWhiteSpace(activeDialogue.startNodeId))
        {
            if (nodeLookup.TryGetValue(activeDialogue.startNodeId, out int index))
            {
                return index;
            }

            Debug.LogWarning($"Start node id not found: {activeDialogue.startNodeId}. Falling back to first node.");
        }

        return 0;
    }

    private void ShowNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= activeDialogue.nodes.Count)
        {
            EndConversation();
            return;
        }

        ClearOptions();

        currentNodeIndex = nodeIndex;
        DialogueNode node = activeDialogue.nodes[currentNodeIndex];

        string speakerName = string.IsNullOrWhiteSpace(node.speakerNameOverride)
            ? activeDialogue.speakerName
            : node.speakerNameOverride;

        NPCNameText.text = speakerName;
        NPCDialogueText.text = node.text;

        FireTrigger(node.triggerId);

        if (node.choices != null && node.choices.Count > 0)
        {
            ShowChoices(node);
        }
    }

    private void ShowChoices(DialogueNode node)
    {
        waitingForChoice = true;

        for (int i = 0; i < node.choices.Count; i++)
        {
            int choiceIndex = i;
            DialogueChoice choice = node.choices[i];

            Button button = Instantiate(optionButtonPrefab, optionsContainer);
            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.optionText;
            }

            button.onClick.AddListener(() => SelectChoice(choiceIndex));
        }
    }

    public void DisplayFreeChatLine(string speakerName, string line)
    {
        ClearOptions();

        activeDialogue = null;
        conversationActive = true;
        waitingForChoice = false;

        SetPlayerMovement(false);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        NPCNameText.text = speakerName;
        NPCDialogueText.text = line;
    }

    private void EndDisplayedFreeChat()
    {
        EndConversation();
    }

    private void SelectChoice(int choiceIndex)
    {
        if (!conversationActive || currentNodeIndex < 0)
        {
            return;
        }

        DialogueNode currentNode = activeDialogue.nodes[currentNodeIndex];

        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
        {
            return;
        }

        DialogueChoice choice = currentNode.choices[choiceIndex];

        FireTrigger(choice.triggerId);
        ClearOptions();

        waitingForChoice = false;

        if (choice.endsConversationAfterThisLine)
        {
            EndConversation();
            return;
        }

        if (!string.IsNullOrWhiteSpace(choice.nextNodeId))
        {
            JumpToNode(choice.nextNodeId);
            return;
        }

        AdvanceFromCurrentNode();
    }

    private void AdvanceFromCurrentNode()
    {
        if (!conversationActive || currentNodeIndex < 0)
        {
            return;
        }

        DialogueNode currentNode = activeDialogue.nodes[currentNodeIndex];

        if (currentNode.endsConversationAfterThisLine)
        {
            EndConversation();
            return;
        }

        if (!string.IsNullOrWhiteSpace(currentNode.nextNodeId))
        {
            JumpToNode(currentNode.nextNodeId);
            return;
        }

        int nextIndex = currentNodeIndex + 1;

        if (nextIndex >= activeDialogue.nodes.Count)
        {
            EndConversation();
            return;
        }

        ShowNode(nextIndex);
    }

    private void JumpToNode(string nodeId)
    {
        if (nodeLookup.TryGetValue(nodeId, out int index))
        {
            ShowNode(index);
            return;
        }

        Debug.LogWarning($"Dialogue node id not found: {nodeId}");
        EndConversation();
    }

    private void FireTrigger(string triggerId)
    {
        if (string.IsNullOrWhiteSpace(triggerId))
        {
            return;
        }

        onDialogueTrigger?.Invoke(triggerId);
    }

    private void ClearOptions()
    {
        if (optionsContainer == null)
        {
            return;
        }

        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsContainer.GetChild(i).gameObject);
        }
    }

    private void EndConversation()
    {
        ClearOptions();

        activeDialogue = null;
        currentNodeIndex = -1;
        conversationActive = false;
        waitingForChoice = false;

        SetPlayerMovement(true);

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (conversationActive)
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
    public void DisplayPendingLine(string speakerName)
    {
        ClearOptions();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        NPCNameText.text = speakerName;
        NPCDialogueText.text = "...";
    }
}
