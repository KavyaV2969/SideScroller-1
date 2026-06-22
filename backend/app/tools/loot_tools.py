from typing import Any

from app.tools.json_loader import load_json_file
from app.tools.paths import DATA_DIR


def get_weekly_loot() -> dict[str, Any]:
    """Load the current approved weekly loot table."""

    return load_json_file(DATA_DIR / "loot_tables" / "weekly_loot.json")
