import pytest

from app.config import Settings, get_settings


@pytest.fixture(autouse=True)
def offline_settings(monkeypatch):
    """Tests never read developer credentials or call a paid model by accident."""
    monkeypatch.setitem(Settings.model_config, "env_file", None)
    monkeypatch.setenv("AI_BACKEND_MODE", "deterministic")
    monkeypatch.setenv("OPENAI_API_KEY", "")
    get_settings.cache_clear()
    yield
    get_settings.cache_clear()
