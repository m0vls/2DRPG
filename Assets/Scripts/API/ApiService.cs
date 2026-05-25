using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService : MonoBehaviour
{
    public static ApiService Instance { get; private set; }

    [SerializeField] private string baseUrl = "http://localhost:3000/api";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var savedToken = PlayerPrefs.GetString("auth_token", "");
        if (!string.IsNullOrEmpty(savedToken))
            AuthToken = savedToken;
    }

    public string AuthToken { get; set; }

    private string Url(string path) => $"{baseUrl}{path}";

    public IEnumerator Register(string nickname, string password, Action<AuthResponse> onSuccess, Action<string> onError)
    {
        var body = new AuthRequest { nickname = nickname, password = password };
        yield return Post("/auth/register", body, onSuccess, onError);
    }

    public IEnumerator Login(string nickname, string password, Action<AuthResponse> onSuccess, Action<string> onError)
    {
        var body = new AuthRequest { nickname = nickname, password = password };
        yield return Post("/auth/login", body, onSuccess, onError);
    }

    public IEnumerator GetProgress(Action<PlayerProgress> onSuccess, Action<string> onError)
    {
        yield return Get("/progress", onSuccess, onError);
    }

    public IEnumerator UpdateProgress(ProgressUpdate update, Action<PlayerProgress> onSuccess, Action<string> onError)
    {
        yield return Put("/progress", update, onSuccess, onError);
    }

    public IEnumerator GetRunHistory(Action<RunHistoryEntry[]> onSuccess, Action<string> onError)
    {
        using var request = UnityWebRequest.Get(Url("/runs"));
        if (!string.IsNullOrEmpty(AuthToken))
            request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        string wrapped = "{\"items\":" + request.downloadHandler.text + "}";
        var data = JsonUtility.FromJson<RunHistoryList>(wrapped);
        onSuccess?.Invoke(data.items);
    }

    public IEnumerator PostRun(RunRecord run, Action onSuccess, Action<string> onError)
    {
        var json = JsonUtility.ToJson(run);
        using var request = new UnityWebRequest(Url("/runs"), "POST");
        var data = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(AuthToken))
            request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            var err = JsonUtility.FromJson<ApiError>(request.downloadHandler.text);
            onError?.Invoke(err?.error ?? request.error);
        }
        else
        {
            onSuccess?.Invoke();
        }
    }

    private IEnumerator Post<T>(string path, object body, Action<T> onSuccess, Action<string> onError)
    {
        var json = JsonUtility.ToJson(body);
        using var request = new UnityWebRequest(Url(path), "POST");
        var data = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(AuthToken))
            request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");

        yield return request.SendWebRequest();

        HandleResponse(request, onSuccess, onError);
    }

    private IEnumerator Get<T>(string path, Action<T> onSuccess, Action<string> onError)
    {
        using var request = UnityWebRequest.Get(Url(path));
        if (!string.IsNullOrEmpty(AuthToken))
            request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");

        yield return request.SendWebRequest();

        HandleResponse(request, onSuccess, onError);
    }

    private IEnumerator Put<T>(string path, object body, Action<T> onSuccess, Action<string> onError)
    {
        var json = JsonUtility.ToJson(body);
        using var request = new UnityWebRequest(Url(path), "PUT");
        var data = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(AuthToken))
            request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");

        yield return request.SendWebRequest();

        HandleResponse(request, onSuccess, onError);
    }

    private void HandleResponse<T>(UnityWebRequest request, Action<T> onSuccess, Action<string> onError)
    {
        if (request.result != UnityWebRequest.Result.Success && request.responseCode >= 400)
        {
            var error = JsonUtility.FromJson<ApiError>(request.downloadHandler.text);
            onError?.Invoke(error?.error ?? request.error);
            return;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            return;
        }

        var data = JsonUtility.FromJson<T>(request.downloadHandler.text);
        onSuccess?.Invoke(data);
    }
}
