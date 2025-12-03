#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

/// <summary>
/// Editor-only ad overlay system for testing ads in the Unity Editor.
/// </summary>
public class AdOverlay : MonoBehaviour
{
    private static AdOverlay _instance;
    public static AdOverlay Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AdOverlay");
                _instance = go.AddComponent<AdOverlay>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private GameObject overlayCanvas;
    private GameObject overlayPanel;
    private Text adText;
    private Button closeButton;
    private Button grantRewardButton;
    private Button rejectRewardButton;

    private Action onAdClosed;
    private Action<string> onAdFailed;
    private TaskCompletionSource<RewardedAdResult> rewardedAdTaskSource;
    private TaskCompletionSource<bool> interstitialAdTaskSource;
    private bool isRewardedAd = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        CreateOverlayUI();
    }

    private void CreateOverlayUI()
    {
        // Create Canvas
        overlayCanvas = new GameObject("AdOverlayCanvas");
        overlayCanvas.transform.SetParent(transform);
        Canvas canvas = overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // Very high to be on top
        
        // Configure CanvasScaler for responsive UI
        CanvasScaler scaler = overlayCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        overlayCanvas.AddComponent<GraphicRaycaster>();

        // Create Panel (background)
        overlayPanel = new GameObject("AdOverlayPanel");
        overlayPanel.transform.SetParent(overlayCanvas.transform, false);
        Image panelImage = overlayPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f); // Dark semi-transparent background

        RectTransform panelRect = overlayPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Create Ad Text
        GameObject textObj = new GameObject("AdText");
        textObj.transform.SetParent(overlayPanel.transform, false);
        adText = textObj.AddComponent<Text>();
        adText.text = "Ad Overlay";
        adText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        adText.fontSize = 36;
        adText.color = Color.white;
        adText.alignment = TextAnchor.MiddleCenter;
        adText.resizeTextForBestFit = true;
        adText.resizeTextMinSize = 20;
        adText.resizeTextMaxSize = 48;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.5f);
        textRect.anchorMax = new Vector2(0.9f, 0.7f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Create Close Button (for interstitial)
        closeButton = CreateResponsiveButton("Close Ad", 0.3f, 0.3f, 0.7f, 0.4f);
        closeButton.onClick.AddListener(OnCloseAd);

        // Create Grant Reward Button (for rewarded)
        grantRewardButton = CreateResponsiveButton("Grant Reward", 0.25f, 0.3f, 0.48f, 0.4f);
        grantRewardButton.onClick.AddListener(OnGrantReward);
        grantRewardButton.gameObject.SetActive(false);

        // Create Reject Reward Button (for rewarded)
        rejectRewardButton = CreateResponsiveButton("Reject Reward", 0.52f, 0.3f, 0.75f, 0.4f);
        rejectRewardButton.onClick.AddListener(OnRejectReward);
        rejectRewardButton.gameObject.SetActive(false);

        // Hide overlay initially
        overlayCanvas.SetActive(false);
    }

    private Button CreateButton(string text, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(overlayPanel.transform, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = position;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return button;
    }

    private Button CreateResponsiveButton(string text, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(overlayPanel.transform, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(anchorMinX, anchorMinY);
        buttonRect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        buttonRect.offsetMin = new Vector2(10, 10); // Padding
        buttonRect.offsetMax = new Vector2(-10, -10); // Padding

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.resizeTextForBestFit = true;
        buttonText.resizeTextMinSize = 12;
        buttonText.resizeTextMaxSize = 24;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return button;
    }

    public Task ShowInterstitialAd(string eventId, Action onClosed, Action<string> onFailed)
    {
        isRewardedAd = false;
        adText.text = $"Interstitial Ad\nEvent ID: {eventId}";
        
        closeButton.gameObject.SetActive(true);
        grantRewardButton.gameObject.SetActive(false);
        rejectRewardButton.gameObject.SetActive(false);

        onAdClosed = onClosed;
        onAdFailed = onFailed;
        rewardedAdTaskSource = null;
        interstitialAdTaskSource = new TaskCompletionSource<bool>();

        overlayCanvas.SetActive(true);
        
        return interstitialAdTaskSource.Task;
    }

    public Task<RewardedAdResult> ShowRewardedAd(string eventId, Action<RewardedAdResult> onClosed, Action<string> onFailed)
    {
        isRewardedAd = true;
        adText.text = $"Rewarded Ad\nEvent ID: {eventId}\n\nWatch the ad to earn a reward!";
        
        closeButton.gameObject.SetActive(false);
        grantRewardButton.gameObject.SetActive(true);
        rejectRewardButton.gameObject.SetActive(true);

        onAdClosed = null;
        onAdFailed = onFailed;
        rewardedAdTaskSource = new TaskCompletionSource<RewardedAdResult>();

        overlayCanvas.SetActive(true);
        
        return rewardedAdTaskSource.Task;
    }

    private void OnCloseAd()
    {
        overlayCanvas.SetActive(false);
        onAdClosed?.Invoke();
        interstitialAdTaskSource?.SetResult(true);
    }

    private void OnGrantReward()
    {
        overlayCanvas.SetActive(false);
        var result = new RewardedAdResult { isRewardGranted = true };
        rewardedAdTaskSource?.SetResult(result);
    }

    private void OnRejectReward()
    {
        overlayCanvas.SetActive(false);
        var result = new RewardedAdResult { isRewardGranted = false };
        rewardedAdTaskSource?.SetResult(result);
    }

    public void HideOverlay()
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.SetActive(false);
        }
    }
}
#endif

