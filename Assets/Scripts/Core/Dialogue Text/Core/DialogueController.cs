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

    private enum DialogueState { Closed, Continue, Choices, FreeText, Pending }
    private DialogueState state;
    private DialogueText activeDialogue;
    private readonly Dictionary<string, int> nodeLookup = new Dictionary<string, int>();
    private int currentNodeIndex = -1;
    private string lastSubmittedFreeText = "";

    public bool IsConversationActive => state != DialogueState.Closed;
    public DialogueText ActiveDialogue => activeDialogue;
    // Changes on every display/close, including revisiting the same authored node.
    public int Revision { get; private set; }
    public DialogueNode CurrentNode => activeDialogue != null && currentNodeIndex >= 0 &&
        currentNodeIndex < activeDialogue.nodes.Count ? activeDialogue.nodes[currentNodeIndex] : null;

    private void Awake()
    {
        submitTextButton?.onClick.AddListener(SubmitFreeTextInput);
        freeTextInputPanel?.SetActive(false);
    }

    private void OnDestroy() => submitTextButton?.onClick.RemoveListener(SubmitFreeTextInput);

    public void StartDialogue(DialogueText dialogueText) => StartDialogue(dialogueText, null);

    public void StartDialogue(DialogueText dialogueText, string overrideStartNodeId)
    {
        if (dialogueText == null || dialogueText.nodes == null || dialogueText.nodes.Count == 0)
        {
            Debug.LogWarning("Tried to start null or empty dialogue.");
            EndConversation();
            return;
        }

        activeDialogue = dialogueText;
        currentNodeIndex = -1;
        lastSubmittedFreeText = "";
        nodeLookup.Clear();
        for (int i = 0; i < dialogueText.nodes.Count; i++)
        {
            string id = dialogueText.nodes[i]?.nodeId;
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (!nodeLookup.TryAdd(id, i)) Debug.LogWarning($"Duplicate dialogue node id found: {id}");
        }

        int start = 0;
        if (!string.IsNullOrWhiteSpace(overrideStartNodeId) &&
            nodeLookup.TryGetValue(overrideStartNodeId, out int overrideIndex)) start = overrideIndex;
        else if (!string.IsNullOrWhiteSpace(dialogueText.startNodeId) &&
            !nodeLookup.TryGetValue(dialogueText.startNodeId, out start))
            Debug.LogWarning($"Start node id not found: {dialogueText.startNodeId}. Falling back to first node.");
        ShowNode(start);
    }

    public void DisplayNextDialogueText(DialogueText dialogueText)
    {
        if (!IsConversationActive) StartDialogue(dialogueText);
        else if (state == DialogueState.Continue) AdvanceFromCurrentNode();
    }

    private void ShowNode(int index)
    {
        if (activeDialogue == null || index < 0 || index >= activeDialogue.nodes.Count ||
            activeDialogue.nodes[index] == null)
        {
            EndConversation();
            return;
        }

        currentNodeIndex = index;
        DialogueNode node = CurrentNode;
        string speaker = string.IsNullOrWhiteSpace(node.speakerNameOverride)
            ? activeDialogue.speakerName : node.speakerNameOverride;

        if (node.contentMode == DialogueNodeContentMode.GeneratedResponse)
        {
            Present(DialogueState.Pending, speaker, "...");
            OnGeneratedResponseRequested?.Invoke(activeDialogue, node, lastSubmittedFreeText);
            return;
        }

        Present(DialogueState.Continue, speaker, node.text);
        int revision = Revision;
        FireTrigger(node.triggerId);
        // A trigger is allowed to close or replace the dialogue.
        if (Revision != revision) return;

        if (node.inputMode == DialogueNodeInputMode.FreeText)
        {
            OpenTextInput(string.IsNullOrWhiteSpace(node.freeTextPrompt) ? node.text : node.freeTextPrompt);
        }
        else if (node.inputMode == DialogueNodeInputMode.Choices &&
            node.choices != null && node.choices.Count > 0)
        {
            state = DialogueState.Choices;
            for (int i = 0; i < node.choices.Count; i++)
            {
                int choiceIndex = i;
                DialogueChoice choice = node.choices[i];
                if (choice == null) continue;
                Button button = Instantiate(optionButtonPrefab, optionsContainer);
                button.gameObject.SetActive(true);
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = choice.optionText;
                button.onClick.AddListener(() => { if (Revision == revision) SelectChoice(choiceIndex); });
            }
        }
    }

    // All display paths share UI cleanup, input locking, and request invalidation.
    private void Present(DialogueState nextState, string speaker, string line)
    {
        ClearOptions();
        freeTextInputPanel?.SetActive(false);
        state = nextState;
        Revision++;
        gameObject.SetActive(true);
        NPCNameText.text = speaker;
        NPCDialogueText.text = line;
        SetPlayerMovement(false);
    }

    public void ShowFreeTextInput(string speakerName, string promptText)
    {
        Present(DialogueState.FreeText, speakerName, promptText);
        OpenTextInput(promptText);
    }

    public void ShowFreeTextInput() => ShowFreeTextInput("", "What do you say?");

    private void OpenTextInput(string prompt)
    {
        state = DialogueState.FreeText;
        NPCDialogueText.text = prompt;
        freeTextInputPanel?.SetActive(true);
        if (playerInputField == null) return;
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    public void HideFreeTextInput()
    {
        freeTextInputPanel?.SetActive(false);
        if (state == DialogueState.FreeText) state = DialogueState.Continue;
    }

    private void SubmitFreeTextInput()
    {
        if (state != DialogueState.FreeText || playerInputField == null) return;
        string input = playerInputField.text.Trim();
        if (string.IsNullOrWhiteSpace(input)) return;
        lastSubmittedFreeText = input;
        DialogueNode node = CurrentNode;
        DisplayPendingLine(NPCNameText.text);

        if (node != null && node.freeTextSubmitMode == FreeTextSubmitMode.DirectNode)
        {
            JumpToNode(node.directFreeTextTargetNodeId);
            return;
        }
        OnFreeTextSubmitted?.Invoke(input, activeDialogue, node);
    }

    public void DisplayPendingLine(string speakerName) => Present(DialogueState.Pending, speakerName, "...");

    public void DisplayFreeChatLine(string speakerName, string line)
    {
        activeDialogue = null;
        currentNodeIndex = -1;
        Present(DialogueState.Continue, speakerName, line);
    }

    public void DisplayGeneratedResponseLine(string speakerName, string line) =>
        DisplayFreeChatLine(speakerName, (line ?? "").Replace("<", "").Replace(">", "").Trim());

    private void SelectChoice(int index)
    {
        DialogueNode node = CurrentNode;
        if (state != DialogueState.Choices || node?.choices == null ||
            index < 0 || index >= node.choices.Count) return;
        DialogueChoice choice = node.choices[index];
        if (choice == null) return;
        int revision = Revision;
        state = DialogueState.Continue;
        FireTrigger(choice.triggerId);
        if (Revision != revision) return;
        if (choice.endsConversationAfterThisLine) EndConversation();
        else if (!string.IsNullOrWhiteSpace(choice.nextNodeId)) JumpToNode(choice.nextNodeId);
        else AdvanceFromCurrentNode();
    }

    private void AdvanceFromCurrentNode()
    {
        DialogueNode node = CurrentNode;
        if (node == null || node.endsConversationAfterThisLine) EndConversation();
        else if (!string.IsNullOrWhiteSpace(node.nextNodeId)) JumpToNode(node.nextNodeId);
        else ShowNode(currentNodeIndex + 1);
    }

    private bool JumpToNode(string id)
    {
        if (!string.IsNullOrWhiteSpace(id) && nodeLookup.TryGetValue(id, out int index))
        {
            ShowNode(index);
            return IsConversationActive;
        }
        Debug.LogWarning($"Dialogue node id not found: {id}");
        EndConversation();
        return false;
    }

    public bool TryRouteCurrentNodeByIntent(string intent, GameStateProvider gameStateProvider)
    {
        DialogueNode node = CurrentNode;
        DialogueIntentRoute route = node?.intentRoutes?.Find(
            candidate => candidate != null && candidate.intent == intent);
        string destination = IsRouteAllowed(route, gameStateProvider) &&
            !string.IsNullOrWhiteSpace(route.nextNodeId) ? route.nextNodeId : node?.unknownIntentNodeId;
        return JumpToNode(destination);
    }

    private static bool IsRouteAllowed(DialogueIntentRoute route, GameStateProvider provider)
    {
        if (route == null) return false;
        if (!string.IsNullOrWhiteSpace(route.requiredFlag) &&
            (provider == null || !provider.HasFlag(route.requiredFlag))) return false;
        return string.IsNullOrWhiteSpace(route.blockedByFlag) ||
            provider == null || !provider.HasFlag(route.blockedByFlag);
    }

    private void FireTrigger(string id)
    {
        if (!string.IsNullOrWhiteSpace(id)) onDialogueTrigger?.Invoke(id);
    }

    private void ClearOptions()
    {
        if (optionsContainer == null) return;
        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            GameObject option = optionsContainer.GetChild(i).gameObject;
            option.SetActive(false);
            Destroy(option);
        }
    }

    public void EndConversation()
    {
        bool wasActive = IsConversationActive;
        state = DialogueState.Closed;
        Revision++;
        activeDialogue = null;
        currentNodeIndex = -1;
        lastSubmittedFreeText = "";
        nodeLookup.Clear();
        ClearOptions();
        freeTextInputPanel?.SetActive(false);
        SetPlayerMovement(true);
        gameObject.SetActive(false);
        if (wasActive) OnConversationEnded?.Invoke();
    }

    private void OnDisable()
    {
        if (IsConversationActive) EndConversation();
    }

    private void SetPlayerMovement(bool enabled)
    {
        if (player == null) player = FindAnyObjectByType<Player>();
        if (player != null && player.movementEnabled != enabled) player.SetMovementEnabled(enabled);
    }
}
