using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AIDialogueClient : MonoBehaviour
{
    [SerializeField] private string backendUrl = "http://localhost:8000/dialogue/query";
    [SerializeField] private int timeoutSeconds = 10;

    public async Task<AIDialogueResponse> SendDialogueRequest(AIDialogueRequest request)
    {
        if (request == null)
        {
            Debug.LogWarning("AIDialogueClient received null request.");
            return null;
        }

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