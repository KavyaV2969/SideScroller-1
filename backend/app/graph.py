from collections.abc import Callable
from typing import Any

from langgraph.graph import END, START, StateGraph
from langgraph.graph.state import CompiledStateGraph

from app.nodes import (
    build_fallback_response_node,
    build_generated_response_node,
    build_intent_route_response_node,
    call_tool_layer,
    classify_intent,
    generate_free_response,
    load_generation_policy,
    load_intent_policy,
    load_npc_context,
    route_by_requested_mode,
    validate_generated_response,
    validate_intent_output,
    validate_request,
)
from app.schemas import AIDialogueRequest, AIDialogueResponse, build_fallback_response
from app.state import DialogueGraphState


def create_dialogue_graph() -> CompiledStateGraph[Any, Any, Any, Any]:
    """Keep each observable stage, with one fallback rule for every pipeline."""
    graph = StateGraph(DialogueGraphState)
    fallback_node = "build_fallback_response"
    graph.add_node(fallback_node, build_fallback_response_node)
    graph.add_edge(fallback_node, END)

    def pipeline(*stages: Callable[[DialogueGraphState], DialogueGraphState]) -> str:
        names = [stage.__name__.removesuffix("_node") for stage in stages]
        for name, stage in zip(names, stages, strict=True):
            graph.add_node(name, stage)
        for current, following in zip(names, names[1:]):
            def route(state: DialogueGraphState, next_node: str = following) -> str:
                return fallback_node if state.get("graph_route") == "fallback" else next_node

            graph.add_conditional_edges(current, route, [fallback_node, following])
        return names[-1]

    context_end = pipeline(validate_request, load_npc_context)
    intent_end = pipeline(
        load_intent_policy, classify_intent, validate_intent_output,
        build_intent_route_response_node,
    )
    generation_end = pipeline(
        load_generation_policy, call_tool_layer, generate_free_response,
        validate_generated_response, build_generated_response_node,
    )
    graph.add_edge(START, "validate_request")
    graph.add_conditional_edges(context_end, route_by_requested_mode, {
        "intent_route": "load_intent_policy",
        "free_response": "load_generation_policy",
        "fallback": fallback_node,
    })
    graph.add_edge(intent_end, END)
    graph.add_edge(generation_end, END)
    return graph.compile()


dialogue_graph = create_dialogue_graph()


def run_dialogue_graph(request: AIDialogueRequest) -> AIDialogueResponse:
    """Run the graph and return the existing Unity JSON contract."""
    final_state = dialogue_graph.invoke({"request": request})
    response = final_state.get("response") if isinstance(final_state, dict) else None
    if isinstance(response, AIDialogueResponse):
        return response
    return build_fallback_response(request.npcId, "Dialogue graph did not produce a response.")
