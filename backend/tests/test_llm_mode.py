import importlib
from types import SimpleNamespace

from app.config import get_settings
from app.llm.schemas import GeneratedDialogueOutput, IntentClassificationOutput
from app.nodes.build_response import build_generated_response_node, build_intent_route_response_node
from app.schemas import AIDialogueRequest
from app.state import DialogueGraphState


classify_intent_module = importlib.import_module("app.nodes.classify_intent")
generate_response_module = importlib.import_module("app.nodes.generate_response")


def test_default_settings_are_deterministic(monkeypatch) -> None:
    monkeypatch.delenv("AI_BACKEND_MODE", raising=False)
    get_settings.cache_clear()

    try:
        assert get_settings().ai_backend_mode == "deterministic"
    finally:
        get_settings.cache_clear()


def test_settings_reads_llm_mode_from_environment(monkeypatch) -> None:
    monkeypatch.setenv("AI_BACKEND_MODE", "llm")
    get_settings.cache_clear()

    try:
        assert get_settings().ai_backend_mode == "llm"
    finally:
        get_settings.cache_clear()


def test_deterministic_mode_preserves_keyword_classifier(monkeypatch) -> None:
    monkeypatch.setattr(
        classify_intent_module,
        "get_settings",
        lambda: SimpleNamespace(ai_backend_mode="deterministic"),
    )
    state = _intent_state("What is this week's loot?")

    update = classify_intent_module.classify_intent(state)

    assert update["classified_intent"] == "inquiry_of_weekly_loot"
    assert update["raw_intent_model_output"]["mode"] == "deterministic_keyword"


def test_mocked_llm_classifier_output_is_used(monkeypatch) -> None:
    monkeypatch.setattr(
        classify_intent_module,
        "get_settings",
        lambda: SimpleNamespace(ai_backend_mode="llm"),
    )
    monkeypatch.setattr(classify_intent_module, "LLMClient", _MockLLMClient)

    update = classify_intent_module.classify_intent(_intent_state("Tell me about Frostwell."))

    assert update["classified_intent"] == "ask_about_dungeon"
    assert update["raw_intent_model_output"]["mode"] == "openai_structured"


def test_mocked_llm_generated_text_is_used(monkeypatch) -> None:
    monkeypatch.setattr(
        generate_response_module,
        "get_settings",
        lambda: SimpleNamespace(ai_backend_mode="llm"),
    )
    monkeypatch.setattr(generate_response_module, "LLMClient", _MockLLMClient)

    update = generate_response_module.generate_free_response(_generation_state())

    assert update["generated_text"] == "Moonsteel Ore is the prize most hunters whisper about this week."
    assert update["raw_generation_model_output"]["mode"] == "openai_structured"


def test_llm_failure_falls_back_to_deterministic_behavior(monkeypatch) -> None:
    monkeypatch.setattr(
        classify_intent_module,
        "get_settings",
        lambda: SimpleNamespace(ai_backend_mode="llm"),
    )
    monkeypatch.setattr(classify_intent_module, "LLMClient", _FailingLLMClient)

    update = classify_intent_module.classify_intent(_intent_state("What is this week's loot?"))

    assert update["classified_intent"] == "inquiry_of_weekly_loot"
    assert update["raw_intent_model_output"]["fallbackFrom"] == "llm"


def test_llm_mode_final_responses_are_action_free(monkeypatch) -> None:
    monkeypatch.setattr(
        classify_intent_module,
        "get_settings",
        lambda: SimpleNamespace(ai_backend_mode="llm"),
    )
    monkeypatch.setattr(classify_intent_module, "LLMClient", _MockLLMClient)
    monkeypatch.setattr(
        generate_response_module,
        "get_settings",
        lambda: SimpleNamespace(ai_backend_mode="llm"),
    )
    monkeypatch.setattr(generate_response_module, "LLMClient", _MockLLMClient)

    intent_update = classify_intent_module.classify_intent(_intent_state("Anything."))
    intent_response = build_intent_route_response_node(
        {**_intent_state("Anything."), **intent_update}
    )["response"]
    generated_update = generate_response_module.generate_free_response(_generation_state())
    generated_response = build_generated_response_node(
        {**_generation_state(), **generated_update}
    )["response"]

    assert intent_response.proposedActions == []
    assert generated_response.proposedActions == []


class _MockLLMClient:
    def classify_intent(self, **kwargs: object) -> IntentClassificationOutput:
        return IntentClassificationOutput(
            intent="ask_about_dungeon",
            confidence=0.9,
            reason="Matched the Frostwell topic.",
        )

    def generate_dialogue_line(self, **kwargs: object) -> GeneratedDialogueOutput:
        return GeneratedDialogueOutput(
            text="Moonsteel Ore is the prize most hunters whisper about this week."
        )


class _FailingLLMClient:
    def classify_intent(self, **kwargs: object) -> IntentClassificationOutput:
        raise RuntimeError("LLM unavailable")


def _intent_state(player_input: str) -> DialogueGraphState:
    return {
        "request": AIDialogueRequest(playerInput=player_input),
        "npc_profile": {"displayName": "Mira"},
        "allowed_intents": ["ask_about_dungeon", "inquiry_of_weekly_loot", "unknown"],
        "default_intent": "unknown",
        "intent_descriptions": {},
    }


def _generation_state() -> DialogueGraphState:
    return {
        "request": AIDialogueRequest(playerInput="What is this week's loot?"),
        "npc_profile": {"displayName": "Mira"},
        "generation_policy": {"fallbackText": "Fallback.", "maxCharacters": 240},
        "tool_results": {"get_weekly_loot": {"notableLoot": "Moonsteel Ore"}},
    }
