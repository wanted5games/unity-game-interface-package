using System;
using UnityEngine;

public partial class GameInterface
{

    /// <summary>
    /// There are some use cases where the game needs to be muted/unmuted externally. This can be the case when e.g. focus is lost/gained, an ad is shown or sound/music is generally controlled via a 3rd party interface.
    /// React immediately to changes of the(master) mute state: GameInterface.Instance.OnMuteStateChange += (isMuted) => {}
    /// Be aware of the external(master) mute state at game start and before unmute actions: GameInterface.Instance.IsMuted()
    /// The external mute is completely independent of the game's mute and must be treated as a kind of �master� mute. This also means that potential mute buttons in the game remain unaffected by it.
    /// </summary>
    /// <returns></returns>
    public event Action<bool> OnMuteStateChange;

    public void InvokeOnMuteStateChange()
    {
        OnMuteStateChange?.Invoke(IsMuted());
    }

    public void AddMuteStateChangeHandler()
    {
        OnMuteStateChange += (isMuted) =>
        {
            Debug.Log($"[GI] Mute state changed to {isMuted}");

            if (isMuted)
            {
                AudioListener.volume = 0;
            }
            else
            {
                AudioListener.volume = 1;
            }
        };
    }

    /// <summary>
    /// There are some use cases where the game needs to be muted/unmuted externally. This can be the case when e.g. focus is lost/gained, an ad is shown or sound/music is generally controlled via a 3rd party interface.
    /// React immediately to changes of the(master) mute state: GameInterface.Instance.OnMuteStateChange += (isMuted) => {}
    /// Be aware of the external(master) mute state at game start and before unmute actions: GameInterface.Instance.IsMuted()
    /// The external mute is completely independent of the game's mute and must be treated as a kind of �master� mute. This also means that potential mute buttons in the game remain unaffected by it.
    /// Make sure that the initial "mute" check takes place before the very first potential sound/music is played!
    /// </summary>
    /// <returns>bool</returns>
    public bool IsMuted()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.IsMuted();
#else
        return tester ? tester.isMuted : false;
#endif
    }

    /// <summary>
    /// To keep media components such as video ads aligned with the game's audio state, games must report whether audio is muted as a result of player interaction. For example, when the player mutes the game, video ads will also be muted - ensuring a consistent audio experience that respects player preferences and avoids unexpected sound.
    /// Use the following one-way method to report whether the game audio is muted due to player interaction (e.g. when the player (un-)mutes the game, adjusts the volume to or from zero, or when the game starts and the initial player-controlled mute state is known). The goal is to retain knowledge of the player's chosen mute state throughout the entire gameplay session.
    /// Always report the correct mute state immediately after any player-driven change. 
    /// Report the initial mute state during game initialization(before GameReady())
    /// Cover all user interactions that affect audio(buttons, sliders, etc.)
    /// Do not invoke GameMuted() in response to automatic audio state changes, e.g. when the game mutes itself due to focus loss. This method is intended solely for reporting player-initiated mute and unmute actions.
    /// </summary>
    /// <param name="isMuted"></param>
    public void GameMuted(bool isMuted)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.GameMuted(isMuted);
#endif
    }
}
