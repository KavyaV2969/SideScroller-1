"""Pydantic DTOs whose JSON field names match Unity's JsonUtility contracts."""

from pydantic import BaseModel, Field


REQUEST_MODE_INTENT_ROUTE = "intent_route"
REQUEST_MODE_FREE_RESPONSE = "free_response"

RESPONSE_TYPE_INTENT_ROUTE = "intent_route"
RESPONSE_TYPE_GENERATED_RESPONSE = "generated_response"
RESPONSE_TYPE_FALLBACK = "fallback"


class InventoryItemSnapshot(BaseModel):
    """Matches Unity's InventoryItemSnapshot DTO."""

    itemId: str = ""
    quantity: int = 0


class PlayerStateSnapshot(BaseModel):
    """Matches Unity's PlayerStateSnapshot DTO."""

    currentLocationId: str = ""
    playerX: float = 0.0
    playerY: float = 0.0
    moveInputX: float = 0.0
    moveInputY: float = 0.0
    movementEnabled: bool = False
    activeQuestFlags: list[str] = Field(default_factory=list)
    completedQuestFlags: list[str] = Field(default_factory=list)
    inventoryItemIds: list[str] = Field(default_factory=list)
    inventoryItems: list[InventoryItemSnapshot] = Field(default_factory=list)


class AIDialogueRequest(BaseModel):
    """Matches Unity's AIDialogueRequest DTO."""

    npcId: str = ""
    playerInput: str = ""
    requestedMode: str = ""
    currentDialogueId: str = ""
    currentNodeId: str = ""
    generatedResponseRequestId: str = ""
    playerState: PlayerStateSnapshot = Field(default_factory=PlayerStateSnapshot)


class AIDialogueAction(BaseModel):
    """Matches Unity's AIDialogueAction DTO."""

    actionType: str = ""
    itemId: str = ""
    quantity: int = 0
    flagId: str = ""
    questId: str = ""


class AISafetyInfo(BaseModel):
    """Matches Unity's AISafetyInfo DTO."""

    blocked: bool = False
    reason: str = ""


class AIDialogueResponse(BaseModel):
    """Matches Unity's AIDialogueResponse DTO."""

    responseType: str = ""
    npcId: str = ""
    intent: str = ""
    dialogueId: str = ""
    startNodeId: str = ""
    freeChatText: str = ""
    proposedActions: list[AIDialogueAction] = Field(default_factory=list)
    safety: AISafetyInfo = Field(default_factory=AISafetyInfo)


def build_intent_route_response(npc_id: str, intent: str) -> AIDialogueResponse:
    """Build an action-free response for Unity-authored intent routing."""

    return AIDialogueResponse(
        responseType=RESPONSE_TYPE_INTENT_ROUTE,
        npcId=npc_id,
        intent=intent,
        proposedActions=[],
        safety=AISafetyInfo(blocked=False),
    )


def build_generated_response(npc_id: str, text: str) -> AIDialogueResponse:
    """Build an action-free generated NPC line."""

    return AIDialogueResponse(
        responseType=RESPONSE_TYPE_GENERATED_RESPONSE,
        npcId=npc_id,
        freeChatText=text,
        proposedActions=[],
        safety=AISafetyInfo(blocked=False),
    )


def build_fallback_response(npc_id: str, reason: str = "") -> AIDialogueResponse:
    """Build an action-free non-blocking fallback response."""

    return AIDialogueResponse(
        responseType=RESPONSE_TYPE_FALLBACK,
        npcId=npc_id,
        proposedActions=[],
        safety=AISafetyInfo(blocked=False, reason=reason),
    )


def build_blocked_response(npc_id: str, reason: str) -> AIDialogueResponse:
    """Build an action-free blocked fallback response."""

    return AIDialogueResponse(
        responseType=RESPONSE_TYPE_FALLBACK,
        npcId=npc_id,
        proposedActions=[],
        safety=AISafetyInfo(blocked=True, reason=reason),
    )


def has_actions(response: AIDialogueResponse) -> bool:
    """Return whether a response attempts to include gameplay actions."""

    return len(response.proposedActions) > 0
