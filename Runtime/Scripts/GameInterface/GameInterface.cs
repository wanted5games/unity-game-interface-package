using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class GameInterface
{
    private const string PREF_KEY = "GameInterfaceTesterWindow_LastGUID";

    // Singleton instance for callbacks
    private static GameInterface _instance;
    public static GameInterface Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameInterface();
                _instance.Start();
                var go = new GameObject("GameInterface");
                go.AddComponent<GameInterfaceGameObject>();
            }

            return _instance;
        }
    }
    public static GameInterface GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameInterface();
            _instance.Start();
            var go = new GameObject("GameInterface");
            go.AddComponent<GameInterfaceGameObject>();
        }
            
        return _instance;
    }   

    private GameInterfaceTester tester;

    private int _nextRequestId = 1;
    private Dictionary<int, IPendingRequest> _pendingRequests = new Dictionary<int, IPendingRequest>();

    private int GetNextRequestId()
    {
        return _nextRequestId++;
    }

    public void ResolveRequest(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("ResolveRequest called with null or empty json");
            return;
        }

        RequestResult response = null;

        try
        {
            response = JsonUtility.FromJson<RequestResult>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse JSON in ResolveRequest: " + ex);
            return;
        }

        if (response == null)
        {
            Debug.LogError("ResolveRequest: parsed response is null");
            return;
        }

        if (!_pendingRequests.TryGetValue(response.taskId, out var pending))
        {
            Debug.LogWarning($"No pending request found for taskId {response.taskId}");
            return;
        }

        _pendingRequests.Remove(response.taskId);

        try
        {
            // Parse dynamic result
            object result = null;

            if (response.success && !string.IsNullOrEmpty(response.result))
            {
                string trimmed = response.result.Trim();

                if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
                {
                    // For objects/arrays, leave as raw JSON string
                    result = trimmed;
                }
                else if (trimmed == "true" || trimmed == "false")
                {
                    result = trimmed == "true";
                }
                else if (int.TryParse(trimmed, out int intResult))
                {
                    result = intResult;
                }
                else if (float.TryParse(trimmed, out float floatResult))
                {
                    result = floatResult;
                }
                else
                {
                    // Treat as string (remove quotes)
                    result = trimmed.Trim('"');
                }
            }

            if (response.success)
            {
                pending.SetResultFromObject(result);
            }
            else
            {
                pending.SetException(new Exception("Ad failed or closed."));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("ResolveRequest: exception while resolving task: " + ex);
            pending.SetException(ex);
        }
    }
    private async Task<T> ExecuteWebGLRequest<T>(Action<int> webglAction, Action<T> onComplete = null)
    {
        T result;

#if UNITY_WEBGL && !UNITY_EDITOR
        var taskId = GetNextRequestId();
        webglAction(taskId);
        result = await AwaitJsPromise<T>(taskId);
#else
        await Task.Delay(100);

        // If T is bool, set 90% chance true
        if (typeof(T) == typeof(bool))
        {
            result = (T)(object)(UnityEngine.Random.value <= 0.9f);
        }

        else if (typeof(T) == typeof(ShowRewardedAdResult))
        {
            var rewarded = new ShowRewardedAdResult
            {
                isRewardGranted = tester ? tester.rewardedAdAvailable : true,
            };
            result = (T)(object)rewarded;

            if (tester) 
            {
                InvokeOnRewardedAdAvailabilityChange(null, tester.rewardedAdAvailable);

                await Task.Delay(500);

                tester.rewardedAdAvailable = rewarded.isRewardGranted;
                InvokeOnRewardedAdAvailabilityChange(null, tester.rewardedAdAvailable);
            }
        }
        // For other reference types, just create a new instance if possible
        else if (typeof(T).IsClass)
        {
            result = Activator.CreateInstance<T>();
        }
        else
        {
            // For other value types, use default
            result = default;
        }
#endif

        onComplete?.Invoke(result);
        return result;
    }

    private async Task ExecuteWebGLRequest(Action<int> webglAction, Action onComplete = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
         var taskId = GetNextRequestId();
         webglAction(taskId);
         await AwaitJsPromise<object>(taskId);
#else
        await Task.Delay(100);
#endif
        onComplete?.Invoke();
    }

    public Task<T> AwaitJsPromise<T>(int requestId)
    {
        var pending = new PendingRequest<T>();
        _pendingRequests[requestId] = pending;
        return pending.Task;
    }

    public void ResolveAction(string json)
    {
        var data = JsonUtility.FromJson<JsonActionData>(json);

        if (data == null || string.IsNullOrEmpty(data.type))
        {
            Debug.LogWarning("[GI] Invalid action data received.");
            Debug.Log(json);

            return;
        }

        string actionType = data.type;
        switch (actionType)
        {
            case "OnGoToHome": OnGoToHome?.Invoke(); break;
            case "OnPauseStateChange": OnPauseStateChange?.Invoke(data.isPaused); break;
            case "OnGoToNextLevel": OnGoToNextLevel?.Invoke(); break;
            case "OnGoToLevel": OnGoToLevel?.Invoke(data.level); break;
            case "OnRestartGame": OnRestartGame?.Invoke(); break;
            case "OnQuitGame": OnQuitGame?.Invoke(); break;
            case "OnGameOver": OnGameOver?.Invoke(); break;
            case "OnMuteStateChange": OnMuteStateChange?.Invoke(data.isMuted); break;
            case "OnRewardedAdAvailabilityChange": OnRewardedAdAvailabilityChange?.Invoke(data.eventId, data.hasRewardedAd); break;
            case "OnOffsetChange":
                var offset = new OffsetResult
                {
                    top = data.top,
                    bottom = data.bottom,
                    left = data.left,
                    right = data.right
                };
                OnOffsetChange?.Invoke(offset);
                break;
            case "OnIAPEvent":
                var iapEvent = new IAPEvent
                {
                    type = data.type
                };
                OnIAPEvent?.Invoke(iapEvent);
                break;
            case "OnVisibilityChange": OnVisibilityChange?.Invoke(data.hidden); break;
            default: Debug.LogWarning($"[GI] Unknown action type: {actionType}"); break;
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        EditorPrefs.GetString(GameInterfaceTesterWindow.PREF_KEY, ""); // Get the tester

        string guid = EditorPrefs.GetString(PREF_KEY, "");
        string path = !string.IsNullOrEmpty(guid)
            ? AssetDatabase.GUIDToAssetPath(guid) : GameInterfaceTesterWindow.PREF_PATH;
        tester = AssetDatabase.LoadAssetAtPath<GameInterfaceTester>(path);
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.OnGoToHome();
        GameInterfaceBridge.OnGoToNextLevel();
        GameInterfaceBridge.OnGoToLevel();

        GameInterfaceBridge.OnRestartGame();
        GameInterfaceBridge.OnQuitGame();
        GameInterfaceBridge.OnGameOver();

        GameInterfaceBridge.OnPauseStateChange();
        GameInterfaceBridge.OnMuteStateChange();

        GameInterfaceBridge.OnRewardedAdAvailabilityChange();
        GameInterfaceBridge.OnOffsetChange(true);
        Debug.Log("[GI] GameInterface initialized and callbacks registered.");
#endif
        InitAds();
    }
}


[Serializable]
public class RequestResult
{
    public int taskId;       // The ID of the request
    public bool success;     // True if JS promise resolved, false if rejected
    public string result;    // JSON string representing the actual result (any type)
}

public class JsonActionData
{
    public string type;

    public bool isPaused;
    public bool isMuted;
    public int level;
    public bool hasRewardedAd;
    public string eventId;

    public float top;
    public float bottom;
    public float left;
    public float right;
    public bool hidden;
}

interface IPendingRequest
{
    void SetResultFromObject(object result);
    void SetException(Exception e);
}

class PendingRequest<T> : IPendingRequest
{
    private TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

    public void SetResultFromObject(object result)
    {
        if (result == null)
        {
            tcs.SetResult(default(T));
            return;
        }

        // If T is a class (not primitive) and result is a string, deserialize it
        if (result is string strResult && typeof(T).IsClass && typeof(T) != typeof(string))
        {
            try
            {
                T deserialized = JsonUtility.FromJson<T>(strResult);
                tcs.SetResult(deserialized);
            }
            catch (Exception ex)
            {
                tcs.SetException(new Exception("Failed to deserialize JSON to type " + typeof(T).Name, ex));
            }
        }
        else
        {
            // For primitives or T=string
            tcs.SetResult((T)result);
        }
    }

    public void SetException(Exception e)
    {
        tcs.SetException(e);
    }

    public Task<T> Task => tcs.Task;
}

public class GameInterfaceGameObject : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void ResolveRequest(string json)
    {
        Debug.Log("[GI] ResolveRequest called with json: " + json);
        GameInterface.Instance.ResolveRequest(json);
    }

    public void ResolveAction(string json)
    {
        Debug.Log("[GI] ResolveAction called with json: " + json);
        GameInterface.Instance.ResolveAction(json);
    }   
}