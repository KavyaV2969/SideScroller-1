from collections.abc import Callable
from typing import Any

from app.tools.loot_tools import get_weekly_loot


TOOL_REGISTRY = {
    "get_weekly_loot": get_weekly_loot,
}


def call_tool(tool_name: str) -> dict[str, Any]:
    """Call a registered read-only game-data tool."""

    tool: Callable[[], dict[str, Any]] = TOOL_REGISTRY[tool_name]
    return tool()


def list_tool_names() -> list[str]:
    """Return registered read-only tool names in deterministic order."""

    return sorted(TOOL_REGISTRY)
