from app.state import DialogueGraphState


def classify_intent(state: DialogueGraphState) -> DialogueGraphState:
    """Classify text with a deterministic, authored-allowlist keyword policy."""

    request = state.get("request")
    player_input = request.playerInput.lower() if request is not None else ""
    candidate_intent, matched_rule = _classify_keywords(player_input)

    allowed_intents = [
        intent
        for intent in state.get("allowed_intents", [])
        if isinstance(intent, str) and intent
    ]
    default_intent = state.get("default_intent", "unknown")
    if not isinstance(default_intent, str):
        default_intent = "unknown"

    classified_intent = _select_allowed_intent(
        candidate_intent,
        default_intent,
        allowed_intents,
    )

    return {
        "classified_intent": classified_intent,
        "raw_intent_model_output": {
            "mode": "deterministic_keyword",
            "matchedRule": matched_rule,
            "candidateIntent": candidate_intent,
        },
    }


def _classify_keywords(player_input: str) -> tuple[str, str]:
    if any(keyword in player_input for keyword in ("loot", "drop", "reward table")):
        return "inquiry_of_weekly_loot", "weekly_loot_keywords"

    if any(keyword in player_input for keyword in ("brother", "tomas")):
        return "mention_brother_helped", "brother_keywords"

    if any(keyword in player_input for keyword in ("frostwell", "dungeon")):
        return "ask_about_dungeon", "dungeon_keywords"

    if any(keyword in player_input for keyword in ("ignore", "system", "prompt")):
        return "prompt_injection", "prompt_injection_keywords"

    return "unknown", "default"


def _select_allowed_intent(
    candidate_intent: str,
    default_intent: str,
    allowed_intents: list[str],
) -> str:
    if candidate_intent in allowed_intents:
        return candidate_intent

    if default_intent in allowed_intents:
        return default_intent

    if allowed_intents:
        return allowed_intents[0]

    return "unknown"
