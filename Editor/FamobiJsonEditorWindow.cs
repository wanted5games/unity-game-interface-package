#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

[Serializable]
public class FamobiJsonData
{
    public FamobiJsonInterstitialData interstitial = new FamobiJsonInterstitialData();
    public FamobiJsonRewardedData rewarded = new FamobiJsonRewardedData();
    public FamobiJsonIAPData iap = new FamobiJsonIAPData();
    public int schemaVersion = 1;
}

[Serializable]
public class FamobiJsonInterstitialData
{
    public List<string> eventIds = new List<string>();
    public List<string> allowlist = new List<string> { "*" };
    public List<string> blocklist = new List<string>();
}

[Serializable]
public class FamobiJsonRewardedData
{
    public List<string> eventIds = new List<string>();
    public List<string> allowlist = new List<string> { "*" };
    public List<string> blocklist = new List<string>();
}

[Serializable]
public class FamobiJsonIAPProduct
{
    public string sku = "";
    public string title = "";
    public string description = "";
    public string imageURI = "";
    public float priceValue = 0f;
}

[Serializable]
public class FamobiJsonIAPData
{
    public List<FamobiJsonIAPProduct> products = new List<FamobiJsonIAPProduct>();
}

public class FamobiJsonEditorWindow : EditorWindow
{
    private FamobiJsonData jsonData;
    private Vector2 scrollPosition;
    private string jsonFilePath;
    private new bool hasUnsavedChanges = false;

    private const string JSON_RELATIVE_PATH = "Assets/WebGLTemplates/GameInterface/famobi.json";

    [MenuItem("Game Interface/Famobi.json Editor")]
    public static void ShowWindow()
    {
        FamobiJsonEditorWindow window = GetWindow<FamobiJsonEditorWindow>("Famobi.json Editor");
        window.minSize = new Vector2(500, 400);
        window.LoadJsonFile();
    }

    private void OnEnable()
    {
        string relativePath = JSON_RELATIVE_PATH;
        jsonFilePath = Path.Combine(Application.dataPath, relativePath.Replace("Assets/", ""));
        LoadJsonFile();
    }

    private void LoadJsonFile()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogWarning($"[GI] famobi.json not found at {jsonFilePath}. Creating default file.");
            jsonData = new FamobiJsonData();
            SaveJsonFile();
            AssetDatabase.Refresh();
        }
        else
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                jsonData = JsonUtility.FromJson<FamobiJsonData>(jsonContent);
                if (jsonData == null)
                {
                    jsonData = new FamobiJsonData();
                }
                hasUnsavedChanges = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GI] Failed to load famobi.json: {e}");
                jsonData = new FamobiJsonData();
            }
        }
    }

    private void SaveJsonFile()
    {
        if (jsonData == null)
        {
            Debug.LogError("[GI] No data to save.");
            return;
        }

        // Validate interstitial eventIds before saving
        List<string> invalidInterstitialIds = new List<string>();
        foreach (var eventId in jsonData.interstitial.eventIds)
        {
            if (!string.IsNullOrEmpty(eventId) && !IsValidInterstitialEventId(eventId))
            {
                invalidInterstitialIds.Add(eventId);
            }
        }

        // Validate rewarded eventIds before saving
        List<string> invalidRewardedIds = new List<string>();
        foreach (var eventId in jsonData.rewarded.eventIds)
        {
            if (!string.IsNullOrEmpty(eventId) && !IsValidRewardedEventId(eventId))
            {
                invalidRewardedIds.Add(eventId);
            }
        }

        if (invalidInterstitialIds.Count > 0 || invalidRewardedIds.Count > 0)
        {
            string message = "";
            if (invalidInterstitialIds.Count > 0)
            {
                message += $"Interstitial eventIds with invalid format:\n{string.Join(", ", invalidInterstitialIds)}\n\n";
                message += "Format must be \"button:$location:$action\" or \"break:$location\".\n\n";
            }
            if (invalidRewardedIds.Count > 0)
            {
                message += $"Rewarded eventIds with invalid format:\n{string.Join(", ", invalidRewardedIds)}\n\n";
                message += "Format must be \"button:$location:$action\".\n\n";
            }
            message += "Save anyway?";
            
            if (!EditorUtility.DisplayDialog("Invalid EventId Format", message, "Yes", "No"))
            {
                return; // User cancelled save
            }
        }

        try
        {
            string directory = Path.GetDirectoryName(jsonFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (var product in jsonData.iap.products)
            {
                product.priceValue = (float)Math.Round(product.priceValue, 2, MidpointRounding.AwayFromZero);
            }
            
            string jsonContent = JsonUtility.ToJson(jsonData, true);
            
            jsonContent = Regex.Replace(
                jsonContent,
                @"""priceValue"":\s*([\d.Ee\-+]+)",
                m => {
                    if (double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float, 
                        System.Globalization.CultureInfo.InvariantCulture, out double val))
                    {
                        double rounded = Math.Round(val, 2, MidpointRounding.AwayFromZero);
                        string formatted = rounded.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                        return $"\"priceValue\": {formatted}";
                    }
                    return m.Value;
                }
            );
            
            File.WriteAllText(jsonFilePath, jsonContent);
            AssetDatabase.Refresh();
            hasUnsavedChanges = false;
            Debug.Log("[GI] famobi.json saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GI] Failed to save famobi.json: {e}");
        }
    }

    private void OnGUI()
    {
        if (jsonData == null)
        {
            EditorGUILayout.HelpBox("Failed to load famobi.json. Please check the file path.", MessageType.Error);
            if (GUILayout.Button("Reload"))
            {
                LoadJsonFile();
            }
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Famobi.json Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Edit eventIds for ads and IAP products. Other fields are read-only.", MessageType.Info);
        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Interstitial EventIds
        EditorGUILayout.LabelField("Interstitial Ad EventIds", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Format: \"button:$location:$action\" (e.g., \"button:menu:start\") or \"break:$location\" (e.g., \"break:result\")", MessageType.Info);
        DrawEventIdsList(jsonData.interstitial.eventIds, "Interstitial", true);
        EditorGUILayout.Space(10);

        // Rewarded EventIds
        EditorGUILayout.LabelField("Rewarded Ad EventIds", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Format: \"button:$location:$action\" (e.g., \"button:result:restart\")", MessageType.Info);
        DrawEventIdsList(jsonData.rewarded.eventIds, "Rewarded", false);
        EditorGUILayout.Space(10);

        // IAP Products
        EditorGUILayout.LabelField("IAP Products", EditorStyles.boldLabel);
        DrawIAPProductsList();
        EditorGUILayout.Space(10);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = hasUnsavedChanges;
        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            SaveJsonFile();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Reload", GUILayout.Height(30)))
        {
            if (hasUnsavedChanges)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes. Reload anyway?", "Yes", "No"))
                {
                    LoadJsonFile();
                }
            }
            else
            {
                LoadJsonFile();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (hasUnsavedChanges)
        {
            EditorGUILayout.HelpBox("You have unsaved changes.", MessageType.Warning);
        }
    }

    private void DrawEventIdsList(List<string> eventIds, string prefix, bool isInterstitial)
    {
        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < eventIds.Count; i++)
        {
            string eventId = eventIds[i];
            bool isValid = true;
            
            if (!string.IsNullOrEmpty(eventId))
            {
                if (isInterstitial)
                {
                    isValid = IsValidInterstitialEventId(eventId);
                }
                else
                {
                    isValid = IsValidRewardedEventId(eventId);
                }
            }

            EditorGUILayout.BeginHorizontal();
            
            // Change text color if invalid
            if (!isValid)
            {
                GUI.color = Color.red;
            }
            
            eventIds[i] = EditorGUILayout.TextField($"{prefix} EventId {i + 1}", eventIds[i]);
            
            GUI.color = Color.white;
            
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                eventIds.RemoveAt(i);
                i--;
                hasUnsavedChanges = true;
            }
            EditorGUILayout.EndHorizontal();

            // Show validation message below invalid entries
            if (!isValid)
            {
                if (isInterstitial)
                {
                    EditorGUILayout.HelpBox($"Invalid format. Must be \"button:$location:$action\" or \"break:$location\"", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox($"Invalid format. Must be \"button:$location:$action\"", MessageType.Warning);
                }
            }
        }

        if (GUILayout.Button($"Add {prefix} EventId"))
        {
            eventIds.Add("");
            hasUnsavedChanges = true;
        }

        if (EditorGUI.EndChangeCheck())
        {
            hasUnsavedChanges = true;
        }
    }

    private bool IsValidInterstitialEventId(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return true; // Allow empty (user might be typing)
        }

        // Check for "button:$location:$action" format
        // Pattern: "button:" followed by non-empty location, ":", non-empty action
        if (eventId.StartsWith("button:", StringComparison.Ordinal))
        {
            string remainder = eventId.Substring(7); // Skip "button:"
            int colonIndex = remainder.IndexOf(':');
            if (colonIndex > 0 && colonIndex < remainder.Length - 1)
            {
                string location = remainder.Substring(0, colonIndex);
                string action = remainder.Substring(colonIndex + 1);
                if (!string.IsNullOrWhiteSpace(location) && !string.IsNullOrWhiteSpace(action))
                {
                    return true;
                }
            }
        }

        // Check for "break:$location" format
        // Pattern: "break:" followed by non-empty location
        if (eventId.StartsWith("break:", StringComparison.Ordinal))
        {
            string location = eventId.Substring(6); // Skip "break:"
            if (!string.IsNullOrWhiteSpace(location))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidRewardedEventId(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return true; // Allow empty (user might be typing)
        }

        // Check for "button:$location:$action" format
        // Pattern: "button:" followed by non-empty location, ":", non-empty action
        if (eventId.StartsWith("button:", StringComparison.Ordinal))
        {
            string remainder = eventId.Substring(7); // Skip "button:"
            int colonIndex = remainder.IndexOf(':');
            if (colonIndex > 0 && colonIndex < remainder.Length - 1)
            {
                string location = remainder.Substring(0, colonIndex);
                string action = remainder.Substring(colonIndex + 1);
                if (!string.IsNullOrWhiteSpace(location) && !string.IsNullOrWhiteSpace(action))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DrawIAPProductsList()
    {
        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < jsonData.iap.products.Count; i++)
        {
            var product = jsonData.iap.products[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Product {i + 1}", EditorStyles.boldLabel);
            
            product.sku = EditorGUILayout.TextField("SKU", product.sku);
            product.title = EditorGUILayout.TextField("Title", product.title);
            
            EditorGUILayout.LabelField("Description", EditorStyles.label);
            product.description = EditorGUILayout.TextArea(product.description, GUILayout.Height(50));
            
            product.imageURI = EditorGUILayout.TextField("Image URI", product.imageURI);
            
            float priceInput = EditorGUILayout.FloatField("Price Value", product.priceValue);
            // Round to max 2 decimal places as user types
            product.priceValue = (float)Math.Round(priceInput, 2, MidpointRounding.AwayFromZero);
            
            if (GUILayout.Button("Remove Product", GUILayout.Height(25)))
            {
                jsonData.iap.products.RemoveAt(i);
                i--;
                hasUnsavedChanges = true;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Add IAP Product"))
        {
            jsonData.iap.products.Add(new FamobiJsonIAPProduct());
            hasUnsavedChanges = true;
        }

        if (EditorGUI.EndChangeCheck())
        {
            hasUnsavedChanges = true;
        }
    }
}
#endif
