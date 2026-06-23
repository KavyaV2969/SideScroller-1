from pathlib import Path

from app.tools.json_loader import load_json_file
from app.tools.paths import DATA_DIR
from app.tools.registry import list_tool_names


def test_npc_profile_ids_match_filenames() -> None:
    _assert_identity_matches_filename("npcs", "npcId")


def test_dialogue_policy_ids_match_filenames() -> None:
    _assert_identity_matches_filename("dialogue_nodes", "dialogueId")


def test_generation_policy_ids_match_filenames() -> None:
    _assert_identity_matches_filename(
        "generation_policies",
        "generatedResponseRequestId",
    )


def test_generation_policies_reference_registered_tools_and_required_allowlists() -> None:
    registered_tools = set(list_tool_names())

    for policy_path in _json_files("generation_policies"):
        policy = load_json_file(policy_path)
        for field_name in (
            "allowedNpcIds",
            "allowedDialogueIds",
            "allowedNodeIds",
            "allowedTools",
        ):
            value = policy.get(field_name)
            assert isinstance(value, list) and value, (
                f"{policy_path.name} must contain a non-empty {field_name} list."
            )

        unknown_tools = set(policy["allowedTools"]) - registered_tools
        assert not unknown_tools, (
            f"{policy_path.name} references unregistered tools: {sorted(unknown_tools)}"
        )


def _assert_identity_matches_filename(directory: str, identity_field: str) -> None:
    for data_path in _json_files(directory):
        data = load_json_file(data_path)
        assert data.get(identity_field) == data_path.stem, (
            f"{data_path.name} must declare {identity_field}='{data_path.stem}'."
        )


def _json_files(directory: str) -> list[Path]:
    return sorted((DATA_DIR / directory).glob("*.json"))
