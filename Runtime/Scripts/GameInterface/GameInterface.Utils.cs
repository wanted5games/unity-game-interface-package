public partial class GameInterface
{
	/// <summary>
	/// Checks if a specific feature is supported by the current platform or environment. Examples are "rewarded", "copyright", "credits", etc.
	/// <para>For example:</para>
	/// <para>- The check via <c>GameInterface.Instance.HasFeature("audio")</c> returns true by default. If false, all mute buttons and other audio-related controls must be hidden and disabled. If hiding a mute button results in a settings overlay being empty, the button for opening/showing the settings must also be hidden. Overlays/Menus without elements should be avoided!</para>
	/// <para>- The check via <c>GameInterface.Instance.HasFeature("pause")</c> returns true by default. If false, all pause buttons and other controls that pause the game (such as a settings button) must be hidden and disabled.</para>
	/// <para>- Games must hide score / progress, if requested: <c>GameInterface.Instance.HasFeature("score")</c>, <c>GameInterface.Instance.HasFeature("progress")</c></para>
	/// <para>- Games must hide UI elements, if requested: <c>GameInterface.Instance.HasFeature("credits")</c>, <c>GameInterface.Instance.HasFeature("version")</c>, <c>GameInterface.Instance.HasFeature("privacy")</c></para>
	/// <para>If rewarded ads are not supported (if <c>GameInterface.Instance.HasFeature("rewarded")</c> returns <c>false</c>) you should implement another ways for players on those platforms to get the same benefits, even tho they cannot watch rewarded ads - contact us and we can discuss the options. It should be ensured that no UI elements point to a possible, but deactivated "Rewarded Ad" functionality.</para>
	/// </summary>
	/// <param name="feature"></param>
	/// <returns>bool</returns>
	public bool HasFeature(string feature)
	{
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.HasFeature(feature);
#else
		return tester ? tester.HasFeature(feature) : true;
#endif
	}

	public object GetConfig(string key)
	{
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.GetConfig(key);
#else
		return null;
#endif
	}

	public string GetCurrentLanguage()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.GetCurrentLanguage();
#else
		return tester ? tester.ToLocale() : "en";
#endif
	}

	public void ResizeGameCanvas()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		GameInterfaceBridge.ResizeGameCanvas();
#endif
	}
}