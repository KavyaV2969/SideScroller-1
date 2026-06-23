# Starting the AI Dialogue Backend

Run these commands from the `backend` folder.

```powershell
cd C:\Users\Lenovo\Documents\UnityProjects\SideScroller-Attempt1\backend
python -m pip install -e ".[dev]"
python -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

The server is ready when this returns JSON in a second terminal or browser:

```text
http://127.0.0.1:8000/health
```

Expected response:

```json
{"status":"ok"}
```

## LLM mode

Your `.env` file controls the backend mode. For OpenAI-backed responses, it should include:

```env
AI_BACKEND_MODE=llm
OPENAI_API_KEY=...
OPENAI_MODEL=gpt-5.4-mini
OPENAI_TIMEOUT_SECONDS=10
```

Use `AI_BACKEND_MODE=deterministic` to run without OpenAI calls. Do not commit `.env` or share its API key.

## Unity connection

In Unity's `AIDialogueClient` Inspector:

- Set **Use Mock Responses** to `false`.
- Set **Backend URL** to `http://localhost:8000/dialogue/query`.

Stop the server with `Ctrl+C` in its terminal.
