using System.Runtime.InteropServices;
using System;

public static class GameInterfaceBridge
{
    [DllImport("__Internal")]
    public static extern void SendPreloadProgress(int progress);

    [DllImport("__Internal")]
    public static extern void GameReady();

    [DllImport("__Internal")]
    public static extern void InitVisibilityChange();

    [DllImport("__Internal")]
    public static extern bool IsHidden();

    [DllImport("__Internal")]
    public static extern void GameStart(int taskId, int level);

    [DllImport("__Internal")]
    public static extern void SendProgress(int progress);

    [DllImport("__Internal")]
    public static extern void SendScore(int score);

    [DllImport("__Internal")]
    public static extern void GameComplete(int taskId);

    [DllImport("__Internal")]
    public static extern void GameOver(int taskId);

    [DllImport("__Internal")]
    public static extern void GameQuit(int taskId);

    [DllImport("__Internal")]
    public static extern void GamePause(int taskId);

    [DllImport("__Internal")]
    public static extern void GameResume(int taskId);

    [DllImport("__Internal")]
    public static extern void OnGoToHome();

    [DllImport("__Internal")]
    public static extern void OnGoToNextLevel();

    [DllImport("__Internal")]
    public static extern void OnGoToLevel();

    [DllImport("__Internal")]
    public static extern void OnRestartGame();

    [DllImport("__Internal")]
    public static extern void OnQuitGame();

    [DllImport("__Internal")]
    public static extern void OnGameOver();

    [DllImport("__Internal")]
    public static extern void OnMuteStateChange();

    [DllImport("__Internal")]
    public static extern bool IsMuted();

    [DllImport("__Internal")]
    public static extern bool HasFeature(string feature);

    [DllImport("__Internal")]
    public static extern float GameMuted(bool isMuted);

    [DllImport("__Internal")]
    public static extern void OnPauseStateChange();

    [DllImport("__Internal")]
    public static extern bool IsPaused();

    [DllImport("__Internal")]
    public static extern string GetCopyrightLogoURL(string size, string theme);

    [DllImport("__Internal")]
    public static extern void ShowInterstitialAd(int taskId, string eventId, string placementType);

    [DllImport("__Internal")]
    public static extern void ShowRewardedAd(int taskId, string eventId);

    [DllImport("__Internal")]
    public static extern void OnRewardedAdAvailabilityChange();

    [DllImport("__Internal")]
    public static extern void HasRewardedAd(int taskId, string eventId);

    [DllImport("__Internal")]
    public static extern bool IsRewardedAdAvailable(string eventId);

    [DllImport("__Internal")]
    public static extern string GetOffsets();

    [DllImport("__Internal")]
    public static extern void OnOffsetChange(bool auto);

    [DllImport("__Internal")]
    public static extern void ResizeGameCanvas();

    [DllImport("__Internal")]
    public static extern int InnerHeight();

    [DllImport("__Internal")]
    public static extern int InnerWidth();

    [DllImport("__Internal")]
    public static extern void Track(string eventType, string data);

    [DllImport("__Internal")]
    public static extern void SetStorageItem(string item, string value);

    [DllImport("__Internal")]
    public static extern string GetStorageItem(string key);

    [DllImport("__Internal")]
    public static extern void RemoveStorageItem(string key);

    [DllImport("__Internal")]
    public static extern void ClearStorage();

    [DllImport("__Internal")]
    public static extern void Log(string[] args);

    [DllImport("__Internal")]
    public static extern string GetConfig(string key);

    [DllImport("__Internal")]
    public static extern string GetCurrentLanguage();

    [DllImport("__Internal")]
    public static extern float GetValue(string key);

    [DllImport("__Internal")]
    public static extern float GetIAPProducts(int taskId);

    [DllImport("__Internal")]
    public static extern float BuyIAPProduct(int taskId, string sku);

    [DllImport("__Internal")]
    public static extern float ConsumeIAPProduct(int taskId, string transactionId);

    [DllImport("__Internal")]
    public static extern void OnIAPEvent();
}

[Serializable]
public class TrackingObject
{
    public string eventId;
}
