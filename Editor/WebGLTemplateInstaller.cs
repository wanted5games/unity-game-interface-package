#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

[InitializeOnLoad]
public static class WebGLTemplateInstaller
{
    private const string templateFolderName = "GameInterface";
    
    // List of files that should trigger a copy when changed
    private static readonly string[] trackedFiles = new string[]
    {
        "thumbnail.png"
    };

    static WebGLTemplateInstaller()
    {
        InstallTemplate();
    }

    private static void InstallTemplate()
    {
        string packagePath = $"Packages/com.famobi.game-interface/WebGLTemplates/{templateFolderName}";
        string destinationPath = $"Assets/WebGLTemplates/{templateFolderName}";

        if (!Directory.Exists(packagePath))
        {
            Debug.LogWarning($"[Game Interface] WebGL template source not found: {packagePath}");
            return;
        }

        // Ensure parent folder exists
        string parentFolder = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(parentFolder))
        {
            Directory.CreateDirectory(parentFolder);
        }

        // If destination doesn't exist, copy entire folder
        if (!Directory.Exists(destinationPath))
        {
            FileUtil.CopyFileOrDirectory(packagePath, destinationPath);
            AssetDatabase.Refresh();
            Debug.Log($"[Game Interface] WebGL template installed: {templateFolderName}");
            return;
        }

        // Check which tracked files need to be updated
        List<string> filesToUpdate = new List<string>();
        foreach (string fileName in trackedFiles)
        {
            string sourceFile = Path.Combine(packagePath, fileName);
            string destFile = Path.Combine(destinationPath, fileName);

            if (!File.Exists(sourceFile))
            {
                continue;
            }

            // If destination file doesn't exist or content differs, add to update list
            if (!File.Exists(destFile) || !File.ReadAllBytes(sourceFile).SequenceEqual(File.ReadAllBytes(destFile)))
            {
                filesToUpdate.Add(fileName);
            }
        }

        // Update only the files that changed
        if (filesToUpdate.Count > 0)
        {
            foreach (string fileName in filesToUpdate)
            {
                string sourceFile = Path.Combine(packagePath, fileName);
                string destFile = Path.Combine(destinationPath, fileName);

                // Ensure destination directory exists
                string destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Copy the file
                File.Copy(sourceFile, destFile, true);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Game Interface] WebGL template updated: {string.Join(", ", filesToUpdate)}");
        }
    }
}
#endif
