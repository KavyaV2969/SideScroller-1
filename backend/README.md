# AI dialogue

Unity owns the conversation, authored routes, choices, triggers, and gameplay changes.
The backend returns either an allowed intent or a bounded generated line. Both response
types remain action-free.

## Implementation

- `DialogueController` has one state: closed, continue, choices, free text, or pending.
  Display paths share UI cleanup and player movement locking. Each display has a revision
  so an asynchronous response cannot overwrite a newer conversation.
- `AINPCDialogue` connects the active NPC to controller events.
- `AIDialogueService` builds and validates both request types in one path.
  `AIDialogueClient` supplies either mock responses or HTTP transport.
- `app/graph.py` defines the shared context path and the intent/generation pipelines.
  Individual LangGraph stages remain visible when inspecting or streaming the graph.
  Every stage can short-circuit to the shared fallback builder.
- Policy/data loading, allowlists, read-only tools, deterministic fallback, optional
  structured model output, and generated text validation keep their existing roles.

The Unity serialized field names, script identities, authored assets, and HTTP JSON
contract are unchanged. Generated replies still display one line and end on the next
interaction; `generatedResponseEndsConversation` remains reserved.

Legacy `DialogueActionValidator`, `DialogueActionExecutor`, and
`DialogueResponseDatabase` remain compatible with existing assets and callers.
The active AI path does not execute model-proposed gameplay actions. The executor still
only logs actions, and `GameStateProvider` still reads inspector test data; wiring actual
quest/inventory changes is a separate feature.

## Running and authoring

Use Python 3.11 or 3.12, as specified in `pyproject.toml`.
See [starting the backend](docs/STARTING_BACKEND.md),
[adding NPCs](docs/ADDING_AI_NPCS.md), and the
[dialogue authoring guide](docs/DIALOGUE_TEXT_AND_NPC_SETUP.md).

## Verification

From `backend/`:

```powershell
python -B -m pytest -p no:cacheprovider
```

Tests ignore the developer's `.env`, run deterministically, and mock model calls.
They do not use a paid API. Coverage includes the JSON contract, both graph paths,
policy restrictions, invalid inputs, output filtering, model/tool failures, and
fallback routing after validation.

For Unity integration tests, start the local fixture from `backend/`:

```powershell
$env:AI_BACKEND_MODE = 'deterministic'
python -B -m uvicorn unity_fixture:app --app-dir tests --host 127.0.0.1 --port 18001
```

With SampleScene open, run from the project root:

```powershell
unity command editor_play
unity command run_script --file Tests/DialogueVerification.cs --entry DialogueVerification.Run
unity command editor_stop
```

Exit Play Mode even if a check fails. The verifier uses runtime fixtures and writes
`Temp/DialogueVerification.txt`; it does not save scene or dialogue asset changes.
It exercises authored flow, choices/triggers, free text, flag conditions, generated
responses, stale replies, and actual Unity-to-FastAPI/LangGraph HTTP requests.
The fixture's `/fixture/*` routes are test-only and are not registered by `app.main`.
