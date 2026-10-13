"""Local HTTP fixture for Tests/DialogueVerification.cs; never part of the game API."""
from fastapi import FastAPI
from fastapi.responses import PlainTextResponse

from app.main import app as backend


app = FastAPI()
app.include_router(backend.router)


@app.post("/fixture/{case}")
def response_fixture(case: str):
    if case == "malformed":
        return PlainTextResponse("not json")
    if case == "error":
        return PlainTextResponse("unavailable", status_code=503)
    response = dict(
        responseType="generated_response", npcId="mira_village_lass",
        freeChatText="<b>" + "x" * 300 + "</b>", intent="ask_about_dungeon",
        proposedActions=[], safety={"blocked": False, "reason": ""},
    )
    if case == "wrong_npc":
        response["npcId"] = "someone_else"
    elif case == "blocked":
        response["safety"]["blocked"] = True
    elif case == "actions":
        response["proposedActions"] = [{"actionType": "give_item", "itemId": "small_token", "quantity": 1}]
    elif case == "wrong_type":
        response["responseType"] = "free_chat"
    elif case == "empty":
        response["freeChatText"] = " "
    elif case == "intent_actions":
        response["responseType"] = "intent_route"
        response["proposedActions"] = [{"actionType": "give_item"}]
    elif case == "intent":
        response["responseType"] = "intent_route"
    return response
