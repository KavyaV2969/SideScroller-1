from __future__ import annotations

from typing import Any, Literal, TypedDict

from app.schemas import AIDialogueRequest, AIDialogueResponse


JsonDict = dict[str, Any]

RequestedMode = Literal["intent_route", "free_response"]
GraphRoute = Literal[
    "intent_route",
    "free_response",
    "fallback",
]


class DialogueGraphState(TypedDict, total=False):
    # Original Unity request.
    request: AIDialogueRequest

    # Final Unity-compatible response.
    response: AIDialogueResponse

    # Request routing.
    requested_mode: RequestedMode
    graph_route: GraphRoute

    # Shared loaded context.
    npc_profile: JsonDict

    # Intent-route path.
    node_policy: JsonDict
    allowed_intents: list[str]
    default_intent: str
    intent_descriptions: dict[str, str]
    classified_intent: str
    raw_intent_model_output: JsonDict

    # Free-response path.
    generation_policy: JsonDict
    allowed_tools: list[str]
    tool_results: JsonDict
    generated_text: str
    raw_generation_model_output: JsonDict

    # Validation and fallback.
    safety_blocked: bool
    safety_reason: str
    fallback_reason: str
    errors: list[str]
