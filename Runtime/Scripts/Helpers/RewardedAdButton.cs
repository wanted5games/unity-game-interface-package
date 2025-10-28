using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class RewardedAdButton : MonoBehaviour
{
    [SerializeField] private string eventName = "";

    public Action OnRewardGranted;
    public Action OnRewardDeclined;

    private bool isSupported = false;
    
    public void Start()
    {

        if (string.IsNullOrEmpty(eventName))
        {
            throw new Exception("RewardedAdButton: Event name is not set"); 
        }

        isSupported = GameInterface.Instance.HasFeature("rewarded");

        if (!isSupported)
        {
            gameObject.SetActive(false);
            return;
        }

        GetComponent<Button>().onClick.AddListener(ShowRewardedAd);
    }

    public void OnEnable()
    {
        HandleRewardedAdAvailabilityChange();
        GameInterface.Instance.OnRewardedAdAvailabilityChange += HandleRewardedAdAvailabilityChange;
    }

    public void OnDisable()
    {
        GameInterface.Instance.OnRewardedAdAvailabilityChange -= HandleRewardedAdAvailabilityChange;
    }

    private void HandleRewardedAdAvailabilityChange(string eventId = null, bool isAvailable = false)
    {
        GetComponent<Button>().interactable = GameInterface.Instance.IsRewardedAdAvailable(eventName);
    }

    private void ShowRewardedAd()
    {
        GameInterface.Instance.ShowRewardedAd(eventName, OnAdResult);
    }
    
    private void OnAdResult(RewardedAdResult result)
    {
        if (result.isRewardGranted)
        {
            OnRewardGranted?.Invoke();
        }
        else
        {
            OnRewardDeclined?.Invoke();
        }
    }
}
