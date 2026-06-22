from app.schemas import REQUEST_MODE_FREE_RESPONSE, REQUEST_MODE_INTENT_ROUTE
from app.state import DialogueGraphState


MAX_PLAYER_INPUT_CHARACTERS = 500


def validate_request(state: DialogueGraphState) -> DialogueGraphState:
    """Validate the narrow Unity request contract before loading any data."""

    request = state.get("request")
    if request is None:
        return _fallback(state, "Missing request.")

    if not request.npcId:
        return _fallback(state, "Missing NPC id.")

    if request.requestedMode not in {
        REQUEST_MODE_INTENT_ROUTE,
        REQUEST_MODE_FREE_RESPONSE,
    }:
        return _fallback(state, "Unsupported requested mode.")

    if not request.currentDialogueId:
        return _fallback(state, "Missing dialogue id.")

    if not request.currentNodeId:
        return _fallback(state, "Missing node id.")

    if len(request.playerInput) > MAX_PLAYER_INPUT_CHARACTERS:
        return _fallback(state, "Player input is too long.")

    if (
        request.requestedMode == REQUEST_MODE_FREE_RESPONSE
        and not request.generatedResponseRequestId
    ):
        return _fallback(state, "Missing generated response request id.")

    return {
        "requested_mode": request.requestedMode,
        "graph_route": request.requestedMode,
        "errors": list(state.get("errors", [])),
    }


def _fallback(state: DialogueGraphState, reason: str) -> DialogueGraphState:
    return {
        "graph_route": "fallback",
        "fallback_reason": reason,
        "errors": _append_error(state, reason),
    }


def _append_error(state: DialogueGraphState, message: str) -> list[str]:
    return [*state.get("errors", []), message]
