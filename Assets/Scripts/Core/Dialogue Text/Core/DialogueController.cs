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

    public event System.Action<string, DialogueText, DialogueNode> OnFreeTextSubmitted;
    public event System.Action<DialogueText, DialogueNode, string> OnGeneratedResponseRequested;
    public event System.Action OnConversationEnded;

    private DialogueText activeDialogue;
    private readonly Dictionary<string, int> nodeLookup = new Dictionary<string, int>();

    private int currentNodeIndex = -1;
    private bool conversationActive;
    public bool IsConversationActive => conversationActive;
    private bool waitingForChoice;
    private bool waitingForFreeTextInput;
    private bool waitingForAIResponse;
    private string lastSubmittedFreeText;

    public DialogueText ActiveDialogue => activeDialogue;

    public DialogueNode CurrentNode
    {
        get
        {
            if (activeDialogue == null ||
                currentNodeIndex < 0 ||
                currentNodeIndex >= activeDialogue.nodes.Count)
            {
                return null;
            }

            return activeDialogue.nodes[currentNodeIndex];
        }
    }

    private void Awake()
    {
        if (submitTextButton != null)
        {
            submitTextButton.onClick.AddListener(SubmitFreeTextInput);
        }

        HideFreeTextInput();
    }

    public void ShowFreeTextInput(string speakerName, string promptText)
    {
        ClearOptions();

        conversationActive = true;
        waitingForChoice = false;
        waitingForAIResponse = false;
        waitingForFreeTextInput = true;

        gameObject.SetActive(true);

        NPCNameText.text = speakerName;
        NPCDialogueText.text = promptText;

        freeTextInputPanel?.SetActive(true);

        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }

        SetPlayerMovement(false);
    }

    public void ShowFreeTextInput()
    {
        ShowFreeTextInput("", "What do you say?");
    }

    public void HideFreeTextInput()
    {
        freeTextInputPanel?.SetActive(false);
        waitingForFreeTextInput = false;
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

        DialogueText submittedDialogue = activeDialogue;
        DialogueNode submittedNode = CurrentNode;

        lastSubmittedFreeText = input;

        HideFreeTextInput();

        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = true;

        NPCDialogueText.text = "...";

        if (submittedNode != null &&
            submittedNode.freeTextSubmitMode == FreeTextSubmitMode.DirectNode)
        {
            if (string.IsNullOrWhiteSpace(submittedNode.directFreeTextTargetNodeId))
            {
                Debug.LogWarning("FreeText DirectNode mode has no target node id.");
                EndConversation();
                return;
            }

            JumpToNode(submittedNode.directFreeTextTargetNodeId);
            return;
        }

        OnFreeTextSubmitted?.Invoke(input, submittedDialogue, submittedNode);
    }

    public void DisplayNextDialogueText(DialogueText dialogueText)
    {
        if (!conversationActive)
        {
            StartConversation(dialogueText);
            return;
        }

        if (waitingForChoice || waitingForFreeTextInput || waitingForAIResponse)
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
        if (dialogueText == null || dialogueText.nodes == null || dialogueText.nodes.Count == 0)
        {
            Debug.LogWarning("Tried to start null or empty dialogue.");
            EndConversation();
            return;
        }

        HideFreeTextInput();
        activeDialogue = dialogueText;
        conversationActive = true;
        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = false;
        currentNodeIndex = -1;
        lastSubmittedFreeText = "";

        BuildNodeLookup();
        SetPlayerMovement(false);
        gameObject.SetActive(true);
        ShowNode(GetNodeIndexOrFallback(overrideStartNodeId));
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
        StartDialogue(dialogueText);
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
        if (activeDialogue == null || nodeIndex < 0 || nodeIndex >= activeDialogue.nodes.Count)
        {
            EndConversation();
            return;
        }

        ClearOptions();
        HideFreeTextInput();

        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = false;

        currentNodeIndex = nodeIndex;
        DialogueNode node = activeDialogue.nodes[currentNodeIndex];

        if (node.contentMode == DialogueNodeContentMode.GeneratedResponse)
        {
            ShowGeneratedResponseNode(node);
            return;
        }

        string speakerName = string.IsNullOrWhiteSpace(node.speakerNameOverride)
            ? activeDialogue.speakerName
            : node.speakerNameOverride;

        NPCNameText.text = speakerName;
        NPCDialogueText.text = node.text;

        FireTrigger(node.triggerId);

        switch (node.inputMode)
        {
            case DialogueNodeInputMode.Choices:
                if (node.choices != null && node.choices.Count > 0)
                {
                    ShowChoices(node);
                }
                break;

            case DialogueNodeInputMode.FreeText:
                ShowNodeFreeTextInput(speakerName, node.freeTextPrompt);
                break;

            case DialogueNodeInputMode.Continue:
            default:
                break;
        }
    }

    private void ShowGeneratedResponseNode(DialogueNode node)
    {
        ClearOptions();
        HideFreeTextInput();

        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = true;

        string speakerName = string.IsNullOrWhiteSpace(node.speakerNameOverride)
            ? activeDialogue.speakerName
            : node.speakerNameOverride;

        NPCNameText.text = speakerName;
        NPCDialogueText.text = "...";

        OnGeneratedResponseRequested?.Invoke(activeDialogue, node, lastSubmittedFreeText);
    }

    private void ShowNodeFreeTextInput(string speakerName, string promptText)
    {
        waitingForFreeTextInput = true;

        if (!string.IsNullOrWhiteSpace(promptText))
        {
            NPCDialogueText.text = promptText;
        }

        freeTextInputPanel?.SetActive(true);

        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.ActivateInputField();
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
        HideFreeTextInput();

        activeDialogue = null;
        conversationActive = true;
        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = false;
        currentNodeIndex = -1;

        SetPlayerMovement(false);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        NPCNameText.text = speakerName;
        NPCDialogueText.text = line;
    }

    public void DisplayGeneratedResponseLine(string speakerName, string line)
    {
        ClearOptions();
        HideFreeTextInput();

        activeDialogue = null;
        currentNodeIndex = -1;
        conversationActive = true;
        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = false;

        SetPlayerMovement(false);
        gameObject.SetActive(true);

        NPCNameText.text = speakerName;
        NPCDialogueText.text = SanitizeGeneratedText(line);
    }

    private string SanitizeGeneratedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        return text.Replace("<", "").Replace(">", "").Trim();
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
        if (!conversationActive)
        {
            return;
        }

        if (activeDialogue == null)
        {
            EndConversation();
            return;
        }

        if (currentNodeIndex < 0)
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
        HideFreeTextInput();

        activeDialogue = null;
        currentNodeIndex = -1;
        conversationActive = false;
        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = false;
        lastSubmittedFreeText = "";

        SetPlayerMovement(true);

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        OnConversationEnded?.Invoke();
    }

    private void OnDisable()
    {
        SetPlayerMovement(true);
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
        HideFreeTextInput();

        conversationActive = true;
        waitingForChoice = false;
        waitingForFreeTextInput = false;
        waitingForAIResponse = true;

        SetPlayerMovement(false);
        gameObject.SetActive(true);

        NPCNameText.text = speakerName;
        NPCDialogueText.text = "...";
    }

    public bool TryRouteCurrentNodeByIntent(string intent, GameStateProvider gameStateProvider)
    {
        waitingForAIResponse = false;

        DialogueNode node = CurrentNode;

        if (node == null)
        {
            Debug.LogWarning("Cannot route intent because there is no current dialogue node.");
            EndConversation();
            return false;
        }

        DialogueIntentRoute route = FindIntentRoute(node, intent);

        if (route == null)
        {
            return RouteToUnknownIntentNode(node);
        }

        if (!IsRouteAllowed(route, gameStateProvider))
        {
            Debug.LogWarning($"Intent route blocked by conditions. Intent: {intent}");
            return RouteToUnknownIntentNode(node);
        }

        if (string.IsNullOrWhiteSpace(route.nextNodeId))
        {
            Debug.LogWarning($"Intent route has no next node. Intent: {intent}");
            return RouteToUnknownIntentNode(node);
        }

        JumpToNode(route.nextNodeId);
        return true;
    }

    private DialogueIntentRoute FindIntentRoute(DialogueNode node, string intent)
    {
        if (node.intentRoutes == null || string.IsNullOrWhiteSpace(intent))
        {
            return null;
        }

        foreach (DialogueIntentRoute route in node.intentRoutes)
        {
            if (route != null && route.intent == intent)
            {
                return route;
            }
        }

        return null;
    }

    private bool RouteToUnknownIntentNode(DialogueNode node)
    {
        if (!string.IsNullOrWhiteSpace(node.unknownIntentNodeId))
        {
            JumpToNode(node.unknownIntentNodeId);
            return true;
        }

        Debug.LogWarning("No matching intent route and no unknownIntentNodeId set.");
        EndConversation();
        return false;
    }

    private bool IsRouteAllowed(DialogueIntentRoute route, GameStateProvider gameStateProvider)
    {
        if (route == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(route.requiredFlag))
        {
            if (gameStateProvider == null || !gameStateProvider.HasFlag(route.requiredFlag))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(route.blockedByFlag))
        {
            if (gameStateProvider != null && gameStateProvider.HasFlag(route.blockedByFlag))
            {
                return false;
            }
        }

        return true;
    }
}
