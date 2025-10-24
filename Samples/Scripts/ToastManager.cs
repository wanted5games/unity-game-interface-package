using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance;
    [Header("UI References")]
    public Transform toastContainer; // Container where toasts appear
    public GameObject toastPrefab;   // Prefab for a single toast

    [Header("Settings")]
    public float toastDuration = 2f;     // How long each toast stays
    public float spacing = 10f;          // Space between stacked toasts
    public float fadeDuration = 0.5f;    // Fade in/out duration

    private readonly List<GameObject> activeToasts = new List<GameObject>();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowToast(string message)
    {
        Debug.Log(message);
        GameObject toast = Instantiate(toastPrefab, toastContainer);
        toast.transform.SetAsFirstSibling(); // newest toast on top

        Text text = toast.GetComponentInChildren<Text>();
        if (text != null) text.text = message;

        activeToasts.Add(toast);

        StartCoroutine(ToastLifecycle(toast));
        RearrangeToasts();
    }

    private IEnumerator ToastLifecycle(GameObject toast)
    {
        CanvasGroup cg = toast.GetComponent<CanvasGroup>();
        if (cg == null) cg = toast.AddComponent<CanvasGroup>();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            cg.alpha = t / fadeDuration;
            yield return null;
        }
        cg.alpha = 1f;

        // Wait duration
        yield return new WaitForSecondsRealtime(toastDuration);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            cg.alpha = 1f - t / fadeDuration;
            yield return null;
        }

        cg.alpha = 0f;
        activeToasts.Remove(toast);
        Destroy(toast);
        RearrangeToasts();
    }

    private void RearrangeToasts()
    {
        float yOffset = 0f;
        foreach (var toast in activeToasts)
        {
            RectTransform rt = toast.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, yOffset);
            yOffset += rt.sizeDelta.y + spacing;
        }
    }
}