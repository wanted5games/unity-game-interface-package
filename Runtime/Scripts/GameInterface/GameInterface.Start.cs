using System;
using System.Collections;
using UnityEngine;

public partial class GameInterface
{
    public event Action<bool> OnVisibilityChange;
    public void InitAds()
    {
        //StartCoroutine(RewardedAdAvailabilityCoroutine());
    }

    private IEnumerator RewardedAdAvailabilityCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            OnRewardedAdAvailabilityChange?.Invoke(string.Empty, false);
        }
    }

    /// <summary>
    /// During the (pre-)loading process, the game must inform about the current progress (in percentage).
    /// </summary>
    /// <param name="progress">number between 0-100</param>
    public void SendPreloadProgress(int progress)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.SendPreloadProgress(progress);
#endif
    }

    /// <summary>
    /// Call window.GameInterface.gameReady() anywhere the game can land after loading. The most common candidates are the title screen, the level map or the level/gameplay itself.
    /// </summary>
    public void GameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.GameReady();
#endif
    }

    public void DisableSplashScreen()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.DisableSplashScreen();
#endif
    }

    public bool IsHidden()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.IsHidden();
#else
        return false;
#endif
    }

    public void InitVisibilityChange()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.InitVisibilityChange();   
#endif
    }
}
