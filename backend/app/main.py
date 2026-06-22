from fastapi import FastAPI

from app.graph import run_dialogue_graph
from app.schemas import AIDialogueRequest, AIDialogueResponse, build_fallback_response


app = FastAPI(
    title="SideScroller AI Dialogue Backend",
    version="0.1.0",
)


@app.get("/health")
def health() -> dict[str, str]:
    """Return a minimal readiness response for local development."""

    return {"status": "ok"}


@app.post("/dialogue/query", response_model=AIDialogueResponse)
def query_dialogue(request: AIDialogueRequest) -> AIDialogueResponse:
    """Run a Unity dialogue request through the compiled deterministic graph."""

    try:
        return run_dialogue_graph(request)
    except Exception:
        return build_fallback_response(
            npc_id=request.npcId,
            reason="Dialogue backend failed.",
        )
