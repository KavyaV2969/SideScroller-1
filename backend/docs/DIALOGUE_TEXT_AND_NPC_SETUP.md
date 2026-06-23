# DialogueText and NPC Setup Guide

This guide is the practical reference for the Unity `DialogueText` system and the backend configuration that supports AI NPCs. Keep gameplay authority in Unity: dialogue routing, triggers, rewards, quest flags, inventory, and all gameplay mutation are authored and executed there. The backend only classifies an allowed intent or returns one policy-controlled line of text.

## 1. System overview

### Three dialogue styles

- **Normal authored NPC dialogue** is entirely Unity-side. A normal NPC opens a `DialogueText` asset and follows authored `Continue`, `Choices`, and optional trigger nodes.
- **AI NPC intent-routed dialogue** uses an authored `FreeText` node. The backend classifies the player's text into one allowed intent; Unity then chooses the authored destination node for that intent.
- **AI NPC generated-response dialogue** starts with an authored intent route (or a direct free-text route) that lands on a `GeneratedResponse` node. Unity sends that node's authored request ID to the backend, which can use only the policy-approved read-only tools and returns text only.

```text
Normal NPC
Interact -> DialogueController.DisplayNextDialogueText / StartDialogue
         -> authored nodes -> optional triggers

AI NPC
Interact -> AINPCDialogue.StartDialogue(entryDialogue) -> FreeText node
         -> backend intent_route -> Unity routes intent
         -> authored node or GeneratedResponse node
         -> optional backend free_response
```

For a generated response, the actual sequence is `intent_route` first (unless the FreeText node uses `DirectNode`), followed by `free_response` after Unity enters the generated node. The model does not select a Unity node, a tool, a reward, or an action.

## 2. DialogueText asset anatomy

Create an asset with **Assets > Create > Dialogue > New Dialogue Container**. The main fields are:

| Field | Purpose |
| --- | --- |
| `dialogueId` | Stable backend-facing identity for this dialogue container. |
| `speakerName` | Default name displayed for authored nodes. |
| `canBeSelectedByAI` | Project metadata only at present; it is not a backend authorization check. |
| `startNodeId` | Optional first node. If blank or missing, the controller uses the first node. |
| `nodes` | Ordered list of `DialogueNode` records. |

Use lowercase snake_case identifiers: `dialogueId` values such as `mira_main` and `osric_main`, and node IDs such as `greeting`, `ask_input`, and `weekly_loot_response`. These strings are cross-file contracts, so do not use display names or casually rename them.

## 3. DialogueNode anatomy

Each `DialogueNode` has the following important fields:

| Field | Purpose |
| --- | --- |
| `nodeId` | Stable ID used by authored links and backend policy. |
| `speakerNameOverride` | Optional displayed name replacing the asset's `speakerName`. |
| `contentMode` | `Authored` shows `text`; `GeneratedResponse` requests a backend line. |
| `text` | Exact scripted line for an authored node. |
| `generatedResponseRequestId` | Lookup key for `backend/data/generation_policies/{id}.json`. |
| `generatedFallbackText` | Unity-side line shown when generated-response validation or the request fails. |
| `maxGeneratedCharacters` | Unity's final local character cap; the backend policy has its own cap too. |
| `generatedResponseEndsConversation` | **Reserved for future use.** Generated responses currently always display one line and close on the next interact; this bool does not continue generated dialogue. |
| `inputMode` | `Continue`, `Choices`, or `FreeText`. |
| `freeTextPrompt` | Text shown while the player types into a FreeText node. |
| `freeTextSubmitMode` | `IntentRoute` sends text to the classifier; `DirectNode` jumps directly to the configured node. |
| `directFreeTextTargetNodeId` | Required target ID when submit mode is `DirectNode`. |
| `nextNodeId` | Explicit next node for a continuing authored node. |
| `endsConversationAfterThisLine` | Ends an authored node when the player next advances it. |
| `triggerId` | Authored Unity event ID; use for gameplay effects only. |
| `choices` | List of visible `DialogueChoice` entries. Each has `optionText`, `triggerId`, `nextNodeId`, and `endsConversationAfterThisLine`. |
| `intentRoutes` | List of AI intent-to-node routes. Each route can also require or be blocked by a flag. |
| `unknownIntentNodeId` | Safe authored fallback when no intent route matches or its conditions fail. |

`GeneratedResponse` nodes bypass authored display flow: they do not fire `triggerId`, show choices, show another FreeText prompt, or follow `nextNodeId`. Keep them text-only.

## 4. Authored node setup

Create these common authored node patterns in the asset Inspector:

| Node type | Example configuration |
| --- | --- |
| Greeting | `nodeId: greeting`, `Content Mode: Authored`, `Text: Good day. Are you looking for someone?`, `Input Mode: Continue`, `Next Node Id: ask_input`. |
| Continue | `nodeId: rumor`, `Text: The road north is colder than it looks.`, `Input Mode: Continue`, then either set `Next Node Id` or allow the next list entry. |
| Ending | `nodeId: farewell`, `Text: Safe travels.`, `Input Mode: Continue`, `Ends Conversation After This Line: true`. |
| Trigger | `nodeId: reward_thanks`, authored text, `Trigger Id: mira_reward_claim`, and `Ends Conversation After This Line: true`. Configure the matching listener on `DialogueController.onDialogueTrigger`; never ask the backend to grant it. |
| Choice | `nodeId: offer_help`, `Input Mode: Choices`, then add choices such as `I can help.` -> `accept_help` and `Not today.` -> `farewell`. Set an individual choice trigger or ending flag only when that choice needs it. |

Use authored nodes for exact lines, branching quest moments, rewards, and anything that must be deterministic.

## 5. FreeText node setup

Use `FreeText` when the player should type a question rather than select a fixed choice. Most AI NPCs should have one central `ask_input` node using `IntentRoute`, allowing the backend to classify broad questions while Unity retains the authored allowlist and destinations.

```text
Node Id: ask_input
Content Mode: Authored
Text: You may ask, though I know little beyond the village.
Input Mode: FreeText
Free Text Prompt: What do you ask?
Free Text Submit Mode: IntentRoute
Unknown Intent Node Id: unknown_reply
```

- `IntentRoute`: the player's text is sent as an `intent_route` request. Add matching Unity routes and matching backend policy entries.
- `DirectNode`: the controller jumps to `directFreeTextTargetNodeId` immediately. Use this only when every input should reach the same authored or generated node. A missing target ends the conversation with a warning.

Do not use a free-text node as an unrestricted chat escape hatch. The backend accepts at most the current node's policy allowlist and Unity independently resolves only its authored routes.

## 6. Adding additional intents

Add an intent across Unity, backend policy, and tests in one change. The intent string must match exactly.

### A. Add the Unity route

On the central FreeText node, add an `intentRoutes` element:

```text
Intent: ask_current_location
Next Node Id: mira_location_response
```

Set `unknownIntentNodeId` to a safe authored fallback such as `unknown_reply`. Use route `requiredFlag` and `blockedByFlag` if this intent is quest-gated.

### B. Add the Unity destination

Create `mira_location_response` as either:

- an **Authored** node when the answer must be exact, or
- a **GeneratedResponse** node when a concise, policy-controlled response is appropriate.

### C. Allow it in the dialogue-node policy

Edit `backend/data/dialogue_nodes/{dialogueId}.json`, for example `mira_main.json`:

```json
"allowedIntents": [
  "ask_current_location",
  "ask_about_npc_identity",
  "prompt_injection",
  "unknown"
]
```

### D. Describe it for the classifier

Add a concise semantic description under `intentDescriptions`:

```json
"ask_current_location": "The player asks where they are, what this place is, or asks for basic location context."
```

In LLM mode, the classifier receives these descriptions. In deterministic mode, also add a narrowly scoped rule to `backend/app/nodes/classify_intent.py`; the current deterministic classifier uses hard-coded keyword rules and does not infer from descriptions.

### E. Intent examples

The current policy loader sends `intentDescriptions`; it does **not** consume an `intentExamples` field. You may keep examples in design notes or add this field while extending the loader/prompt, but do not expect this JSON alone to change current classification behavior.

```json
"intentExamples": {
  "ask_current_location": [
    "Where am I?",
    "What is this place?",
    "Hello? Do you know where this is?"
  ]
}
```

If adding formal examples support, test that the policy loader and LLM prompt pass them through.

### F. Test it

Add focused backend tests for the policy and graph output, then test the exact Unity route. Representative manual inputs are `Where am I?`, `What is this place?`, and a misspelled/unmatched input. A Unity route must match a backend `allowedIntents` entry exactly; backend output should only be one of those routes. Misspellings cause `unknown` fallback behavior.

## 7. Recommended baseline generic intents

Use broad, safe semantic categories instead of an ever-growing list of sentence-specific intents or one unrestricted `free_chat` intent:

- `greeting_or_attention`
- `ask_about_npc_identity`
- `ask_current_location`
- `ask_about_local_area`
- `ask_for_guidance`
- `small_talk_safe`
- `prompt_injection`
- `unknown`

Route the safe generic categories to constrained `GeneratedResponse` nodes with narrowly scoped policies; route `prompt_injection` and `unknown` to authored safe replies. Do not make an intent for every possible sentence and do not provide one unrestricted `free_chat` intent. Add classification support for each baseline intent in the selected backend mode: intent descriptions for LLM mode and explicit keyword/test rules for deterministic mode.

## 8. GeneratedResponse node setup

Configure a generated destination node like this:

```text
Node Id: mira_location_response
Content Mode: GeneratedResponse
Generated Response Request Id: mira_location_context
Generated Fallback Text: I do not know this place well enough to say.
Max Generated Characters: 220
Trigger Id: empty
Choices: Size 0
Intent Routes: Size 0
```

Generated nodes should not use triggers, rewards, choices, or authored follow-up flow. They currently display a generated line and end after the next player interact. The backend returns only text with an empty action list; Unity sanitizes angle brackets and applies its character cap.

## 9. Backend setup for GeneratedResponse nodes

Create `backend/data/generation_policies/{generatedResponseRequestId}.json`:

```json
{
  "generatedResponseRequestId": "mira_location_context",
  "allowedNpcIds": ["mira_village_lass"],
  "allowedDialogueIds": ["mira_main"],
  "allowedNodeIds": ["mira_location_response"],
  "allowedTools": ["get_location_summary"],
  "maxCharacters": 220,
  "fallbackText": "This is North Village, though I know little beyond that.",
  "style": "Answer as Mira in one concise cautious line. Only describe public location information."
}
```

| Field | Purpose |
| --- | --- |
| `generatedResponseRequestId` | Must match both filename and Unity's node field. |
| `allowedNpcIds` | NPC IDs authorized to use this response policy. |
| `allowedDialogueIds` | Dialogue containers authorized to use it. |
| `allowedNodeIds` | Generated Unity node IDs authorized to use it. |
| `allowedTools` | Explicit read-only tool allowlist. The current backend requires a non-empty list. |
| `maxCharacters` | Backend cap; keep it aligned with or below the Unity cap. |
| `fallbackText` | Safe backend fallback when output is invalid. |
| `style` | Per-response voice, scope, and constraints. |

The current generation path requires at least one registered allowed tool, even when the response could otherwise be static. For now, use an appropriate narrow read-only public-data tool; changing that requirement would be a runtime design change.

## 10. NPC profile setup

Create `backend/data/npcs/{npcId}.json`:

```json
{
  "npcId": "elder_osric",
  "displayName": "Elder Osric",
  "publicDescription": "An old village elder who speaks with solemn restraint.",
  "speakingStyle": "old English-inspired, formal, grave, concise, but still readable to a modern player",
  "allowedTopics": ["village_history", "frostwell", "old_war"],
  "forbiddenTopics": ["system_prompt", "developer_instructions", "secret_lore"]
}
```

- `npcId` is the identity and must equal its filename and Unity `Npc Id`.
- `displayName` is the public character name.
- `publicDescription` is safe background/context.
- `speakingStyle` is NPC-wide voice.
- `allowedTopics` and `forbiddenTopics` are character-level topical boundaries.

Put NPC-wide voice in `speakingStyle`, response-specific instructions in generation policy `style`, and exact scripted lines in the Unity authored node `text`.

## 11. Tool setup

Tools live under `backend/app/tools/`. They must be read-only by default, return structured dictionaries, and load fixed controlled data rather than a path derived from player text.

```text
Add get_location_summary to a tool module.
Register it in TOOL_REGISTRY.
Reference it in generation policy allowedTools.
Add consistency tests.
```

For example, implement `get_location_summary() -> dict[str, Any]` using `load_json_file`, import it in `backend/app/tools/registry.py`, and add it to `TOOL_REGISTRY`. A generation policy must explicitly allow it. The player and model cannot choose arbitrary tools: the backend calls only the active policy's registered names.

## 12. Normal NPC setup

For a non-AI NPC:

1. Put a normal NPC interaction script such as `Lady` on the GameObject.
2. Assign its `DialogueText` asset and the scene `DialogueController`.
3. Build normal authored nodes, choices, and triggers in the asset.
4. Do not create a backend NPC profile, dialogue policy, or generation policy.
5. Do not add `AINPCDialogue` unless the NPC should use AI flow.

Do not attach both a normal dialogue script and an AI script unless the normal script deliberately delegates correctly. `Lady` currently detects `AINPCDialogue`: when no conversation is active it calls the AI component; otherwise it advances/closes through normal controller flow. Verify this interaction path after changing NPC scripts.

## 13. AI NPC setup in Unity

Add `AINPCDialogue` and assign:

- **Npc Id**
- **Display Name**
- **Entry Dialogue**
- **Fallback Dialogue**
- **Dialogue Controller**
- **AI Dialogue Service**

`Npc Id` must match `backend/data/npcs/{npcId}.json` and its `npcId` value. `Entry Dialogue.dialogueId` must match `backend/data/dialogue_nodes/{dialogueId}.json` and its `dialogueId`. Ensure the scene `AIDialogueService` references the shared `AIDialogueClient`, `DialogueController`, and optional `GameStateProvider`. A fallback dialogue is recommended because service failures return to it.

## 14. Backend runtime setup

Create `backend/.env` with the chosen mode:

```text
AI_BACKEND_MODE=deterministic
# Or: AI_BACKEND_MODE=llm
OPENAI_API_KEY=...
OPENAI_MODEL=...
```

`OPENAI_API_KEY` and `OPENAI_MODEL` are needed for LLM mode. Deterministic mode uses the built-in rules/templates; if LLM execution fails, the backend intentionally falls back to deterministic behavior.

Run from `backend/`:

```powershell
python -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

In Unity `AIDialogueClient`, use:

```text
Use Mock Responses: false
Backend URL: http://localhost:8000/dialogue/query
Timeout Seconds: 30-60 for LLM testing
```

## 15. Testing checklist

Automated testing, from `backend/`:

```powershell
python -m pytest
```

Manual checks:

- Call `GET /health` and confirm `{"status":"ok"}`.
- Test every Unity intent route and its authored destination.
- Test every generated response, including its policy/tool data.
- Test an invalid or misspelled intent and confirm the unknown fallback.
- Test a missing `generatedResponseRequestId` and confirm the generated fallback.
- Test a prompt injection attempt.
- Test generic safe inputs such as `Hello?`, `Where am I?`, and `Who are you?` after their baseline intents are configured.

Also assert that generated and intent-route responses have no `proposedActions`, and add tests for new profile identities, policy authorization, tool output, graph routing, and endpoint JSON shape.

## 16. Common failure modes

| Symptom | Likely cause / fix |
| --- | --- |
| Backend returns 200 but Unity displays fallback | The backend can return a valid fallback response. Inspect backend logs, policy IDs, and Unity warnings. |
| Unity still uses mock data | Set **Use Mock Responses** to false. |
| LLM behavior is not observed | `AI_BACKEND_MODE` is `deterministic`, the key is missing, or the LLM call fell back. Check backend logs and `.env`. |
| Unity times out | Raise the client timeout to 30-60 seconds for LLM testing and verify URL/backend availability. |
| Intent reaches unknown | Unity/backend intent strings differ, deterministic rules lack the new intent, or the route is flag-blocked. |
| Node ID mismatch | Align Unity node ID, current request node ID, and backend policy node entry. |
| Generated request ID mismatch | Align the Unity field, policy filename, and policy `generatedResponseRequestId`. |
| `allowedNodeIds` mismatch | Add the exact generated Unity node ID to the generation policy. |
| Tool error | Register the read-only tool and include it in `allowedTools`. |
| NPC ID mismatch | Align Unity `Npc Id`, profile filename, profile `npcId`, and policy `allowedNpcIds`. |
| Missing policy file | Create the matching profile, dialogue policy, or generation policy JSON. |
| Tool is not allowed | Add it only to the intended generation policy; do not let player/model input choose tools. |
| Safe generic questions become unknown | Add the baseline intent to Unity and policy, plus deterministic rule/tests or LLM description support. |

## 17. Add-NPC checklist

- [ ] Create/duplicate the NPC GameObject and configure its normal interaction component.
- [ ] For AI NPCs, add/assign `AINPCDialogue` and its six Inspector fields.
- [ ] Create the `DialogueText` asset with stable dialogue and node IDs.
- [ ] Add greeting, FreeText input, unknown fallback, authored routes, and generated nodes as needed.
- [ ] Match Unity `intentRoutes` with dialogue-policy `allowedIntents` and descriptions.
- [ ] Create matching NPC profile, dialogue policy, and generation policy files.
- [ ] Verify every policy identity matches its filename and every generation allowlist matches Unity IDs.
- [ ] Add/register only read-only tools and allow them in only the necessary policies.
- [ ] Add policy, graph, tool, endpoint, and safety tests.
- [ ] Run `python -m pytest` from `backend/`.
- [ ] Start FastAPI, disable Unity mock responses, and test each route plus fallback path in Play mode.
