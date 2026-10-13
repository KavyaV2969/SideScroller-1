from typing import Any

from app.config import get_settings
from app.llm.client import LLMClient
from app.nodes.validate_output import select_allowed_intent
from app.state import DialogueGraphState


def classify_intent(state: DialogueGraphState) -> DialogueGraphState:
    """Classify intent with LLM mode when configured, else deterministic rules."""

    if get_settings().ai_backend_mode == "llm":
        try:
            return _classify_intent_llm(state)
        except Exception:
            return _classify_intent_deterministic(state, llm_fallback=True)

    return _classify_intent_deterministic(state)


def _classify_intent_llm(state: DialogueGraphState) -> DialogueGraphState:
    request = state.get("request")
    output = LLMClient().classify_intent(
        npc_profile=state.get("npc_profile", {}),
        player_input=request.playerInput if request is not None else "",
        allowed_intents=state.get("allowed_intents", []),
        default_intent=state.get("default_intent", "unknown"),
        intent_descriptions=state.get("intent_descriptions", {}),
    )

    return {
        "classified_intent": output.intent,
        "raw_intent_model_output": {
            "mode": "openai_structured",
            "intent": output.intent,
            "confidence": output.confidence,
            "reason": output.reason,
        },
    }


def _classify_intent_deterministic(
    state: DialogueGraphState,
    *,
    llm_fallback: bool = False,
) -> DialogueGraphState:
    request = state.get("request")
    player_input = request.playerInput.lower() if request is not None else ""
    candidate_intent, matched_rule = _classify_keywords(player_input)

    classified_intent = select_allowed_intent(
        candidate_intent,
        state.get("default_intent", "unknown"),
        state.get("allowed_intents", []),
    )

    raw_output: dict[str, Any] = {
        "mode": "deterministic_keyword",
        "matchedRule": matched_rule,
        "candidateIntent": candidate_intent,
    }
    if llm_fallback:
        raw_output["fallbackFrom"] = "llm"

    return {
        "classified_intent": classified_intent,
        "raw_intent_model_output": raw_output,
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
