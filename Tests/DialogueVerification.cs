// Run with SampleScene in Play Mode:
// unity command run_script --file Tests/DialogueVerification.cs --entry DialogueVerification.Run
// HTTP checks require: python -m uvicorn unity_fixture:app --app-dir tests --port 18001 (from backend/)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class DialogueVerification
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<string> Report = new List<string>();
    private static T Get<T>(object owner, string field) => (T)owner.GetType().GetField(field, Fields).GetValue(owner);
    private static void Set(object owner, string field, object value) => owner.GetType().GetField(field, Fields).SetValue(owner, value);
    private static void Check(bool condition, string description)
    {
        Report.Add((condition ? "PASS " : "FAIL ") + description);
        System.IO.File.WriteAllLines("Temp/DialogueVerification.txt", Report);
        if (!condition) throw new Exception(description);
    }
    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(8);
        while (!condition() && DateTime.UtcNow < deadline) await Task.Delay(20);
        if (!condition()) throw new Exception("Timed out waiting for dialogue response.");
    }

    public static async Task<string> Run()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        Report.Clear();
        var controller = UnityEngine.Object.FindAnyObjectByType<DialogueController>(FindObjectsInactive.Include);
        var client = UnityEngine.Object.FindAnyObjectByType<AIDialogueClient>();
        var service = UnityEngine.Object.FindAnyObjectByType<AIDialogueService>();
        var npc = UnityEngine.Object.FindAnyObjectByType<AINPCDialogue>();
        var player = UnityEngine.Object.FindAnyObjectByType<Player>();
        var provider = UnityEngine.Object.FindAnyObjectByType<GameStateProvider>();
        var enemy = UnityEngine.Object.FindAnyObjectByType<Enemy>();
        var line = Get<TextMeshProUGUI>(controller, "NPCDialogueText");
        var name = Get<TextMeshProUGUI>(controller, "NPCNameText");
        var input = Get<TMP_InputField>(controller, "playerInputField");
        var submit = Get<Button>(controller, "submitTextButton");
        var options = Get<Transform>(controller, "optionsContainer");
        var originalEntry = npc.EntryDialogue;
        var originalFallback = npc.FallbackDialogue;
        var originalMock = Get<bool>(client, "useMockResponses");
        var originalDelay = Get<float>(client, "mockDelaySeconds");
        var originalUrl = Get<string>(client, "backendUrl");
        var originalFlags = Get<string[]>(provider, "activeQuestFlags");
        var originalCompleted = Get<string[]>(provider, "completedQuestFlags");
        var originalTriggers = controller.onDialogueTrigger;
        bool simulated = player.rb.simulated;
        bool background = Application.runInBackground;
        var dialogue = ScriptableObject.CreateInstance<DialogueText>();
        var fallback = ScriptableObject.CreateInstance<DialogueText>();
        var fired = new List<string>();
        int ended = 0;
        Action onEnded = () => ended++;
        Action<string> onTrigger = id => fired.Add(id);
        void Submit(string text) { input.text = text; submit.onClick.Invoke(); }
        void Advance() => controller.DisplayNextDialogueText(dialogue);
        void Begin(string start) => controller.StartDialogue(dialogue, start);
        async Task BeginAI(string start)
        {
            controller.EndConversation();
            dialogue.startNodeId = start;
            npc.Interact();
            await Task.Delay(20);
        }
        try
        {
            Application.runInBackground = true;
            player.rb.simulated = false;
            if (enemy != null) enemy.gameObject.SetActive(false);
            Set(client, "useMockResponses", true);
            Set(client, "mockDelaySeconds", 0.06f);
            Set(provider, "activeQuestFlags", new string[0]);
            Set(provider, "completedQuestFlags", new string[0]);
            controller.onDialogueTrigger = new UnityEvent<string>();
            controller.onDialogueTrigger.AddListener(id => onTrigger(id));
            controller.OnConversationEnded += onEnded;

            dialogue.dialogueId = "mira_main";
            dialogue.speakerName = "Mira";
            dialogue.nodes = new List<DialogueNode>
            {
                new DialogueNode { nodeId = "greeting", text = "Hello", triggerId = "greet", nextNodeId = "choices" },
                new DialogueNode { nodeId = "choices", text = "Choose", inputMode = DialogueNodeInputMode.Choices,
                    choices = new List<DialogueChoice> {
                        new DialogueChoice { optionText = "Dungeon", triggerId = "choose", nextNodeId = "dungeon" },
                        new DialogueChoice { optionText = "Bye", endsConversationAfterThisLine = true }
                    } },
                new DialogueNode { nodeId = "dungeon", text = "Frostwell", speakerNameOverride = "Guide", endsConversationAfterThisLine = true },
                new DialogueNode { nodeId = "ask_input", text = "Ask", inputMode = DialogueNodeInputMode.FreeText,
                    freeTextPrompt = "Question?", unknownIntentNodeId = "unknown",
                    intentRoutes = new List<DialogueIntentRoute> {
                        new DialogueIntentRoute { intent = "ask_about_dungeon", nextNodeId = "dungeon" },
                        new DialogueIntentRoute { intent = "inquiry_of_weekly_loot", nextNodeId = "weekly_loot_response" },
                        new DialogueIntentRoute { intent = "mention_brother_helped", nextNodeId = "reward", requiredFlag = "helped_tomas", blockedByFlag = "claimed" },
                        new DialogueIntentRoute { intent = "prompt_injection", nextNodeId = "unknown" }
                    } },
                new DialogueNode { nodeId = "unknown", text = "Unknown.", endsConversationAfterThisLine = true },
                new DialogueNode { nodeId = "reward", text = "Thank you.", triggerId = "reward", endsConversationAfterThisLine = true },
                new DialogueNode { nodeId = "weekly_loot_response", contentMode = DialogueNodeContentMode.GeneratedResponse,
                    generatedResponseRequestId = "weekly_loot", generatedFallbackText = "No loot news.", maxGeneratedCharacters = 60,
                    triggerId = "must_not_fire", nextNodeId = "reward" },
                new DialogueNode { nodeId = "direct", inputMode = DialogueNodeInputMode.FreeText,
                    freeTextSubmitMode = FreeTextSubmitMode.DirectNode, directFreeTextTargetNodeId = "dungeon" },
                new DialogueNode { nodeId = "sequential", text = "One" },
                new DialogueNode { nodeId = "last", text = "Two" }
            };
            fallback.nodes.Add(new DialogueNode { nodeId = "fallback", text = "Authored fallback.", endsConversationAfterThisLine = true });
            Set(npc, "entryDialogue", dialogue);
            Set(npc, "fallbackDialogue", fallback);

            Begin("greeting");
            Check(line.text == "Hello" && !player.movementEnabled && fired.SequenceEqual(new[] { "greet" }), "authored line, trigger, and movement lock");
            Advance();
            Check(controller.CurrentNode.nodeId == "choices", "explicit next-node route");
            Advance();
            Check(controller.CurrentNode.nodeId == "choices", "interact cannot skip a choice");
            options.GetComponentsInChildren<Button>().First().onClick.Invoke();
            Check(line.text == "Frostwell" && name.text == "Guide" && fired.Last() == "choose", "choice label, trigger, route, and speaker override");
            Advance();
            Check(!controller.IsConversationActive && player.movementEnabled && ended == 1, "authored ending restores movement and emits one end event");
            Begin("choices");
            options.GetComponentsInChildren<Button>().Last().onClick.Invoke();
            Check(!controller.IsConversationActive, "choice can end conversation");
            Begin("sequential");
            Advance();
            Check(line.text == "Two", "implicit sequential routing");
            Advance();
            Check(!controller.IsConversationActive, "last node ends conversation");
            dialogue.startNodeId = "dungeon";
            Begin("missing_override");
            Check(line.text == "Frostwell", "invalid override falls back to asset start");
            dialogue.startNodeId = "missing_start";
            Begin(null);
            Check(line.text == "Hello", "missing authored start falls back to first node");

            Begin("ask_input");
            Submit("   ");
            Advance();
            Check(controller.CurrentNode.nodeId == "ask_input" && line.text == "Question?", "empty input ignored and interact cannot skip free text");
            Begin("direct");
            Submit("Anything");
            Check(line.text == "Frostwell", "DirectNode routes without backend");

            Begin("ask_input");
            controller.TryRouteCurrentNodeByIntent("mention_brother_helped", provider);
            Check(line.text == "Unknown.", "required flag gates route");
            Set(provider, "completedQuestFlags", new[] { "helped_tomas" });
            Begin("ask_input");
            controller.TryRouteCurrentNodeByIntent("mention_brother_helped", provider);
            Check(line.text == "Thank you." && fired.Last() == "reward", "completed flag unlocks authored reward trigger");
            Set(provider, "activeQuestFlags", new[] { "claimed" });
            Begin("ask_input");
            controller.TryRouteCurrentNodeByIntent("mention_brother_helped", provider);
            Check(line.text == "Unknown.", "blocked flag takes unknown route");
            Begin("ask_input");
            Check(controller.TryRouteCurrentNodeByIntent("not_an_intent", provider) && line.text == "Unknown.", "unknown intent fallback");

            await BeginAI("ask_input");
            Submit("dungeon");
            int pending = controller.Revision;
            Submit("loot");
            Advance();
            Check(controller.Revision == pending && line.text == "...", "pending blocks duplicate submission and interact");
            await WaitFor(() => line.text == "Frostwell");
            Check(line.text == "Frostwell", "mock intent request routes through NPC subscriptions");
            await BeginAI("ask_input");
            Submit("loot");
            await WaitFor(() => controller.ActiveDialogue == null);
            Check(line.text.Contains("Moonsteel Ore") && line.text.Length == 60 && !fired.Contains("must_not_fire"), "mock intent to generated reply, cap, and no generated trigger");
            Advance();
            Check(!controller.IsConversationActive, "generated line closes instead of executing authored next node");

            Set(client, "mockDelaySeconds", 0.15f);
            await BeginAI("ask_input");
            Submit("loot");
            controller.EndConversation();
            await Task.Delay(220);
            Check(!controller.IsConversationActive && player.movementEnabled, "late response cannot reopen closed dialogue");
            await BeginAI("ask_input");
            Submit("loot");
            Begin("ask_input");
            await Task.Delay(220);
            Check(controller.CurrentNode.nodeId == "ask_input" && line.text == "Question?", "late response cannot overwrite same-node restart");
            await BeginAI("ask_input");
            Submit("loot");
            npc.enabled = false;
            npc.enabled = true;
            await Task.Delay(220);
            Check(controller.ActiveDialogue == dialogue && line.text == "...", "disabled NPC invalidates pending request across re-enable");
            controller.EndConversation();
            int endsBefore = ended;
            Begin("greeting");
            controller.gameObject.SetActive(false);
            Check(!controller.IsConversationActive && player.movementEnabled && ended == endsBefore + 1, "external UI disable closes once and unlocks player");

            UnityAction<string> replace = id => { if (id == "greet") Begin("dungeon"); };
            controller.onDialogueTrigger.AddListener(replace);
            Begin("greeting");
            Check(line.text == "Frostwell" && controller.CurrentNode.nodeId == "dungeon", "reentrant trigger cannot be overwritten by old node rendering");
            controller.onDialogueTrigger.RemoveListener(replace);

            // Cross-language HTTP contract, using the actual FastAPI/LangGraph router.
            Set(client, "useMockResponses", false);
            Set(client, "backendUrl", "http://127.0.0.1:18001/dialogue/query");
            await BeginAI("ask_input");
            Submit("dungeon");
            await WaitFor(() => line.text != "...");
            Check(line.text == "Frostwell", "Unity HTTP intent request through real LangGraph");
            await BeginAI("ask_input");
            Submit("weekly loot");
            await WaitFor(() => controller.ActiveDialogue == null);
            Check(line.text.Contains("Moonsteel Ore"), "Unity HTTP intent-to-generation round trip");

            foreach (string invalid in new[] { "wrong_npc", "blocked", "actions", "wrong_type", "empty", "malformed", "error" })
            {
                Set(client, "backendUrl", "http://127.0.0.1:18001/fixture/" + invalid);
                await BeginAI("weekly_loot_response");
                await WaitFor(() => line.text != "...");
                Check(line.text == "No loot news.", "generated fallback rejects " + invalid);
            }
            Set(client, "backendUrl", "http://127.0.0.1:18001/fixture/valid");
            await BeginAI("weekly_loot_response");
            await WaitFor(() => line.text != "...");
            Check(line.text.Length <= 60 && !line.text.Contains("<") && !line.text.Contains(">"), "HTTP generated text is sanitized and capped");
            Set(client, "backendUrl", "http://127.0.0.1:18001/fixture/intent_actions");
            await BeginAI("ask_input");
            Submit("dungeon");
            await WaitFor(() => line.text != "...");
            Check(line.text == "Authored fallback.", "intent with proposed actions uses authored fallback");
            Set(npc, "fallbackDialogue", null);
            await BeginAI("ask_input");
            Submit("dungeon");
            await WaitFor(() => line.text != "...");
            Check(line.text == "I do not know what to say to that.", "missing fallback asset uses standalone line");

            Set(service, "client", null);
            await BeginAI("weekly_loot_response");
            Check(line.text == "No loot news.", "missing client uses generated fallback");
            controller.DisplayGeneratedResponseLine("Mira", " <b>safe</b> ");
            Check(line.text == "bsafe/b", "standalone generated display preserves text sanitization");
            Advance();
            Check(!controller.IsConversationActive, "standalone generated line closes on interact");
            Report.Add("COMPLETE");
            return string.Join("\n", Report);
        }
        finally
        {
            controller.EndConversation();
            controller.OnConversationEnded -= onEnded;
            controller.onDialogueTrigger = originalTriggers;
            Set(npc, "entryDialogue", originalEntry);
            Set(npc, "fallbackDialogue", originalFallback);
            Set(client, "useMockResponses", originalMock);
            Set(client, "mockDelaySeconds", originalDelay);
            Set(client, "backendUrl", originalUrl);
            Set(service, "client", client);
            Set(provider, "activeQuestFlags", originalFlags);
            Set(provider, "completedQuestFlags", originalCompleted);
            player.rb.simulated = simulated;
            if (enemy != null) enemy.gameObject.SetActive(true);
            Application.runInBackground = background;
            UnityEngine.Object.Destroy(dialogue);
            UnityEngine.Object.Destroy(fallback);
            System.IO.File.WriteAllLines("Temp/DialogueVerification.txt", Report);
        }
    }
}
