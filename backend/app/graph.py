from collections.abc import Callable
from typing import Any

from langgraph.graph import END, START, StateGraph
from langgraph.graph.state import CompiledStateGraph

from app.nodes.build_response import (
    build_fallback_response_node,
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
from app.schemas import AIDialogueRequest, AIDialogueResponse, build_fallback_response
from app.state import DialogueGraphState


def route_after_validation(state: DialogueGraphState) -> str:
    """Route validated requests to shared context loading or fallback."""

    if state.get("graph_route") == "fallback":
        return "fallback"

    return "load_npc_context"


def route_after_context(state: DialogueGraphState) -> str:
    """Route the shared context to its policy-specific branch."""

    graph_route = state.get("graph_route")
    if graph_route == "fallback":
        return "fallback"

    if graph_route == "intent_route":
        return "load_intent_policy"

    if graph_route == "free_response":
        return "load_generation_policy"

    return "fallback"


def route_fallback_or_next(next_node: str) -> Callable[[DialogueGraphState], str]:
    """Build a conditional route that preserves fallback short-circuiting."""

    def route(state: DialogueGraphState) -> str:
        return "fallback" if state.get("graph_route") == "fallback" else next_node

    return route


def create_dialogue_graph() -> CompiledStateGraph[Any, Any, Any, Any]:
    """Compile the deterministic dialogue graph from reusable node functions."""

    graph = StateGraph(DialogueGraphState)
    graph.add_node("validate_request", validate_request)
    graph.add_node("load_npc_context", load_npc_context)
    graph.add_node("load_intent_policy", load_intent_policy)
    graph.add_node("load_generation_policy", load_generation_policy)
    graph.add_node("classify_intent", classify_intent)
    graph.add_node("call_tool_layer", call_tool_layer)
    graph.add_node("generate_free_response", generate_free_response)
    graph.add_node("validate_intent_output", validate_intent_output)
    graph.add_node("validate_generated_response", validate_generated_response)
    graph.add_node("build_intent_route_response", build_intent_route_response_node)
    graph.add_node("build_generated_response", build_generated_response_node)
    graph.add_node("build_fallback_response", build_fallback_response_node)

    graph.add_edge(START, "validate_request")
    graph.add_conditional_edges(
        "validate_request",
        route_after_validation,
        {
            "fallback": "build_fallback_response",
            "load_npc_context": "load_npc_context",
        },
    )
    graph.add_conditional_edges(
        "load_npc_context",
        route_after_context,
        {
            "fallback": "build_fallback_response",
            "load_intent_policy": "load_intent_policy",
            "load_generation_policy": "load_generation_policy",
        },
    )
    graph.add_conditional_edges(
        "load_intent_policy",
        route_fallback_or_next("classify_intent"),
        {
            "fallback": "build_fallback_response",
            "classify_intent": "classify_intent",
        },
    )
    graph.add_edge("classify_intent", "validate_intent_output")
    graph.add_edge("validate_intent_output", "build_intent_route_response")
    graph.add_edge("build_intent_route_response", END)

    graph.add_conditional_edges(
        "load_generation_policy",
        route_fallback_or_next("call_tool_layer"),
        {
            "fallback": "build_fallback_response",
            "call_tool_layer": "call_tool_layer",
        },
    )
    graph.add_conditional_edges(
        "call_tool_layer",
        route_fallback_or_next("generate_free_response"),
        {
            "fallback": "build_fallback_response",
            "generate_free_response": "generate_free_response",
        },
    )
    graph.add_edge("generate_free_response", "validate_generated_response")
    graph.add_edge("validate_generated_response", "build_generated_response")
    graph.add_edge("build_generated_response", END)

    graph.add_edge("build_fallback_response", END)
    return graph.compile()


dialogue_graph = create_dialogue_graph()


def run_dialogue_graph(request: AIDialogueRequest) -> AIDialogueResponse:
    """Run the graph and always return a Unity-compatible response DTO."""

    final_state = dialogue_graph.invoke({"request": request})
    response = final_state.get("response") if isinstance(final_state, dict) else None

    if isinstance(response, AIDialogueResponse):
        return response

    return build_fallback_response(
        request.npcId,
        "Dialogue graph did not produce a response.",
    )
