public partial class GameInterface
{
    /// <summary>
    /// Especially exclusive games must include a "copyright" logo, serving as proof of copyright. This logo should be prominently displayed on the main/home screen. It is important to ensure that the logo adjusts responsively, accommodating both the device's orientation and the layout dimensions. Its position should be aligned with the overall game design but generally placed at the bottom area. The logo should be visible immediately after the game has fully loaded and should not require any user interaction to appear. However, if <c>GameInterface.Instance.HasFeature("copyright")</c> returns false, the logo must be hidden.
    /// The logo's URL can be retrieved via <c>GameInterface.Instance.GetCopyrightLogoURL()</c>. By default, this method returns the absolute path to a (light) logo in its original size (1024x1024px).
    /// Depending on the game engine, it may be beneficial to use a smaller logo size.By passing the optional size String parameter, you can access the following sizes:
    /// <para>"small" → 64x64px</para>
    /// <para>"medium" → 128x128px</para>
    /// <para>"large" → 256x256px</para>
    /// <para>"xlarge" → 512x512px</para>
    /// <para>The second (optional) parameter theme specifies the logo variant for background compatibility. The accepted values are:</para>
    /// <para>"dark" (default) → Light logo for dark backgrounds.</para>
    /// <para>"light" → Dark logo for light backgrounds.</para>
    /// <para>In some cases, the logo is transparent or invisible!</para>
    /// <para>DO NOT combine it with any GUI elements</para>
    /// <para>DO NOT give the logo a background color</para>
    /// <para>DO NOT combine the logo with any effects</para>
    /// </summary>
    /// <param name="size"></param>
    /// <param name="theme"></param>
    /// <returns></returns>
    public string GetCopyrightLogoURL(string size, string theme = "dark")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GameInterfaceBridge.GetCopyrightLogoURL(size, theme);
#else
        return tester ? tester.logoUrl : "";
#endif
    }
}
