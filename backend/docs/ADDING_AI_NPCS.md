# Adding AI NPCs

Use this as the short, repeatable checklist for adding an AI NPC. For the complete Unity `DialogueText`, normal-NPC, AI-NPC, intent, generated-response, backend-policy, and tool reference, see [DialogueText and NPC Setup Guide](DIALOGUE_TEXT_AND_NPC_SETUP.md).

## Architecture and authority

```text
Unity authored DialogueText
  -> FreeText / intent_route
  -> backend returns one allowed intent
  -> Unity authored intent route
  -> authored node or GeneratedResponse node
  -> optional free_response returns one text line
```

Unity owns node placement, intent routes, choices, triggers, rewards, quest flags, inventory, and all gameplay mutation. The backend owns validation, intent classification, approved read-only tool calls, and policy-controlled generated text. The model never chooses routes/tools or produces gameplay actions; responses use `proposedActions: []`.

`generatedResponseEndsConversation` is reserved for future use. Generated responses currently display one line, then close on the next interact.

## Add an AI NPC

1. Choose lowercase snake_case IDs, for example `elder_osric`, `osric_main`, `ask_input`, `village_history_response`, `elder_village_history`, and `ask_about_village_history`.
2. Add `AINPCDialogue` to the NPC GameObject and assign **Npc Id**, **Display Name**, **Entry Dialogue**, optional **Fallback Dialogue**, **Dialogue Controller**, and **AI Dialogue Service**. Its interaction script must call `AINPCDialogue.Interact()`; the existing `Lady` component already delegates to it.
3. Create an authored `DialogueText` asset. Add a greeting and a central `ask_input` node using `FreeText` + `IntentRoute`, then add `unknownIntentNodeId` and Unity intent routes.
4. Add authored destination nodes for deterministic/triggered outcomes, and `GeneratedResponse` nodes for approved generated text. Generated nodes must be text-only: no triggers, rewards, choices, or authored follow-up.
5. Create `backend/data/npcs/{npcId}.json`, `backend/data/dialogue_nodes/{dialogueId}.json`, and one generation policy for each generated request ID. Each filename must match the corresponding JSON identity field.
6. Keep every Unity route intent exactly equal to that node policy's `allowedIntents` entry. Add an `intentDescriptions` entry and, in deterministic mode, update the explicit classifier rule/tests for the new intent.
7. If a generated answer needs data, add a fixed, read-only tool under `backend/app/tools/`, register it in `TOOL_REGISTRY`, and allow it only in the applicable generation policy.
8. Run `python -m pytest` from `backend/`, start FastAPI, turn off Unity mock responses, and exercise every intended route plus unknown and generated fallbacks.

## Recommended generic intent baseline

Do not make an intent for every possible sentence, and do not add unrestricted `free_chat`. Start with broad, safe intents such as `greeting_or_attention`, `ask_about_npc_identity`, `ask_current_location`, `ask_about_local_area`, `ask_for_guidance`, `small_talk_safe`, `prompt_injection`, and `unknown`. Route safe generic intents to constrained generated nodes; route injection and unknown to authored fallbacks. Configure each intent in Unity, policy, and the active classifier mode.

## Quick diagnosis

- **Unknown route:** intent spelling, route conditions, and dialogue-policy allowlist must match exactly.
- **Generated fallback:** check request ID, policy allowlists, registered/allowed tool, NPC ID, backend URL, and client mode.
- **Mock response:** set **Use Mock Responses** to false.
- **LLM behaves deterministically:** check `AI_BACKEND_MODE`, API key, model configuration, and backend logs.

For field-by-field examples, policy JSON, normal NPC setup, DirectNode behavior, testing coverage, and the full failure-mode table, use the [DialogueText and NPC Setup Guide](DIALOGUE_TEXT_AND_NPC_SETUP.md).
