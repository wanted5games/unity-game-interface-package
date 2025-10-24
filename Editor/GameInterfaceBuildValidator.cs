using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class GameInterfaceBuildValidator
{
    // Settings
    const long MAX_TOTAL_SIZE_BYTES = 100L * 1024 * 1024; // 100 MB
    const long MAX_FILE_SIZE_BYTES = 10L * 1024 * 1024;   // 10 MB

    [PostProcessBuild]
    public static void ValidateWebGLBuild(BuildTarget target, string pathToBuild)
    {
        if (target != BuildTarget.WebGL) return;

        Debug.Log("Starting WebGL build validation...");

        Regex invalidChar = new Regex(@"[^a-zA-Z0-9._-]");
        long totalSize = 0;
        bool hasErrors = false;

        foreach (string file in Directory.GetFiles(pathToBuild, "*", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileName(file);

            // Check for invalid symbols
            if (invalidChar.IsMatch(fileName))
            {
                Debug.LogError($"Invalid filename detected: {file}");
                hasErrors = true;
            }

            // Check file size
            FileInfo fi = new FileInfo(file);
            totalSize += fi.Length;

            if (fi.Length > MAX_FILE_SIZE_BYTES)
            {
                Debug.LogError($"File too large ({fi.Length / (1024 * 1024)} MB): {file}");
                hasErrors = true;
            }
        }

        // Check total bundle size
        if (totalSize > MAX_TOTAL_SIZE_BYTES)
        {
            Debug.LogError($"Total build size is too large ({totalSize / (1024 * 1024)} MB). Maximum allowed is 100 MB.");
            hasErrors = true;
        }

        if (hasErrors)
        {
            Debug.LogError("WebGL build validation FAILED!");
        }
        else
        {
            Debug.Log("WebGL build validation passed ✅");
        }
    }
}
