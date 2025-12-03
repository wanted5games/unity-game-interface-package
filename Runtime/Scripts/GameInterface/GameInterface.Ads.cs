using System;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public partial class GameInterface
{
    private float lastInterstitialAdTime = -1f;

    /// <summary>
    /// Listen for rewarded ad availability changes to update relevant UI elements accordingly. This will also be executed every second, because the availability can change at any time. Make sure to use <c>IsRewardedAdAvailable(eventId)</c> to get the current status.
    /// </summary>
    public event Action<string, bool> OnRewardedAdAvailabilityChange;
    public void InvokeOnRewardedAdAvailabilityChange(string eventId = null, bool isAvailable = false)
    {
        OnRewardedAdAvailabilityChange?.Invoke(eventId, isAvailable);
    }
    /// <summary>
    /// The <c>GameInterface.Instance.onOffsetChange += callback</c> method signals a change in values. This could be triggered by rotating the device.
    /// <para>Example:</para>
    /// <para>After rotating the device a banner ad that was previously displayed under the game in portrait mode is now replaced by an ad on the right side in landscape mode.</para>
    /// Since the "free" area has to change for this, it is important to react directly to a change and to adapt the canvas accordingly:
    /// Depending on the implementation, it may be sufficient to call the game's own resize method when the offsets change.
    /// <para>How to achieve that?</para>
    /// Since the games are required to be responsive anyway, we suggest making the main canvas or container element dependent on the offset values in addition to the size of the window.
    /// So the game should theoretically behave as if it were in an iFrame that we don't stretch to full height and/or width.
    /// Of course, the correct technical implementation depends on the game and cannot be generalized.
    /// In some games, you can achieve your goal with pure CSS adjustments.Others require an(additional) change in the engine's resize method, in which, for example, the value of window.innerHeight has to be reduced by <c>GameInterface.getOffsets().bottom</c> to create an area of a certain height under the game.
    /// <para>Advice of the day</para>
    /// At least for the bottom and right areas, it is often sufficient to simply replace <c>window.innerHeight</c> and <c>window.innerWidth</c> with 
    /// <c>GameInterface.Instance.InnerHeight</c> and <c>GameInterface.Instance.InnerWidth</c>.
    /// It would be nice if the free area didn't look too different from the game. Depending on the technical implementation, this could mean, for example, choosing a background color that matches the game.
    /// </summary>
    public event Action<OffsetResult> OnOffsetChange;

    public void InvokeOnOffsetChange(OffsetResult offsets)
    {
        OnOffsetChange?.Invoke(offsets);
    }

    /// <summary>
    /// The trend is moving away from ads in natural breaks towards ad triggers on buttons. A showInterstitialAd call should therefore be triggered when the player clicks on a button that is suitable for an ad placement. This particularly includes buttons that take the player to another screen.
    /// <para><c>eventId</c> --> <c>"button:$location:$action"</c> (e.g. <c>"button:result:restart"</c>)</para>
    /// <para><c>placementType</c> --> <c>"start", "next", "pause", "resume", "quit", "browse"</c></para>
    /// <para>Games must implement ad-tigger functionality in natural breaks</para>
    /// The most common "natural break" can be found in a result screen: The player has won or lost, the result screen is shown and on it, for example, the achieved score or stars. The following should now be taken into account: Before making an ad call, make sure that no buttons are visible. Show potential buttons only after the ad has finished.
    /// <para><c>eventId</c> --> <c>"break:$location"</c> (e.g. <c>"break:result"</c>)</para>
    /// <para>To prevent players from accidentally clicking on an ad when they actually wanted to click on a button, these should only be made visible after the potential ad. Best practice(for a result screen): Give the player the chance to see the achieved result beforehand and call the ad with the help of a delay or triggered by e.g.the end of a shown animation:</para>
    /// All eventIds triggered in the game must be listed in the famobi.json file. Please use the provided arrays to differentiate between interstitial and rewarded ad events. 
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="placementType"></param>
    /// <returns></returns>
    public async Task ShowInterstitialAd(string eventId, string placementType = "", Action onAdClosed = null, Action<string> onAdFailed = null)
    {
        CheckIfInterstitialEventExists(eventId);
        
        if (tester != null && tester.interstitialAdCooldown > 0 && lastInterstitialAdTime >= 0f)
        {
            float timeSinceLastAdMs = (Time.time - lastInterstitialAdTime) * 1000f;
            if (timeSinceLastAdMs < tester.interstitialAdCooldown)
            {
                float remainingCooldownMs = tester.interstitialAdCooldown - timeSinceLastAdMs;
                string errorMessage = $"Interstitial ad is on cooldown. Please wait {remainingCooldownMs:F0} more milliseconds.";
                Debug.Log($"[GI] {errorMessage}");
                onAdFailed?.Invoke(errorMessage);
                return;
            }
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        await ExecuteWebGLRequest(id => GameInterfaceBridge.ShowInterstitialAd(id, eventId, placementType), onAdClosed, onAdFailed);
        lastInterstitialAdTime = Time.time;
#else
        bool isPaused = IsPaused();
        bool isMuted = IsMuted();
        OnPauseStateChange?.Invoke(true);
        OnMuteStateChange?.Invoke(true);

        int delay = tester != null ? tester.showInterstitialAdDelay : 0;
        await Task.Delay(delay);
        
        await AdOverlay.Instance.ShowInterstitialAd(eventId, onAdClosed, onAdFailed);
        onAdClosed?.Invoke();
        lastInterstitialAdTime = Time.time;

        OnPauseStateChange?.Invoke(isPaused);
        OnMuteStateChange?.Invoke(isMuted);
#endif
    }

    /// <summary>
    /// Games must implement support for rewarded ads. All eventIds triggered in the game must be listed in the famobi.json file. Please use the provided arrays to differentiate between interstitial and rewarded ad events. 
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="onAdClosed"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<RewardedAdResult> ShowRewardedAd(string eventId, Action<RewardedAdResult> onAdClosed = null, Action<string> onAdFailed = null)
    {
        CheckIfRewardedEventExists(eventId);
#if UNITY_WEBGL && !UNITY_EDITOR
        return await ExecuteWebGLRequest<RewardedAdResult>(id => GameInterfaceBridge.ShowRewardedAd(id, eventId), onAdClosed, onAdFailed);
#else
        bool isPaused = IsPaused();
        bool isMuted = IsMuted();
        OnMuteStateChange?.Invoke(true);
        OnPauseStateChange?.Invoke(true);

        int delay = tester != null ? tester.showRewardedAdDelay : 0;
        await Task.Delay(delay);
        

        var result = await AdOverlay.Instance.ShowRewardedAd(eventId, onAdClosed, onAdFailed);
        onAdClosed?.Invoke(result);

        OnMuteStateChange?.Invoke(isMuted);
        OnPauseStateChange?.Invoke(isPaused);
        
        return result;
#endif
    }

    /// <summary>
    /// If no rewarded ad is currently available (checkable via <c>GameInterface.Instance.HasRewardedAd(eventId)</c> or <c>GameInterface.Instance.IsRewardedAdAvailable(eventId)</c>), but the functionality is generally supported and activated (if <c>GameInterface.Instance.HasFeature("rewarded")</c> returns <c>true</c>), the relevant UI elements, such as buttons, should be deactivated or hidden.
    /// The values from <c>GameInterface.Instance.hasRewardedAd()</c> and <c>window.GameInterface.isRewardedAdAvailable(eventId)</c> are mostly just a snapshot and can change within a fraction. It is therefore important not only to use this function once, but to check it at short intervals (or even every frame) in order to adapt UI elements according to the result.
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public Task<bool> HasRewardedAd(string eventId, Action<bool> onResult = null, Action<string> onError = null)
    {
        CheckIfRewardedEventExists(eventId);
        int delay = tester != null ? tester.hasRewardedAdDelay : 0;
        return ExecuteWebGLRequest<bool>(id => GameInterfaceBridge.HasRewardedAd(id, eventId), onResult, onError, delay);
    }

    /// <summary>
    /// If no rewarded ad is currently available (checkable via <c>GameInterface.Instance.HasRewardedAd(eventId)</c> or <c>GameInterface.Instance.IsRewardedAdAvailable(eventId)</c>), but the functionality is generally supported and activated (if <c>GameInterface.Instance.HasFeature("rewarded")</c> returns <c>true</c>), the relevant UI elements, such as buttons, should be deactivated or hidden.
    /// The values from <c>GameInterface.Instance.hasRewardedAd()</c> and <c>window.GameInterface.isRewardedAdAvailable(eventId)</c> are mostly just a snapshot and can change within a fraction. It is therefore important not only to use this function once, but to check it at short intervals (or even every frame) in order to adapt UI elements according to the result.
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public bool IsRewardedAdAvailable(string eventId)
    {
        CheckIfRewardedEventExists(eventId);
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.IsRewardedAdAvailable(eventId);
#else
        return tester ? tester.rewardedAdAvailable : true;
#endif
    }

    /// <summary>
    /// In order to be able to show banner ads in the games and not run the risk of covering UI elements as a result, a functionality must be implemented that can "reserve" a certain free area on the top, bottom, right and left side. It is important to consider these values from the beginning and also continuously, ideally on a per frame basis! However, best practice is to use GameInterface.onOffsetChange and be informed directly about changes.
    /// </summary>
    /// <returns></returns>
    public OffsetResult GetOffsets()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string result = GameInterfaceBridge.GetOffsets();
        if (!string.IsNullOrEmpty(result))
        {
            return JsonUtility.FromJson<OffsetResult>(result);
        }

        return new OffsetResult {
            left = 0,
            right = 0,
            top = 0,
            bottom = 0
        };
#else
        return tester ? tester.offsets : new OffsetResult
        {
            left = 0,
            right = 0,
            top = 0,
            bottom = 0
        };
#endif
    }

    private void CheckIfRewardedEventExists(string eventId)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(eventId))
        {
            Debug.LogError($"[GI] Rewarded event ID is null or empty");
            return;
        }

        string path = Path.Combine(Application.dataPath, "WebGLTemplates/GameInterface/famobi.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"[GI] famobi.json not found");
            return;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<GameInterfaceData>(json);
        if (data.rewarded.eventIds.Contains(eventId))
        {
            return;
        }

        Debug.LogError($"[GI] Rewarded event {eventId} not found in famobi.json");
#endif
    }

    private void CheckIfInterstitialEventExists(string eventId)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(eventId))
        {
            Debug.LogError($"[GI] Interstitial event ID is null or empty");
            return;
        }

        GameInterfaceData data = FetchFamobiJson();
        if (!data.interstitial.eventIds.Contains(eventId))
        {
            Debug.LogError($"[GI] Interstitial event {eventId} not found in famobi.json");
        }
#endif
    }
}

public class RewardedAdResult
{
    public bool isRewardGranted;
}

[Serializable]
public class OffsetResult
{
    public float left;
    public float right;
    public float top;
    public float bottom;
}

[Serializable]
public class InterstitialData
{
    public List<string> eventIds;
    public List<string> allowlist;
    public List<string> blocklist;
}

[Serializable]
public class RewardedData
{
    public List<string> eventIds;
    public List<string> allowlist;
    public List<string> blocklist;
}
