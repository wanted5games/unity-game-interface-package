using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Supported languages.
/// </summary>
public enum SupportedLanguage
{
    English,
    German,
    Turkish,
    Polish,
    Russian,
    Dutch,
    Spanish,
    Portuguese,
    French
}

/// <summary>
/// Game Interface Settings ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "GameInterfaceTester", menuName = "Game Interface/Tester")]
public partial class GameInterfaceTester : ScriptableObject
{
    // ---------------- Runtime Fields ----------------
    [SerializeField] private bool _rewardedAdAvailable = true;
    [SerializeField] private bool _isMuted = false;
    [SerializeField] private bool _isPaused = false;
    [SerializeField] private int _targetLevel = 1;
    [SerializeField] private string _logoUrl = "";

    [SerializeField] private OffsetResult _offsets;
    [SerializeField] private SupportedLanguage _language = SupportedLanguage.English;

    // ---------------- Runtime Properties ----------------
    public bool rewardedAdAvailable { get => _rewardedAdAvailable; set => _rewardedAdAvailable = value; }
    public bool isPaused { get => _isPaused; set => _isPaused = value; }
    public bool isMuted { get => _isMuted; set => _isMuted = value; }
    public int targetLevel { get => _targetLevel; set => _targetLevel = value; }
    public string logoUrl { get => _logoUrl; set => _logoUrl = value; }
    public OffsetResult offsets { get => _offsets; set => _offsets = value; }
    public SupportedLanguage language { get => _language; set => _language = value; }

    // Returns locale string for the selected language
    public string ToLocale()
    {
        switch (_language)
        {
            case SupportedLanguage.German: return "de-DE";
            case SupportedLanguage.English: return "en";
            case SupportedLanguage.Turkish: return "tr-TR";
            case SupportedLanguage.Polish: return "pl-PL";
            case SupportedLanguage.Russian: return "ru-RU";
            case SupportedLanguage.Dutch: return "nl-NL";
            case SupportedLanguage.Spanish: return "es";
            case SupportedLanguage.Portuguese: return "pt";
            case SupportedLanguage.French: return "fr";
            default: return "en";
        }
    }

    public bool HasFeature(string featureName)
    {
#if UNITY_EDITOR
        foreach (var f in features)
        {
            if (f.name == featureName)
                return f.enabled;
        }
#endif
        return false;
    }

    // ---------- Context Menu Methods ----------
    [ContextMenu("GoToHome")]
    private void GoToHome() => GameInterface.Instance?.InvokeOnGoToHome();

    [ContextMenu("GoToNextLevel")]
    private void GoToNextLevel() => GameInterface.Instance?.InvokeOnGoToNextLevel();

    [ContextMenu("GoToLevel")]
    private void GoToLevel() => GameInterface.Instance?.InvokeOnGoToLevel(targetLevel);

    [ContextMenu("RestartGame")]
    private void RestartGame() => GameInterface.Instance?.InvokeOnRestartGame();

    [ContextMenu("QuitGame")]
    private void QuitGame() => GameInterface.Instance?.InvokeOnQuitGame();

    [ContextMenu("GameOver")]
    private void GameOver() => GameInterface.Instance?.InvokeOnGameOver();
}

#if UNITY_EDITOR

/// <summary>
/// Editor-only logic for GameInterfaceTester.
/// Contains features, buttons, and change detection.
/// </summary>
public partial class GameInterfaceTester
{
    [System.Serializable]
    public class FeatureToggle
    {
        public string name;
        public bool enabled;
    }

    [SerializeField] public List<FeatureToggle> features = new List<FeatureToggle>();

    // Change detection dictionaries (editor only)
    private Dictionary<string, object> previousValues = new Dictionary<string, object>();
    private Dictionary<string, Action<object>> valueChangedCallbacks = new Dictionary<string, Action<object>>();

    // Previous offsets
    private OffsetResult _previousOffset;

    private void OnEnable()
    {
        // Register boolean callbacks
        previousValues["_rewardedAdAvailable"] = _rewardedAdAvailable;
        valueChangedCallbacks["_rewardedAdAvailable"] = value =>
            GameInterface.Instance?.InvokeOnRewardedAdAvailabilityChange(null, null);

        previousValues["_isMuted"] = _isMuted;
        valueChangedCallbacks["_isMuted"] = value =>
            GameInterface.Instance?.InvokeOnMuteStateChange();

        previousValues["_isPaused"] = _isPaused;
        valueChangedCallbacks["_isPaused"] = value =>
            GameInterface.Instance?.InvokeOnPauseStateChange();
    }

    private void OnValidate()
    {
        CheckValueChange("_rewardedAdAvailable", _rewardedAdAvailable);
        CheckValueChange("_isMuted", _isMuted);
        CheckValueChange("_isPaused", _isPaused);

        if (_previousOffset.left != _offsets.left ||
            _previousOffset.right != _offsets.right ||
            _previousOffset.top != _offsets.top ||
            _previousOffset.bottom != _offsets.bottom)
        {
            _previousOffset.left = _offsets.left;
            _previousOffset.right = _offsets.right;
            _previousOffset.top = _offsets.top;
            _previousOffset.bottom = _offsets.bottom;

            OnOffsetChange();
        }
    }

    private void CheckValueChange<T>(string fieldName, T currentValue)
    {
        if (!previousValues.ContainsKey(fieldName))
            previousValues[fieldName] = currentValue;

        if (!EqualityComparer<T>.Default.Equals((T)previousValues[fieldName], currentValue))
        {
            previousValues[fieldName] = currentValue;
            valueChangedCallbacks[fieldName]?.Invoke(currentValue);
        }
    }

    private void OnOffsetChange()
    {
        Debug.Log($"Offsets changed: L={_offsets.left}, R={_offsets.right}, T={_offsets.top}, B={_offsets.bottom}");
        GameInterface.Instance?.InvokeOnOffsetChange(_offsets);
    }
}
#endif
