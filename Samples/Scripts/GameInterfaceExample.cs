using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GameInterfaceExample : MonoBehaviour
{
    [SerializeField] private Button RewardedAdButton;
    [SerializeField] private Button GameStartButton;
    [SerializeField] private Button GameCompleteButton;
    [SerializeField] private Button GameQuitButton;
    [SerializeField] private Button GamePauseButton;
    [SerializeField] private Button GameResumeButton;
    [SerializeField] private Button BuyProductButton;
    [SerializeField] private GameObject Version;
    [SerializeField] private GameObject Copyright;

    private bool starting = false;
    private bool inGame = false;
    private bool isPaused = false;

    public void Awake()
    {
        GameInterface.Instance.GameReady();
        GameInterface.Instance.InitVisibilityChange();
        isPaused = GameInterface.Instance.IsPaused();
    }

    public void Start()
    {
        GameInterface.Instance.GetIAPProducts((products) =>
        {
            Debug.Log("[GI Tester] Retrieved IAP Products:");
            foreach (var product in products)
            {
                Debug.Log($"- SKU: {product.sku}, Title: {product.title}, Price: {product.displayPrice}");
            }
        });

        Debug.Log("[GI Tester] Current language: " + GameInterface.Instance.GetCurrentLanguage());
        Debug.Log("[GI Tester] Hidden state: " + (GameInterface.Instance.IsHidden() ? "Hidden" : "Visible"));

        RewardedAdButton.gameObject.SetActive(GameInterface.Instance.HasFeature("rewarded"));
        BuyProductButton.gameObject.SetActive(GameInterface.Instance.HasFeature("iap"));
        Version.SetActive(GameInterface.Instance.HasFeature("version"));
        Copyright.SetActive(GameInterface.Instance.HasFeature("copyright"));
    }

    public void OnEnable()
    {
        GameInterface.Instance.OnGoToHome += HandleGoToHome;
        GameInterface.Instance.OnGoToLevel += HandleGoToLevel;
        GameInterface.Instance.OnGoToNextLevel += HandleGoToNextLevel;
        GameInterface.Instance.OnRestartGame += HandleRestartGame;
        GameInterface.Instance.OnQuitGame += HandleQuitGame;
        GameInterface.Instance.OnGameOver += HandleGameOver;

        GameInterface.Instance.OnPauseStateChange += HandlePauseStateChange;
        GameInterface.Instance.OnMuteStateChange += HandleMuteStateChange;

        GameInterface.Instance.OnRewardedAdAvailabilityChange += HandleRewardedAdAvailabilityChange;

        GameInterface.Instance.OnOffsetChange += HandleOffsetChange;

        GameInterface.Instance.OnIAPEvent += HandleIAPEvent;
        GameInterface.Instance.OnVisibilityChange += HandleVisibilityChange;

        HandleRewardedAdAvailabilityChange(null, null);
        CheckGameButtonState();
        HandleOffsetChange(GameInterface.Instance.GetOffsets());
    }

    public void OnDisable()
    {
        GameInterface.Instance.OnGoToHome -= HandleGoToHome;
        GameInterface.Instance.OnGoToLevel -= HandleGoToLevel;
        GameInterface.Instance.OnGoToNextLevel -= HandleGoToNextLevel;
        GameInterface.Instance.OnRestartGame -= HandleRestartGame;
        GameInterface.Instance.OnQuitGame -= HandleQuitGame;
        GameInterface.Instance.OnGameOver -= HandleGameOver;

        GameInterface.Instance.OnPauseStateChange -= HandlePauseStateChange;
        GameInterface.Instance.OnMuteStateChange -= HandleMuteStateChange;

        GameInterface.Instance.OnRewardedAdAvailabilityChange -= HandleRewardedAdAvailabilityChange;

        GameInterface.Instance.OnOffsetChange -= HandleOffsetChange;

        GameInterface.Instance.OnIAPEvent -= HandleIAPEvent;
    }
    private void HandleGoToHome()
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnGoToHome event received");
    }

    private void HandleGoToLevel(int level)
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnGoToLevel event received for level: " + level);
    }

    private void HandleGoToNextLevel()
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnGoToNextLevel event received");
    }

    private void HandleRestartGame()
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnRestartGame event received");
    }

    private void HandleQuitGame()
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnQuitGame event received");
    }

    private void HandleGameOver()
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnGameOver event received");
    }

    private void HandlePauseStateChange(bool isPaused)
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnPauseStateChange event received: " + (isPaused ? "Paused" : "Resumed"));
    }

    private void HandleMuteStateChange(bool isMuted)
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnMuteStateChange event received: " + (isMuted ? "Muted" : "Unmuted"));
    }

    private void HandleRewardedAdAvailabilityChange(string? eventId, bool? hasRewardedAd)
    {
        RewardedAdButton.interactable = GameInterface.Instance.IsRewardedAdAvailable("test_event");
    }

    private void HandleOffsetChange(OffsetResult offsetResult)
    {
        GameInterface.Instance.ResizeGameCanvas(); // It only changes the game canvas in WebGL builds

        ToastManager.Instance?.ShowToast("[GI Tester] OnOffsetChange event received: " +
            "top=" + offsetResult.top + ", " +
            "bottom=" + offsetResult.bottom + ", " +
            "left=" + offsetResult.left + ", " +
            "right=" + offsetResult.right);
    }

    private void HandleIAPEvent(IAPEvent iapEvent)
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnIAPEvent received: " + iapEvent.type);

        switch (iapEvent.type)
        {
            case "PURCHASE_SUCCESS_EVENT":
                ToastManager.Instance.ShowToast("[GI Tester] Purchase successful!");

                var purchasedSku = iapEvent.detail.sku;
                var purchase = iapEvent.detail.purchase;

                if (string.IsNullOrEmpty(purchasedSku) || string.IsNullOrEmpty(purchase))
                {
                    ToastManager.Instance.ShowToast("[GI Tester] Invalid purchase details received.");
                    return;
                }
                // Credit product to user here

                GameInterface.Instance.ConsumeProduct(purchase, (result) =>
                    {
                        ToastManager.Instance.ShowToast("[GI Tester] ConsumeProduct callback executed");
                    });
                break;
            case "PURCHASE_FAIL_EVENT":
                ToastManager.Instance.ShowToast("[GI Tester] Purchase failed!");
                break;
            case "CONSUME_SUCCESS_EVENT":
                ToastManager.Instance.ShowToast("[GI Tester] Consume successful!");
                break;
            case "CONSUME_FAILURE_EVENT":
                ToastManager.Instance.ShowToast("[GI Tester] Consume failed!");
                break;
            default:
                ToastManager.Instance.ShowToast("[GI Tester] Unknown IAP event type: " + iapEvent.type);
                break;
        }
    }

    private void HandleVisibilityChange(bool isHidden)
    {
        ToastManager.Instance.ShowToast("[GI Tester] OnVisibilityChange event received: " + (isHidden ? "Hidden" : "Visible"));
    }

    public async void ShowRewardedAd()
    {
        try
        {
            var result = await GameInterface.Instance.ShowRewardedAd("test_event");
            ToastManager.Instance.ShowToast("[GI Tester] ShowRewardedAd result: " + (result.isRewardGranted ? "Reward Granted" : "No Reward"));

        }
        catch (Exception ex)
        {
            ToastManager.Instance.ShowToast("[GI Tester] ShowRewardedAd failed: " + ex.Message);
        }
    }

    public async void GameStart()
    {
        if (starting || inGame)
        {
            ToastManager.Instance.ShowToast("[GI Tester] Game is already starting or in progress");
            return;
        }

        starting = true;
        CheckGameButtonState();

        await GameInterface.Instance.ShowInterstitialAd("button:menu:start");
        ToastManager.Instance.ShowToast("[GI Tester] Interstitial Ad shown before GameStart");
        await GameInterface.Instance.GameStart(1);
        ToastManager.Instance.ShowToast("[GI Tester] GameStart callback executed for level 1");

        Dictionary<string, object> eventData = new Dictionary<string, object>
        {
            { "event", GAEventType.DESIGN },
            { "eventId", "Game:Start:Level_1" },
        };

        GameInterface.Instance.Track(eventData);

        starting = false;
        inGame = true;
        CheckGameButtonState();
    }

    public void GameComplete()
    {
        GameInterface.Instance.GameComplete(() =>
        {
            Dictionary<string, object> eventData = new Dictionary<string, object>
            {
                { "eventId", "Game:Complete:Level_1" },
            };

            GameInterface.Instance.Track(GAEventType.DESIGN, eventData);

            ToastManager.Instance.ShowToast("[GI Tester] GameComplete callback executed");

            inGame = false;
            CheckGameButtonState();
        });
    }

    public async void GameQuit()
    {
        StartCoroutine(GameQuitCoroutine());
    }

    private IEnumerator GameQuitCoroutine()
    {
        Task task = GameInterface.Instance.GameQuit();
        yield return TaskExtensions.WaitForTask(task);

        Dictionary<string, object> eventData = new Dictionary<string, object>
        {
            { "event", GAEventType.DESIGN },
            { "eventId", "Game:Quit:Level_1" },
        };

        GameInterface.Instance.Track(eventData);

        ToastManager.Instance.ShowToast("[GI Tester] GameQuit callback executed");
        inGame = false;
        CheckGameButtonState();
    }

    public async void GamePause()
    {
        await GameInterface.Instance.GamePause();
        ToastManager.Instance.ShowToast("[GI Tester] GamePause callback executed");
        Time.timeScale = 0f;
        isPaused = true;
        CheckGameButtonState();
    }

    public async void GameResume()
    {
        await GameInterface.Instance.GameResume();
        ToastManager.Instance.ShowToast("[GI Tester] GameResume called");
        Time.timeScale = 1f;
        isPaused = false;
        CheckGameButtonState();
    }

    public async void BuyProduct()
    {
        try
        {
            var result = await GameInterface.Instance.BuyProduct("test_sku");
            ToastManager.Instance.ShowToast("[GI Tester] BuyProduct result:");
            ToastManager.Instance.ShowToast("SKU: " + result.detail.sku);
        }
        catch (Exception ex)
        {
            ToastManager.Instance.ShowToast("[GI Tester] BuyProduct failed: " + ex.Message);
        }
    }

    public void SavePlayerData()
    {
        GameInterface.Instance.SetStorageItem("string", RandomString(10));
        GameInterface.Instance.SetStorageItem("int", UnityEngine.Random.Range(1, 100));
        GameInterface.Instance.SetStorageItem("bool", UnityEngine.Random.value > 0.5f);
        GameInterface.Instance.SetStorageItem("float", UnityEngine.Random.Range(1f, 100f));
        GameInterface.Instance.SetStorageItem("object", new PlayerData { playerName = "Player_" + RandomString(5), highScore = UnityEngine.Random.Range(1000, 5000) });

        ToastManager.Instance.ShowToast("[GI Tester] Saved Player Data to Storage");
    }

    public void LoadPlayerData()
    {
        ToastManager.Instance.ShowToast("[GI Tester] Loaded Player Data from Storage");
        ToastManager.Instance.ShowToast("string: " + GameInterface.Instance.GetStorageItem<string>("string"));
        ToastManager.Instance.ShowToast("int: " + GameInterface.Instance.GetStorageItem<int>("int"));
        ToastManager.Instance.ShowToast("bool: " + GameInterface.Instance.GetStorageItem<bool>("bool"));
        ToastManager.Instance.ShowToast("float: " + GameInterface.Instance.GetStorageItem<float>("float"));
        var pdata = GameInterface.Instance.GetStorageItem<PlayerData>("object");
        ToastManager.Instance.ShowToast("object: playerName=" + pdata.playerName + ", highScore=" + pdata.highScore);
    }

    private void CheckGameButtonState()
    {
        GameStartButton.interactable = !inGame && !starting;
        GameCompleteButton.interactable = inGame;
        GameQuitButton.interactable = inGame;
        GamePauseButton.interactable = inGame && !isPaused;
        GameResumeButton.interactable = inGame && isPaused;
    }
    private static readonly string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string RandomString(int length)
    {
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            int index = UnityEngine.Random.Range(0, chars.Length);
            sb.Append(chars[index]);
        }
        return sb.ToString();
    }

    public class PlayerData
    {
        public string playerName = "";
        public int highScore = 0;
    }
}