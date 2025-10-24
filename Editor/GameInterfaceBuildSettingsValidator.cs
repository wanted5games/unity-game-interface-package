using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class WebGLBuildSettingsValidator
{
    [PostProcessBuild]
    public static void ValidateWebGLSettings(BuildTarget target, string pathToBuild)
    {
        if (target != BuildTarget.WebGL) return;

        Debug.Log("Validating WebGL build settings...");

        bool hasErrors = false;

        // 1. Compression Format (Gzip or Brotli)
        var compressionFormat = PlayerSettings.WebGL.compressionFormat;
        if (compressionFormat != WebGLCompressionFormat.Gzip && compressionFormat != WebGLCompressionFormat.Brotli)
        {
            Debug.LogError($"Compression format is {compressionFormat}. It should be Gzip or Brotli.");
            hasErrors = true;
        }

        // 2. Decompression Fallback
        if (!PlayerSettings.WebGL.decompressionFallback)
        {
            Debug.LogError("Decompression fallback is disabled. It should be enabled.");
            hasErrors = true;
        }

        // 3. Splash Screen disabled
#if UNITY_2020_1_OR_NEWER
        if (PlayerSettings.SplashScreen.show)
#else
        if (PlayerSettings.SplashScreen.showUnityLogo)
#endif
        {
            Debug.LogError("Splash Screen is enabled. It should be disabled.");
            hasErrors = true;
        }

        // 4. WebGL Template
        string webglTemplate = PlayerSettings.WebGL.template;
        if (webglTemplate != "PROJECT:GameInterface")
        {
            Debug.LogError($"WebGL Template is '{webglTemplate}'. Expected 'PROJECT:GameInterface'.");
            hasErrors = true;
        }

        // 5. Run In Background
        if (!PlayerSettings.runInBackground)
        {
            Debug.LogError("Run In Background is disabled. It should be enabled for WebGL.");
            hasErrors = true;
        }

        // 6. Strip Engine Code
        if (!PlayerSettings.stripEngineCode)
        {
            Debug.LogError("Strip Engine Code is disabled, should be enabled.");
            hasErrors = true;
        }

        if (hasErrors)
        {
            Debug.LogError("WebGL build settings validation FAILED!");
        }
        else
        {
            Debug.Log("WebGL build settings validation passed ✅");
        }
    }
}
