from app.schemas import REQUEST_MODE_FREE_RESPONSE, REQUEST_MODE_INTENT_ROUTE
from app.state import DialogueGraphState, fallback


MAX_PLAYER_INPUT_CHARACTERS = 500


def validate_request(state: DialogueGraphState) -> DialogueGraphState:
    """Validate the narrow Unity request contract before loading any data."""

    request = state.get("request")
    if request is None:
        return fallback(state, "Missing request.")

    if not request.npcId:
        return fallback(state, "Missing NPC id.")

    if request.requestedMode not in {
        REQUEST_MODE_INTENT_ROUTE,
        REQUEST_MODE_FREE_RESPONSE,
    }:
        return fallback(state, "Unsupported requested mode.")

    if not request.currentDialogueId:
        return fallback(state, "Missing dialogue id.")

    if not request.currentNodeId:
        return fallback(state, "Missing node id.")

    if len(request.playerInput) > MAX_PLAYER_INPUT_CHARACTERS:
        return fallback(state, "Player input is too long.")

    if (
        request.requestedMode == REQUEST_MODE_FREE_RESPONSE
        and not request.generatedResponseRequestId
    ):
        return fallback(state, "Missing generated response request id.")

    return {
        "requested_mode": request.requestedMode,
        "graph_route": request.requestedMode,
        "errors": list(state.get("errors", [])),
    }
