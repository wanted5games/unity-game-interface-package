using System;
using System.Threading.Tasks;

public partial class GameInterface
{
    /// <summary>
    /// In order to be able to control the game from outside, the following requests should be implemented and work at least during the actual gameplay:
    /// Make sure that the GameStart and GameQuit events are also called afterwards according to the respective request.
    /// </summary>
    public event Action OnGoToHome;

    /// <summary>
    /// In order to be able to control the game from outside, the following requests should be implemented and work at least during the actual gameplay:
    /// Make sure that the GameStart and GameQuit events are also called afterwards according to the respective request.
    /// </summary>
    public event Action OnGoToNextLevel;

    /// <summary>
    /// In order to be able to control the game from outside, the following requests should be implemented and work at least during the actual gameplay:
    /// Make sure that the GameStart and GameQuit events are also called afterwards according to the respective request.
    /// </summary>
    public event Action<int> OnGoToLevel;

    /// <summary>
    /// In order to be able to control the game from outside, the following requests should be implemented and work at least during the actual gameplay:
    /// Make sure that the GameStart and GameQuit events are also called afterwards according to the respective request.
    /// </summary>
    public event Action OnRestartGame;

    /// <summary>
    /// In order to be able to control the game from outside, the following requests should be implemented and work at least during the actual gameplay:
    /// Make sure that the GameStart and GameQuit events are also called afterwards according to the respective request.
    /// </summary>
    public event Action OnQuitGame;

    /// <summary>
    /// In order to be able to control the game from outside, the following requests should be implemented and work at least during the actual gameplay:
    /// Make sure that the GameStart and GameQuit events are also called afterwards according to the respective request.
    /// </summary>
    public event Action OnGameOver;

    public void InvokeOnGoToHome() { OnGoToHome?.Invoke(); }
    public void InvokeOnGoToNextLevel() { OnGoToNextLevel?.Invoke(); }
    public void InvokeOnGoToLevel(int level) { OnGoToLevel?.Invoke(level); }
    public void InvokeOnRestartGame() { OnRestartGame?.Invoke(); }
    public void InvokeOnQuitGame() { OnQuitGame?.Invoke(); }
    public void InvokeOnGameOver() { OnGameOver?.Invoke(); }


    /// <summary>
    /// As soon as the progress in the game changes, this is to be sent as an integer between 0 and 100 (percent). This method is mainly used in level-based games.
    /// Please note that this only concerns the progress that the player makes during the game.
    /// This does not include, for example, a timer that runs automatically and signals the end of the level!
    /// </summary>
    /// <param name="progress"></param>
    public void SendProgress(int progress)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.SendProgress(progress);
#endif
    }

    /// <summary>
    /// As soon as the score changes in the level/gameplay, it must be sent as an integer greater than or equal to 0: 
    /// </summary>
    /// <param name="score"></param>
    public void SendScore(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.SendScore(score);
#endif
    }

    /// <summary>
    /// Call window.GameInterface.gameStart(level) before the initial (re-)start of a level/gameplay (mostly initiated by clicking on the play button in the home screen, via the restart button in the settings menu or the next button in the result screen).
    /// If the game has no levels, no parameters need to be passed to the function!
    /// The level/gameplay may not start until the Promise is resolved!
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public Task GameStart(int? level = null, Action onComplete = null)
    {
        return ExecuteWebGLRequest(id => GameInterfaceBridge.GameStart(id, level ?? -999), onComplete);
    }

    /// <summary>
    /// When the player wins the level/gameplay (has reached the end of the level or completed the task), GameInterface.gameComplete() must be called.
    /// The optimal time to call gameComplete() is after the "win" animation has finished, immediately before the game would show a possible result screen.
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task GameComplete(Action onComplete = null)
    {
        return ExecuteWebGLRequest(id => GameInterfaceBridge.GameComplete(id), onComplete);
    }

    /// <summary>
    /// If the player loses the level/gameplay (e.g. if the time runs out without reaching the goal or the player dies), GameInterface.gameOver() must be called.
    /// The optimal time to call gameOver() is after the "fail" animation has ended, immediately before the game would show a possible result screen.
    /// Only after resolved Promise the game is allowed to show a result screen or exit the gameplay with a different target screen.
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task GameOver(Action onComplete = null)
    {
        return ExecuteWebGLRequest(id => GameInterfaceBridge.GameOver(id), onComplete);
    }

    /// <summary>
    /// When a player actively leaves the level/gameplay (e.g. via a �home� button in the settings menu) GameInterface.gameQuit() must be called.
    /// Make sure that you do not leave the level/gameplay until the Promise has been resolved!
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task GameQuit(Action onComplete = null)
    {
        return ExecuteWebGLRequest(id => GameInterfaceBridge.GameQuit(id), onComplete);
    }

    /// <summary>
    /// When a player actively pauses the level/gameplay (e.g. via a �pause� button) GameInterface.gamePause() must be called.
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task GamePause(Action onComplete = null)
    {
        return ExecuteWebGLRequest(id => GameInterfaceBridge.GamePause(id), onComplete);
    }

    /// <summary>
    /// When a player actively resumes the level/gameplay from the (pause) menu GameInterface.gameResume() must be called. 
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task GameResume(Action onComplete = null)
    {
        return ExecuteWebGLRequest(id => GameInterfaceBridge.GameResume(id), onComplete);
    }
}
