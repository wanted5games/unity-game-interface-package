#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
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
        "thumbnail.png",
        "index.html"
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

        // Check for missing files (files that exist in source but not in destination)
        List<string> missingFiles = GetMissingFiles(packagePath, destinationPath);
        filesToUpdate.AddRange(missingFiles);

        // Update only the files that changed or are missing
        if (filesToUpdate.Count > 0)
        {
            foreach (string relativePath in filesToUpdate)
            {
                // Skip .meta files
                if (relativePath.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string sourceFile = Path.Combine(packagePath, relativePath);
                string destFile = Path.Combine(destinationPath, relativePath);

                if (!File.Exists(sourceFile))
                {
                    continue;
                }

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

    private static List<string> GetMissingFiles(string sourceDir, string destDir)
    {
        List<string> missingFiles = new List<string>();
        GetMissingFilesRecursive(sourceDir, destDir, sourceDir, missingFiles);
        return missingFiles;
    }

    private static void GetMissingFilesRecursive(string sourceDir, string destDir, string baseSourceDir, List<string> missingFiles)
    {
        // Check all files in the source directory
        foreach (string sourceFile in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(sourceFile);
            
            // Skip .meta files
            if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Get relative path from base source directory
            string relativePath = GetRelativePath(baseSourceDir, sourceFile);
            string destFile = Path.Combine(destDir, relativePath);

            // If file doesn't exist in destination, add to missing files list
            if (!File.Exists(destFile))
            {
                missingFiles.Add(relativePath);
            }
        }

        // Recursively check subdirectories
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string relativeSubDir = GetRelativePath(baseSourceDir, subDir);
            string destSubDir = Path.Combine(destDir, relativeSubDir);
            GetMissingFilesRecursive(subDir, destSubDir, baseSourceDir, missingFiles);
        }
    }

    private static string GetRelativePath(string basePath, string targetPath)
    {
        // Convert to absolute paths for Uri to work correctly
        string baseAbsolute = Path.GetFullPath(basePath);
        string targetAbsolute = Path.GetFullPath(targetPath);
        
        // Ensure base path ends with directory separator
        if (!baseAbsolute.EndsWith(Path.DirectorySeparatorChar.ToString()) && !baseAbsolute.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
        {
            baseAbsolute += Path.DirectorySeparatorChar;
        }
        
        // Use Uri to calculate relative path (compatible with older .NET versions)
        Uri baseUri = new Uri(baseAbsolute);
        Uri targetUri = new Uri(targetAbsolute);
        Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        return relativePath;
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
