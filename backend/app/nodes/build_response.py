from app.schemas import (
    build_blocked_response,
    build_fallback_response,
    build_generated_response,
    build_intent_route_response,
)
from app.state import DialogueGraphState


def build_intent_route_response_node(state: DialogueGraphState) -> DialogueGraphState:
    """Build Unity's action-free intent-route response DTO."""

    response = build_intent_route_response(
        _npc_id(state),
        _string_value(state.get("classified_intent"), "unknown"),
    )
    return {"response": response}


def build_generated_response_node(state: DialogueGraphState) -> DialogueGraphState:
    """Build Unity's action-free generated-response DTO."""

    response = build_generated_response(
        _npc_id(state),
        _string_value(state.get("generated_text"), ""),
    )
    return {"response": response}


def build_fallback_response_node(state: DialogueGraphState) -> DialogueGraphState:
    """Build Unity's action-free fallback response DTO."""

    reason = _string_value(state.get("fallback_reason"), "")
    response = (
        build_blocked_response(_npc_id(state), reason)
        if state.get("safety_blocked") is True
        else build_fallback_response(_npc_id(state), reason)
    )
    return {"response": response}


def _npc_id(state: DialogueGraphState) -> str:
    request = state.get("request")
    return request.npcId if request is not None else ""


def _string_value(value: object, default: str) -> str:
    return value if isinstance(value, str) else default
