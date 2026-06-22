from app.state import DialogueGraphState


def route_by_requested_mode(state: DialogueGraphState) -> str:
    """Return the conditional-edge route selected by request validation."""

    if state.get("graph_route") == "intent_route":
        return "intent_route"

    if state.get("graph_route") == "free_response":
        return "free_response"

    return "fallback"
