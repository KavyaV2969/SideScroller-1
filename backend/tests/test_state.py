import importlib

from app.schemas import (
    AIDialogueRequest,
    build_generated_response,
    build_intent_route_response,
)
from app.state import DialogueGraphState


def test_state_module_imports_cleanly() -> None:
    module = importlib.import_module("app.state")

    assert module.DialogueGraphState is DialogueGraphState


def test_state_can_be_created_as_a_normal_dict_with_request() -> None:
    request = AIDialogueRequest(
        npcId="mira_village_lass",
        requestedMode="intent_route",
    )
    state = DialogueGraphState(request=request)

    assert isinstance(state, dict)
    assert state["request"] is request


def test_state_can_store_final_response() -> None:
    state = DialogueGraphState()
    response = build_intent_route_response("mira_village_lass", "ask_about_dungeon")

    state["response"] = response

    assert state["response"] is response


def test_state_can_store_intent_route_intermediates() -> None:
    state = DialogueGraphState(
        requested_mode="intent_route",
        graph_route="intent_route",
        node_policy={"defaultIntent": "unknown"},
        allowed_intents=["ask_about_dungeon", "unknown"],
        default_intent="unknown",
        intent_descriptions={"ask_about_dungeon": "Asks about Frostwell."},
        classified_intent="ask_about_dungeon",
        raw_intent_model_output={"intent": "ask_about_dungeon"},
    )

    assert state["classified_intent"] == "ask_about_dungeon"


def test_state_can_store_free_response_intermediates() -> None:
    response = build_generated_response("mira_village_lass", "Moonsteel Ore is notable.")
    state = DialogueGraphState(
        requested_mode="free_response",
        graph_route="free_response",
        generation_policy={"maxCharacters": 240},
        allowed_tools=["get_weekly_loot"],
        tool_results={"get_weekly_loot": {"notableLoot": "Moonsteel Ore"}},
        generated_text=response.freeChatText,
        raw_generation_model_output={"text": response.freeChatText},
    )

    assert state["generated_text"] == "Moonsteel Ore is notable."
