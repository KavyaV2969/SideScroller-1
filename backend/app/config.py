from functools import lru_cache
from pathlib import Path
from typing import Literal

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


BACKEND_DIR = Path(__file__).resolve().parents[1]


class Settings(BaseSettings):
    """Runtime configuration for the optional OpenAI-backed node mode."""

    model_config = SettingsConfigDict(
        env_file=BACKEND_DIR / ".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    ai_backend_mode: Literal["deterministic", "llm"] = Field(
        default="deterministic",
        validation_alias="AI_BACKEND_MODE",
    )
    openai_api_key: str = Field(default="", validation_alias="OPENAI_API_KEY")
    openai_model: str = Field(default="gpt-5.4-mini", validation_alias="OPENAI_MODEL")
    openai_timeout_seconds: float = Field(
        default=10.0,
        validation_alias="OPENAI_TIMEOUT_SECONDS",
    )


@lru_cache
def get_settings() -> Settings:
    """Return cached backend settings loaded from environment variables and .env."""

    return Settings()
