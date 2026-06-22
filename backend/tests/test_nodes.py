from collections.abc import Callable

from app.nodes.build_response import (
    build_generated_response_node,
    build_intent_route_response_node,
)
from app.nodes.classify_intent import classify_intent
from app.nodes.generate_response import call_tool_layer, generate_free_response
from app.nodes.load_context import (
    load_generation_policy,
    load_intent_policy,
    load_npc_context,
)
from app.nodes.validate_output import validate_generated_response, validate_intent_output
from app.nodes.validate_request import validate_request
from app.schemas import AIDialogueRequest
from app.state import DialogueGraphState


def test_intent_route_happy_path() -> None:
    state = _run_updates(
        {
            "request": AIDialogueRequest(
                npcId="mira_village_lass",
                requestedMode="intent_route",
                currentDialogueId="mira_main",
                currentNodeId="ask_input",
                playerInput="What is this week's loot?",
            )
        },
        validate_request,
        load_npc_context,
        load_intent_policy,
        classify_intent,
        validate_intent_output,
        build_intent_route_response_node,
    )

    response = state["response"]
    assert response.responseType == "intent_route"
    assert response.intent == "inquiry_of_weekly_loot"
    assert response.proposedActions == []


def test_free_response_happy_path() -> None:
    state = _run_updates(
        {
            "request": AIDialogueRequest(
                npcId="mira_village_lass",
                requestedMode="free_response",
                currentDialogueId="mira_main",
                currentNodeId="weekly_loot_response",
                generatedResponseRequestId="weekly_loot",
                playerInput="What is this week's loot?",
            )
        },
        validate_request,
        load_npc_context,
        load_generation_policy,
        call_tool_layer,
        generate_free_response,
        validate_generated_response,
        build_generated_response_node,
    )

    response = state["response"]
    assert response.responseType == "generated_response"
    assert "Moonsteel Ore" in response.freeChatText
    assert response.proposedActions == []


def test_unsupported_requested_mode_routes_to_fallback() -> None:
    state = _run_updates(
        {"request": AIDialogueRequest(npcId="mira_village_lass", requestedMode="other")},
        validate_request,
    )

    assert state["graph_route"] == "fallback"


def test_free_response_missing_request_id_routes_to_fallback() -> None:
    state = _run_updates(
        {
            "request": AIDialogueRequest(
                npcId="mira_village_lass",
                requestedMode="free_response",
                currentDialogueId="mira_main",
                currentNodeId="weekly_loot_response",
            )
        },
        validate_request,
    )

    assert state["graph_route"] == "fallback"


def test_unknown_tool_routes_to_fallback() -> None:
    state = _run_updates(
        {"allowed_tools": ["unknown_tool"]},
        call_tool_layer,
    )

    assert state["graph_route"] == "fallback"


def test_generated_response_validation_strips_angle_brackets() -> None:
    state = _run_updates(
        {
            "generation_policy": {"fallbackText": "Fallback.", "maxCharacters": 240},
            "generated_text": "<b>Moonsteel Ore</b>",
        },
        validate_generated_response,
    )

    assert state["generated_text"] == "bMoonsteel Ore/b"


def test_built_responses_never_include_actions() -> None:
    intent_state = _run_updates(
        {
            "request": AIDialogueRequest(npcId="mira_village_lass"),
            "classified_intent": "unknown",
        },
        build_intent_route_response_node,
    )
    generated_state = _run_updates(
        {
            "request": AIDialogueRequest(npcId="mira_village_lass"),
            "generated_text": "I am not sure.",
        },
        build_generated_response_node,
    )

    assert intent_state["response"].proposedActions == []
    assert generated_state["response"].proposedActions == []


def test_intent_classifier_never_returns_unallowed_intent() -> None:
    state = _run_updates(
        {
            "request": AIDialogueRequest(playerInput="Tell me about loot."),
            "allowed_intents": ["unknown"],
            "default_intent": "unknown",
        },
        classify_intent,
    )

    assert state["classified_intent"] == "unknown"


def test_generated_response_is_capped_to_policy_limit() -> None:
    state = _run_updates(
        {
            "generation_policy": {"fallbackText": "Fallback.", "maxCharacters": 10},
            "generated_text": "0123456789abc",
        },
        validate_generated_response,
    )

    assert state["generated_text"] == "0123456789"


def _run_updates(
    initial_state: DialogueGraphState,
    *nodes: Callable[[DialogueGraphState], DialogueGraphState],
) -> DialogueGraphState:
    state = dict(initial_state)
    for node in nodes:
        update = node(state)
        state.update(update)

    return state
