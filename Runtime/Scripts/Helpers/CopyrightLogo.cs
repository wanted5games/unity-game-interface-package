using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CopyrightLogo : MonoBehaviour
{
    private enum LogoSize
    {
        Small,
        Medium,
        Large,
        XLarge
    }
    [SerializeField]
    private LogoSize logoSize = LogoSize.Medium;

    private enum LogoTheme
    {
        Dark,
        Light
    }
    [SerializeField]
    private LogoTheme logoTheme = LogoTheme.Dark;
    private Image logo;

    private bool isSupported = false;
    void Start()
    {
        isSupported = GameInterface.Instance.HasFeature("copyright");


        if (isSupported)
        {
            logo = GetComponent<Image>();
            logo.enabled = false;
            DownloadLogo();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void DownloadLogo()
    {
        string logoUrl = GameInterface.Instance.GetCopyrightLogoURL(GetSizeString(), GetThemeString());

        if (string.IsNullOrEmpty(logoUrl))
        {
            Debug.Log("Copyright logo URL is null or empty.");
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(DownloadSprite(logoUrl));
    }

    private string GetSizeString()
    {
        switch (logoSize)
        {
            case LogoSize.Small:
                return "small";
            case LogoSize.Medium:
                return "medium";
            case LogoSize.Large:
                return "large";
            case LogoSize.XLarge:
                return "xlarge";
            default:
                return "medium";
        }
    }

    private string GetThemeString()
    {
        switch (logoTheme)
        {
            case LogoTheme.Dark:
                return "dark";
            case LogoTheme.Light:
                return "light";
            default:
                return "dark";
        }
    }

    private IEnumerator DownloadSprite(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("Copyright logo URL is null or empty.");
            yield break;
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading copyright logo: " + www.error);
            gameObject.SetActive(false);
            yield break;
        }

        Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        logo.sprite = sprite;
        logo.preserveAspect = true;

        logo.enabled = true;
    }
}
