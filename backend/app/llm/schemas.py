from pydantic import BaseModel, Field


class IntentClassificationOutput(BaseModel):
    """Internal structured output for the intent classifier."""

    intent: str = Field(
        description="One allowed intent selected from the backend-provided allowlist."
    )
    confidence: float = Field(ge=0.0, le=1.0)
    reason: str = ""


class GeneratedDialogueOutput(BaseModel):
    """Internal structured output for one generated NPC line."""

    text: str = Field(description="One concise in-character NPC line.")
