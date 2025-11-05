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

        // If destination doesn't exist, copy entire folder (excluding .meta files)
        if (!Directory.Exists(destinationPath))
        {
            CopyDirectoryExcludingMeta(packagePath, destinationPath);
            AssetDatabase.Refresh();
            Debug.Log($"[Game Interface] WebGL template installed: {templateFolderName}");
            return;
        }

        // Check which tracked files need to be updated
        List<string> filesToUpdate = new List<string>();
        foreach (string fileName in trackedFiles)
        {
            // Skip .meta files
            if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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
                // Skip .meta files
                if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string sourceFile = Path.Combine(packagePath, fileName);
                string destFile = Path.Combine(destinationPath, fileName);

                // Ensure destination directory exists
                string destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Copy the file (Unity will automatically generate .meta files)
                File.Copy(sourceFile, destFile, true);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Game Interface] WebGL template updated: {string.Join(", ", filesToUpdate)}");
        }
    }

    private static void CopyDirectoryExcludingMeta(string sourceDir, string destDir)
    {
        // Create destination directory if it doesn't exist
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        // Copy all files except .meta files
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            
            // Skip .meta files
            if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, true);
        }

        // Recursively copy subdirectories
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, subDirName);
            CopyDirectoryExcludingMeta(subDir, destSubDir);
        }
    }
}
#endif
