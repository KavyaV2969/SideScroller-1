from app.state import DialogueGraphState
from app.tools.game_data_tools import (
    get_dialogue_node_policy,
    get_generation_policy,
    get_npc_profile,
)


def load_npc_context(state: DialogueGraphState) -> DialogueGraphState:
    """Load the requested NPC's approved public profile."""

    request = state.get("request")
    if request is None:
        return _fallback(state, "Missing request while loading NPC context.")

    try:
        return {"npc_profile": get_npc_profile(request.npcId)}
    except (FileNotFoundError, KeyError, OSError, ValueError) as error:
        return _fallback(state, f"Unable to load NPC context: {error}")


def load_intent_policy(state: DialogueGraphState) -> DialogueGraphState:
    """Load the authored allowlist used by the intent-route path."""

    request = state.get("request")
    if request is None:
        return _fallback(state, "Missing request while loading intent policy.")

    try:
        node_policy = get_dialogue_node_policy(
            request.currentDialogueId,
            request.currentNodeId,
        )
    except (FileNotFoundError, KeyError, OSError, ValueError) as error:
        return _fallback(state, f"Unable to load intent policy: {error}")

    allowed_intents = _string_list(node_policy.get("allowedIntents"))
    if not allowed_intents:
        return _fallback(state, "Intent policy has no allowed intents.")

    default_intent = node_policy.get("defaultIntent", "unknown")
    if not isinstance(default_intent, str):
        default_intent = "unknown"

    return {
        "node_policy": node_policy,
        "allowed_intents": allowed_intents,
        "default_intent": default_intent,
        "intent_descriptions": _string_dict(node_policy.get("intentDescriptions")),
    }


def load_generation_policy(state: DialogueGraphState) -> DialogueGraphState:
    """Load and authorize the authored policy for a generated-response node."""

    request = state.get("request")
    if request is None:
        return _fallback(state, "Missing request while loading generation policy.")

    try:
        policy = get_generation_policy(request.generatedResponseRequestId)
    except (FileNotFoundError, KeyError, OSError, ValueError) as error:
        return _fallback(state, f"Unable to load generation policy: {error}")

    if request.npcId not in _string_list(policy.get("allowedNpcIds")):
        return _fallback(state, "NPC is not allowed by the generation policy.")

    if request.currentDialogueId not in _string_list(policy.get("allowedDialogueIds")):
        return _fallback(state, "Dialogue is not allowed by the generation policy.")

    if request.currentNodeId not in _string_list(policy.get("allowedNodeIds")):
        return _fallback(state, "Node is not allowed by the generation policy.")

    allowed_tools = _string_list(policy.get("allowedTools"))
    if not allowed_tools:
        return _fallback(state, "Generation policy has no allowed tools.")

    return {
        "generation_policy": policy,
        "allowed_tools": allowed_tools,
    }


def _fallback(state: DialogueGraphState, reason: str) -> DialogueGraphState:
    return {
        "graph_route": "fallback",
        "fallback_reason": reason,
        "errors": [*state.get("errors", []), reason],
    }


def _string_list(value: object) -> list[str]:
    if not isinstance(value, list):
        return []

    return [item for item in value if isinstance(item, str) and item]


def _string_dict(value: object) -> dict[str, str]:
    if not isinstance(value, dict):
        return {}

    return {
        key: item
        for key, item in value.items()
        if isinstance(key, str) and isinstance(item, str)
    }
