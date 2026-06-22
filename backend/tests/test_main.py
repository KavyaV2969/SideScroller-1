from fastapi.testclient import TestClient

from app.main import app


client = TestClient(app)


def test_health_endpoint() -> None:
    response = client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_intent_route_endpoint() -> None:
    response = client.post("/dialogue/query", json=_intent_request_payload())
    body = response.json()

    assert response.status_code == 200
    assert body["responseType"] == "intent_route"
    assert body["intent"] == "inquiry_of_weekly_loot"
    assert body["proposedActions"] == []


def test_free_response_endpoint() -> None:
    response = client.post("/dialogue/query", json=_free_response_request_payload())
    body = response.json()

    assert response.status_code == 200
    assert body["responseType"] == "generated_response"
    assert "Moonsteel Ore" in body["freeChatText"]
    assert body["intent"] == ""
    assert body["proposedActions"] == []


def test_invalid_mode_returns_fallback_response() -> None:
    payload = _intent_request_payload()
    payload["requestedMode"] = "bad_mode"

    response = client.post("/dialogue/query", json=payload)
    body = response.json()

    assert response.status_code == 200
    assert body["responseType"] == "fallback"
    assert body["proposedActions"] == []


def test_response_uses_unity_camel_case_field_names() -> None:
    response = client.post("/dialogue/query", json=_intent_request_payload())
    body = response.json()

    assert response.status_code == 200
    for field_name in ("responseType", "npcId", "freeChatText", "proposedActions"):
        assert field_name in body
    for field_name in ("response_type", "npc_id", "free_chat_text", "proposed_actions"):
        assert field_name not in body


def _intent_request_payload() -> dict[str, object]:
    return {
        "npcId": "mira_village_lass",
        "playerInput": "What is this week's loot?",
        "requestedMode": "intent_route",
        "currentDialogueId": "mira_main",
        "currentNodeId": "ask_input",
        "generatedResponseRequestId": "",
        "playerState": _player_state_payload(),
    }


def _free_response_request_payload() -> dict[str, object]:
    return {
        "npcId": "mira_village_lass",
        "playerInput": "What is this week's loot?",
        "requestedMode": "free_response",
        "currentDialogueId": "mira_main",
        "currentNodeId": "weekly_loot_response",
        "generatedResponseRequestId": "weekly_loot",
        "playerState": _player_state_payload(),
    }


def _player_state_payload() -> dict[str, object]:
    return {
        "currentLocationId": "north_village",
        "activeQuestFlags": [],
        "completedQuestFlags": [],
        "inventoryItemIds": [],
    }
