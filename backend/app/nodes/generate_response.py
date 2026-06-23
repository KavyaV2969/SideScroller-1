from typing import Any

from app.config import get_settings
from app.llm.client import LLMClient
from app.state import DialogueGraphState
from app.tools.registry import call_tool


GENERIC_FALLBACK_TEXT = "I do not know enough to answer that."


def call_tool_layer(state: DialogueGraphState) -> DialogueGraphState:
    """Call only the read-only tools allowed by the authored generation policy."""

    allowed_tools = state.get("allowed_tools", [])
    if not isinstance(allowed_tools, list) or not allowed_tools:
        return _fallback(state, "No allowed tools are available.")

    tool_results: dict[str, Any] = {}
    for tool_name in allowed_tools:
        if not isinstance(tool_name, str):
            return _fallback(state, "Generation policy contains an invalid tool name.")

        try:
            tool_results[tool_name] = call_tool(tool_name)
        except (FileNotFoundError, KeyError, OSError, ValueError) as error:
            return _fallback(state, f"Unable to call approved tool '{tool_name}': {error}")

    return {"tool_results": tool_results}


def generate_free_response(state: DialogueGraphState) -> DialogueGraphState:
    """Generate with LLM mode when configured, else use deterministic templates."""

    if get_settings().ai_backend_mode == "llm":
        try:
            return _generate_free_response_llm(state)
        except Exception:
            return _generate_free_response_deterministic(state, llm_fallback=True)

    return _generate_free_response_deterministic(state)


def _generate_free_response_llm(state: DialogueGraphState) -> DialogueGraphState:
    request = state.get("request")
    output = LLMClient().generate_dialogue_line(
        npc_profile=_dict_value(state.get("npc_profile")),
        player_input=request.playerInput if request is not None else "",
        generation_policy=_dict_value(state.get("generation_policy")),
        tool_results=_dict_value(state.get("tool_results")),
    )

    return {
        "generated_text": output.text,
        "raw_generation_model_output": {
            "mode": "openai_structured",
            "text": output.text,
        },
    }


def _generate_free_response_deterministic(
    state: DialogueGraphState,
    *,
    llm_fallback: bool = False,
) -> DialogueGraphState:
    """Build a concise deterministic line from approved tool data."""

    policy = state.get("generation_policy", {})
    if not isinstance(policy, dict):
        policy = {}

    tool_results = state.get("tool_results", {})
    if not isinstance(tool_results, dict):
        tool_results = {}

    weekly_loot = tool_results.get("get_weekly_loot")
    if isinstance(weekly_loot, dict):
        generated_text = _build_weekly_loot_line(weekly_loot, _fallback_text(policy))
        used_tools = ["get_weekly_loot"]
    else:
        generated_text = _fallback_text(policy)
        used_tools = []

    raw_output: dict[str, Any] = {
        "mode": "deterministic_template",
        "usedTools": used_tools,
    }
    if llm_fallback:
        raw_output["fallbackFrom"] = "llm"

    return {
        "generated_text": generated_text,
        "raw_generation_model_output": raw_output,
    }


def _build_weekly_loot_line(loot: dict[str, Any], fallback_text: str) -> str:
    notable_loot = loot.get("notableLoot")
    if not isinstance(notable_loot, str) or not notable_loot:
        return fallback_text

    common_loot = loot.get("commonLoot")
    common_names = (
        [item for item in common_loot if isinstance(item, str) and item]
        if isinstance(common_loot, list)
        else []
    )
    if not common_names:
        return f"This week's notable loot is {notable_loot}."

    return (
        f"This week's notable loot is {notable_loot}. "
        f"The common pool includes {', '.join(common_names)}."
    )


def _fallback_text(policy: dict[str, Any]) -> str:
    text = policy.get("fallbackText")
    return text if isinstance(text, str) and text else GENERIC_FALLBACK_TEXT


def _dict_value(value: object) -> dict[str, Any]:
    return value if isinstance(value, dict) else {}


def _fallback(state: DialogueGraphState, reason: str) -> DialogueGraphState:
    return {
        "graph_route": "fallback",
        "fallback_reason": reason,
        "errors": [*state.get("errors", []), reason],
    }
