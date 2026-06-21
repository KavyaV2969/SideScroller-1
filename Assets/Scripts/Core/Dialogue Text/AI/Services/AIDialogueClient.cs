using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AIDialogueClient : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField] private string backendUrl = "http://localhost:8000/dialogue/query";
    [SerializeField] private int timeoutSeconds = 10;

    [Header("Mock Testing")]
    [SerializeField] private bool useMockResponses = true;
    [SerializeField] private float mockDelaySeconds = 0.5f;

    public async Task<AIDialogueResponse> SendDialogueRequest(AIDialogueRequest request)
    {
        if (request == null)
        {
            Debug.LogWarning("AIDialogueClient received null request.");
            return null;
        }

        if (useMockResponses)
        {
            return await SendMockDialogueRequest(request);
        }

        return await SendRealDialogueRequest(request);
    }

    private async Task<AIDialogueResponse> SendMockDialogueRequest(AIDialogueRequest request)
    {
        await Task.Delay(Mathf.RoundToInt(mockDelaySeconds * 1000f));

        string input = request.playerInput.ToLowerInvariant();

        if (input.Contains("brother") || input.Contains("tomas"))
        {
            return new AIDialogueResponse
            {
                responseType = "intent_route",
                npcId = request.npcId,
                intent = "mention_brother_helped",
                dialogueId = "",
                startNodeId = "",
                freeChatText = "",
                proposedActions = new AIDialogueAction[0],
                safety = new AISafetyInfo
                {
                    blocked = false,
                    reason = ""
                }
            };
        }

        if (input.Contains("frostwell") || input.Contains("dungeon"))
        {
            return new AIDialogueResponse
            {
                responseType = "intent_route",
                npcId = request.npcId,
                intent = "ask_about_dungeon",
                dialogueId = "",
                startNodeId = "",
                freeChatText = "",
                proposedActions = new AIDialogueAction[0],
                safety = new AISafetyInfo
                {
                    blocked = false,
                    reason = ""
                }
            };
        }

        if (input.Contains("ignore") || input.Contains("system") || input.Contains("prompt"))
        {
            return new AIDialogueResponse
            {
                responseType = "intent_route",
                npcId = request.npcId,
                intent = "prompt_injection",
                dialogueId = "",
                startNodeId = "",
                freeChatText = "",
                proposedActions = new AIDialogueAction[0],
                safety = new AISafetyInfo
                {
                    blocked = false,
                    reason = ""
                }
            };
        }

        return new AIDialogueResponse
        {
            responseType = "intent_route",
            npcId = request.npcId,
            intent = "unknown",
            dialogueId = "",
            startNodeId = "",
            freeChatText = "",
            proposedActions = new AIDialogueAction[0],
            safety = new AISafetyInfo
            {
                blocked = false,
                reason = ""
            }
        };
    }

    private async Task<AIDialogueResponse> SendRealDialogueRequest(AIDialogueRequest request)
    {
        string json = JsonUtility.ToJson(request);

        using UnityWebRequest webRequest = new UnityWebRequest(backendUrl, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.timeout = timeoutSeconds;

        webRequest.SetRequestHeader("Content-Type", "application/json");

        UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"AI dialogue request failed: {webRequest.error}");
            return null;
        }

        string responseJson = webRequest.downloadHandler.text;

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            Debug.LogWarning("AI dialogue backend returned empty response.");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<AIDialogueResponse>(responseJson);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Failed to parse AI dialogue response: {exception.Message}");
            return null;
        }
    }
}