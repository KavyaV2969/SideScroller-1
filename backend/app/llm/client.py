from typing import Any

from openai import OpenAI

from app.config import Settings, get_settings
from app.llm.schemas import GeneratedDialogueOutput, IntentClassificationOutput
from app.prompts.generated_response import build_generated_response_prompt
from app.prompts.intent_classifier import build_intent_classifier_prompt


class LLMClient:
    """Small structured-output adapter around the OpenAI Responses API."""

    def __init__(self, settings: Settings | None = None) -> None:
        self._settings = settings or get_settings()
        if not self._settings.openai_api_key:
            raise RuntimeError("OPENAI_API_KEY is required when AI_BACKEND_MODE=llm.")

        self._client = OpenAI(
            api_key=self._settings.openai_api_key,
            timeout=self._settings.openai_timeout_seconds,
        )

    def classify_intent(
        self,
        *,
        npc_profile: dict[str, Any],
        player_input: str,
        allowed_intents: list[str],
        default_intent: str,
        intent_descriptions: dict[str, str],
    ) -> IntentClassificationOutput:
        """Classify one player message into a backend-authored intent allowlist."""

        prompt = build_intent_classifier_prompt(
            npc_profile=npc_profile,
            player_input=player_input,
            allowed_intents=allowed_intents,
            default_intent=default_intent,
            intent_descriptions=intent_descriptions,
        )
        return self._parse(prompt, IntentClassificationOutput)

    def generate_dialogue_line(
        self,
        *,
        npc_profile: dict[str, Any],
        player_input: str,
        generation_policy: dict[str, Any],
        tool_results: dict[str, Any],
    ) -> GeneratedDialogueOutput:
        """Generate one NPC line from policy-approved context and tool results."""

        prompt = build_generated_response_prompt(
            npc_profile=npc_profile,
            player_input=player_input,
            generation_policy=generation_policy,
            tool_results=tool_results,
        )
        return self._parse(prompt, GeneratedDialogueOutput)

    def _parse(
        self,
        prompt: str,
        output_type: type[IntentClassificationOutput] | type[GeneratedDialogueOutput],
    ) -> IntentClassificationOutput | GeneratedDialogueOutput:
        response = self._client.responses.parse(
            model=self._settings.openai_model,
            input=prompt,
            text_format=output_type,
            store=False,
        )
        parsed = response.output_parsed
        if not isinstance(parsed, output_type):
            raise RuntimeError("OpenAI response did not match the expected structured output.")

        return parsed
