from typing import Any

from app.state import DialogueGraphState, fallback


GENERIC_FALLBACK_TEXT = "I do not know enough to answer that."
_INTERNAL_LEAKAGE_PHRASES = (
    "system prompt",
    "developer instruction",
    "tool result",
    "backend",
    "langgraph",
    "openai",
)


def validate_intent_output(state: DialogueGraphState) -> DialogueGraphState:
    """Constrain a classifier result to the authored intent allowlist."""

    allowed_intents = [
        intent
        for intent in state.get("allowed_intents", [])
        if isinstance(intent, str) and intent
    ]
    if not allowed_intents:
        return fallback(state, "No allowed intents are available.")

    return {
        "classified_intent": select_allowed_intent(
            state.get("classified_intent"), state.get("default_intent", "unknown"), allowed_intents
        )
    }


def select_allowed_intent(candidate: str | None, default: str, allowed: list[str]) -> str:
    if candidate is not None and candidate in allowed:
        return candidate
    if default in allowed:
        return default
    return allowed[0] if allowed else "unknown"


def validate_generated_response(state: DialogueGraphState) -> DialogueGraphState:
    """Sanitize, cap, and remove obvious internal leakage from generated text."""

    policy = state.get("generation_policy", {})
    if not isinstance(policy, dict):
        policy = {}

    default_text = fallback_text(policy)
    generated_text = state.get("generated_text")
    text = generated_text if isinstance(generated_text, str) else ""
    text = text.replace("<", "").replace(">", "").strip()

    if not text or _contains_internal_leakage(text):
        text = default_text

    text = text.replace("<", "").replace(">", "").strip()
    return {"generated_text": text[:_max_characters(policy)]}


def fallback_text(policy: dict[str, Any]) -> str:
    text = policy.get("fallbackText")
    return text if isinstance(text, str) and text else GENERIC_FALLBACK_TEXT


def _max_characters(policy: dict[str, Any]) -> int:
    value = policy.get("maxCharacters", 240)
    return value if isinstance(value, int) and value > 0 else 240


def _contains_internal_leakage(text: str) -> bool:
    normalized_text = text.lower()
    return any(phrase in normalized_text for phrase in _INTERNAL_LEAKAGE_PHRASES)
