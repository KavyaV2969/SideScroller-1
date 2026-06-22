import json

from app.schemas import (
    AIDialogueRequest,
    AIDialogueResponse,
    build_blocked_response,
    build_fallback_response,
    build_generated_response,
    build_intent_route_response,
    has_actions,
)


def test_request_parses_unity_json_shape() -> None:
    payload = {
        "npcId": "mira_village_lass",
        "playerInput": "Tell me about Frostwell.",
        "requestedMode": "intent_route",
        "currentDialogueId": "mira_main",
        "currentNodeId": "ask_input",
        "generatedResponseRequestId": "",
        "playerState": {
            "currentLocationId": "north_village",
            "playerX": 12.5,
            "playerY": -3.0,
            "moveInputX": 1.0,
            "moveInputY": 0.0,
            "movementEnabled": True,
            "activeQuestFlags": ["helped_tomas"],
            "completedQuestFlags": [],
            "inventoryItemIds": ["small_token"],
            "inventoryItems": [{"itemId": "small_token", "quantity": 1}],
        },
    }

    request = AIDialogueRequest.model_validate(payload)

    assert request.npcId == "mira_village_lass"
    assert request.playerState.inventoryItems[0].itemId == "small_token"
    assert request.playerState.inventoryItems[0].quantity == 1


def test_response_serializes_with_unity_field_names() -> None:
    response = build_generated_response("mira_village_lass", "Frostwell is dangerous.")
    payload = json.loads(response.model_dump_json())

    assert set(payload) == {
        "responseType",
        "npcId",
        "intent",
        "dialogueId",
        "startNodeId",
        "freeChatText",
        "proposedActions",
        "safety",
    }
    assert set(payload["safety"]) == {"blocked", "reason"}
    assert "response_type" not in payload
    assert "npc_id" not in payload


def test_intent_route_response_is_action_free() -> None:
    response = build_intent_route_response("mira_village_lass", "ask_about_dungeon")

    assert response.responseType == "intent_route"
    assert response.intent == "ask_about_dungeon"
    assert response.proposedActions == []
    assert not has_actions(response)


def test_generated_response_is_action_free() -> None:
    response = build_generated_response("mira_village_lass", "Frostwell is dangerous.")

    assert response.responseType == "generated_response"
    assert response.freeChatText == "Frostwell is dangerous."
    assert response.proposedActions == []
    assert not has_actions(response)


def test_blocked_response_sets_safety_state() -> None:
    response = build_blocked_response("mira_village_lass", "Unsafe request.")

    assert response.responseType == "fallback"
    assert response.safety.blocked is True
    assert response.safety.reason == "Unsafe request."


def test_all_helper_responses_are_action_free() -> None:
    responses: list[AIDialogueResponse] = [
        build_intent_route_response("mira_village_lass", "unknown"),
        build_generated_response("mira_village_lass", "I am not sure."),
        build_fallback_response("mira_village_lass", "Unavailable."),
        build_blocked_response("mira_village_lass", "Unsafe request."),
    ]

    assert all(not has_actions(response) for response in responses)
