import re
from pathlib import Path
from typing import Any

from app.tools.json_loader import load_json_file
from app.tools.paths import DATA_DIR


_SAFE_DATA_ID = re.compile(r"^[A-Za-z0-9_-]+$")


def get_npc_profile(npc_id: str) -> dict[str, Any]:
    """Load an approved NPC profile and verify its identity."""

    profile = _load_data_file("npcs", npc_id)

    if profile.get("npcId") != npc_id:
        raise ValueError(f"NPC profile identity mismatch for '{npc_id}'.")

    return profile


def get_dialogue_node_policy(dialogue_id: str, node_id: str) -> dict[str, Any]:
    """Load a dialogue node's authored policy."""

    policy = _load_data_file("dialogue_nodes", dialogue_id)
    nodes = policy.get("nodes")

    if not isinstance(nodes, dict) or node_id not in nodes:
        raise KeyError(f"Dialogue node policy not found: '{dialogue_id}/{node_id}'.")

    node_policy = nodes[node_id]
    if not isinstance(node_policy, dict):
        raise ValueError(f"Dialogue node policy must be an object: '{dialogue_id}/{node_id}'.")

    return node_policy


def get_generation_policy(generated_response_request_id: str) -> dict[str, Any]:
    """Load an approved generated-response policy and verify its identity."""

    policy = _load_data_file("generation_policies", generated_response_request_id)

    if policy.get("generatedResponseRequestId") != generated_response_request_id:
        raise ValueError(
            "Generation policy identity mismatch for "
            f"'{generated_response_request_id}'."
        )

    return policy


def _load_data_file(directory: str, data_id: str) -> dict[str, Any]:
    _validate_data_id(data_id)
    return load_json_file(_data_file_path(directory, data_id))


def _validate_data_id(data_id: str) -> None:
    if not _SAFE_DATA_ID.fullmatch(data_id):
        raise ValueError(f"Invalid data identifier: '{data_id}'.")


def _data_file_path(directory: str, data_id: str) -> Path:
    return DATA_DIR / directory / f"{data_id}.json"
