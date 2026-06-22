from app.graph import dialogue_graph, run_dialogue_graph
from app.schemas import AIDialogueRequest, AIDialogueResponse


def test_intent_route_happy_path() -> None:
    response = run_dialogue_graph(
        AIDialogueRequest(
            npcId="mira_village_lass",
            playerInput="What is this week's loot?",
            requestedMode="intent_route",
            currentDialogueId="mira_main",
            currentNodeId="ask_input",
        )
    )

    assert response.responseType == "intent_route"
    assert response.npcId == "mira_village_lass"
    assert response.intent == "inquiry_of_weekly_loot"
    assert response.freeChatText == ""
    assert response.proposedActions == []


def test_intent_route_tomas_path() -> None:
    response = run_dialogue_graph(
        AIDialogueRequest(
            npcId="mira_village_lass",
            playerInput="I helped Tomas.",
            requestedMode="intent_route",
            currentDialogueId="mira_main",
            currentNodeId="ask_input",
        )
    )

    assert response.responseType == "intent_route"
    assert response.intent == "mention_brother_helped"
    assert response.freeChatText == ""
    assert response.proposedActions == []


def test_free_response_happy_path() -> None:
    response = run_dialogue_graph(
        AIDialogueRequest(
            npcId="mira_village_lass",
            playerInput="What is this week's loot?",
            requestedMode="free_response",
            currentDialogueId="mira_main",
            currentNodeId="weekly_loot_response",
            generatedResponseRequestId="weekly_loot",
        )
    )

    assert response.responseType == "generated_response"
    assert response.npcId == "mira_village_lass"
    assert "Moonsteel Ore" in response.freeChatText
    assert response.intent == ""
    assert response.proposedActions == []


def test_unsupported_mode_returns_action_free_fallback() -> None:
    response = run_dialogue_graph(
        AIDialogueRequest(
            npcId="mira_village_lass",
            requestedMode="bad_mode",
        )
    )

    assert response.responseType == "fallback"
    assert response.freeChatText == ""
    assert response.proposedActions == []


def test_missing_generated_response_request_id_returns_fallback() -> None:
    response = run_dialogue_graph(
        AIDialogueRequest(
            npcId="mira_village_lass",
            requestedMode="free_response",
            currentDialogueId="mira_main",
            currentNodeId="weekly_loot_response",
            generatedResponseRequestId="",
        )
    )

    assert response.responseType == "fallback"
    assert response.freeChatText == ""
    assert response.proposedActions == []


def test_unauthorized_generated_node_returns_fallback() -> None:
    response = run_dialogue_graph(
        AIDialogueRequest(
            npcId="mira_village_lass",
            requestedMode="free_response",
            currentDialogueId="mira_main",
            currentNodeId="ask_input",
            generatedResponseRequestId="weekly_loot",
        )
    )

    assert response.responseType == "fallback"
    assert response.freeChatText == ""
    assert response.proposedActions == []


def test_compiled_graph_direct_invoke_returns_response() -> None:
    request = AIDialogueRequest(
        npcId="mira_village_lass",
        playerInput="What is this week's loot?",
        requestedMode="intent_route",
        currentDialogueId="mira_main",
        currentNodeId="ask_input",
    )

    final_state = dialogue_graph.invoke({"request": request})

    assert isinstance(final_state["response"], AIDialogueResponse)
    assert final_state["response"].proposedActions == []
