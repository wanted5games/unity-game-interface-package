using System;
using UnityEngine;

public partial class GameInterface
{
    /// <summary>
    /// There are some use cases where the game needs to be paused/resumed externally. This can be the case when e.g. focus is lost/gained, an ad is shown or pausing is generally controlled via a 3rd party interface.
    /// React immediately to changes of the(master) pause state: GameInterface.Instance.OnPauseStateChange += (isPaused) => {}
    /// Player input should no longer be possible! (Best practice: The game must clearly indicate to the user when it is paused, e.g. with an overlay to gray out the game)
    /// When pausing externally, do not show a (potential) pause menu of the game, as this must be handled completely independently of the player pausing.
    /// </summary>
    /// <returns></returns>
    public event Action<bool> OnPauseStateChange;

    public void InvokeOnPauseStateChange()
    {
        OnPauseStateChange?.Invoke(IsPaused());
    }

    public void AddPauseStateChangeHandler()
    {
        OnPauseStateChange += (isPaused) =>
        {
            Debug.Log($"[GI] Pause state changed to {isPaused}");
            if (isPaused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        };
    }

    /// <summary>
    /// Be aware of the external (master) pause state before pause/resume actions: window.GameInterface.isPaused()
    /// </summary>
    /// <returns></returns>
    public bool IsPaused()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.IsPaused();
#else
        return tester ? tester.isPaused : false;
#endif
    }
}
