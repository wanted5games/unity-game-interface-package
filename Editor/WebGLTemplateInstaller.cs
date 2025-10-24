#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[InitializeOnLoad]
public static class WebGLTemplateInstaller
{
    private const string templateFolderName = "GameInterface";

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

        // Only copy if anything changed
        if (!Directory.Exists(destinationPath) || !FoldersAreEqual(packagePath, destinationPath))
        {
            // Delete old folder if exists
            if (Directory.Exists(destinationPath))
            {
                FileUtil.DeleteFileOrDirectory(destinationPath);
                FileUtil.DeleteFileOrDirectory(destinationPath + ".meta");
            }

            // Copy folder
            FileUtil.CopyFileOrDirectory(packagePath, destinationPath);
            AssetDatabase.Refresh();
            Debug.Log($"[Game Interface] WebGL template installed/updated: {templateFolderName}");
        }

    }

    private static bool FoldersAreEqual(string folder1, string folder2)
    {
        var files1 = Directory.GetFiles(folder1, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"))
            .Select(f => f.Substring(folder1.Length)).OrderBy(f => f).ToArray();

        var files2 = Directory.GetFiles(folder2, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"))
            .Select(f => f.Substring(folder2.Length)).OrderBy(f => f).ToArray();

        if (files1.Length != files2.Length) {
            return false;
        }

        for (int i = 0; i < files1.Length; i++)
        {
            string path1 = folder1 + files1[i];
            string path2 = folder2 + files2[i];

            if (!File.Exists(path2)) {
                return false;
            }
            if (!File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2)))
            {
                return false;
            }
        }

        return true;
    }
}
#endif
