from app.tools.game_data_tools import (
    get_dialogue_node_policy,
    get_generation_policy,
    get_npc_profile,
)
from app.tools.loot_tools import get_weekly_loot
from app.tools.registry import call_tool


def test_get_npc_profile_returns_mira() -> None:
    profile = get_npc_profile("mira_village_lass")

    assert profile["displayName"] == "Mira"


def test_dialogue_node_policy_allows_weekly_loot_intent() -> None:
    policy = get_dialogue_node_policy("mira_main", "ask_input")

    assert "inquiry_of_weekly_loot" in policy["allowedIntents"]


def test_generation_policy_allows_weekly_loot_tool() -> None:
    policy = get_generation_policy("weekly_loot")

    assert "get_weekly_loot" in policy["allowedTools"]


def test_weekly_loot_returns_moonsteel_ore() -> None:
    loot = get_weekly_loot()

    assert loot["notableLoot"] == "Moonsteel Ore"


def test_registry_calls_weekly_loot_tool() -> None:
    loot = call_tool("get_weekly_loot")

    assert loot["notableLoot"] == "Moonsteel Ore"


def test_unknown_tool_raises_key_error() -> None:
    try:
        call_tool("unknown_tool")
    except KeyError:
        return

    raise AssertionError("Unknown tool did not raise KeyError.")
