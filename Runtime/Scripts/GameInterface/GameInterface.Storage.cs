using UnityEngine;
using System;
using System.Collections.Generic;

public partial class GameInterface
{
    public int MaxStorageItemBytes { get; set; } = 1024 * 1024; // 1 MB
    public int MaxTotalStorageBytes { get; set; } = 1024 * 1024; // 1 MB

    private readonly Dictionary<string, string> _storageCache = new Dictionary<string, string>();
    
    private readonly HashSet<string> _warnedIndividualOverLimit = new HashSet<string>();
    private readonly HashSet<string> _warnedIndividual50Percent = new HashSet<string>();
    private readonly HashSet<string> _warnedTotalOverLimit = new HashSet<string>();
    private readonly HashSet<string> _warnedTotal50Percent = new HashSet<string>();

    private int GetUtf8ByteCount(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        return System.Text.Encoding.UTF8.GetByteCount(value);
    }

    private void CheckStorageSize(string key, string serializedValue)
    {
        int size = GetUtf8ByteCount(serializedValue);

        int halfLimit = MaxStorageItemBytes / 2;
           
        if (size >= halfLimit && !_warnedIndividual50Percent.Contains(key))
        {
            Debug.LogWarning($"[GI] Storage: value for key '{key}' is {size} bytes ({size * 100.0f / MaxStorageItemBytes:F1}% of limit).");
            _warnedIndividual50Percent.Add(key);
        }
        
        if (size > MaxStorageItemBytes && !_warnedIndividualOverLimit.Contains(key))
        {
            Debug.LogError($"[GI] Storage: value for key '{key}' is {size} bytes, exceeding limit {MaxStorageItemBytes} bytes.");
            _warnedIndividualOverLimit.Add(key);
        }

        int currentTotal = 0;
        int oldValueSize = 0;
        
        foreach (var kvp in _storageCache)
        {
            int itemSize = GetUtf8ByteCount(kvp.Key) + GetUtf8ByteCount(kvp.Value);
            currentTotal += itemSize;
            
            if (kvp.Key == key)
            {
                oldValueSize = GetUtf8ByteCount(kvp.Value);
            }
        }

        int projectedTotal = currentTotal - oldValueSize + size;
        int halfTotalLimit = MaxTotalStorageBytes / 2;
      
        if (projectedTotal >= halfTotalLimit && !_warnedTotal50Percent.Contains(key))
        {
            Debug.LogWarning($"[GI] Storage: projected total size for key '{key}' would be {projectedTotal} bytes ({projectedTotal * 100.0f / MaxTotalStorageBytes:F1}% of limit).");
            _warnedTotal50Percent.Add(key);
        }
      
        if (projectedTotal > MaxTotalStorageBytes && !_warnedTotalOverLimit.Contains(key))
        {
            Debug.LogError($"[GI] Storage: projected total size would be {projectedTotal} bytes, exceeding limit {MaxTotalStorageBytes} bytes.");
            _warnedTotalOverLimit.Add(key);
        }
    }

    public void SetStorageItem(string key, string value)
    {
        CheckStorageSize(key, value ?? "");

#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.SetStorageItem(key, value);
#else
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
#endif
        _storageCache[key] = value ?? string.Empty;
    }

    public void SetStorageItem(string key, int value) => SetStorageItem(key, value.ToString());
    public void SetStorageItem(string key, float value) => SetStorageItem(key, value.ToString());
    public void SetStorageItem(string key, bool value) => SetStorageItem(key, value.ToString());


    public void SetStorageItem<T>(string key, T value)
    {
        string json = JsonUtility.ToJson(value);
        CheckStorageSize(key, json ?? "");

#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.SetStorageItem(key, json);
#else
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
#endif
        _storageCache[key] = json ?? string.Empty;
    }

    public T GetStorageItem<T>(string key)
    {
        string stored;
        if (!_storageCache.TryGetValue(key, out stored))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            stored = GameInterfaceBridge.GetStorageItem(key);
#else
            stored = PlayerPrefs.GetString(key, null);
#endif
            _storageCache[key] = stored ?? string.Empty;
        }
        Type targetType = typeof(T);

        if (string.IsNullOrEmpty(stored))
        {
            if (targetType == typeof(string))
                return (T)(object)"";

            if (targetType.IsValueType)
                return default;

            return Activator.CreateInstance<T>();
        }

        try
        {
            if (targetType == typeof(string))
                return (T)(object)stored;
            if (targetType == typeof(int) && int.TryParse(stored, out int intVal))
                return (T)(object)intVal;
            if (targetType == typeof(float) && float.TryParse(stored, out float floatVal))
                return (T)(object)floatVal;
            if (targetType == typeof(bool) && bool.TryParse(stored, out bool boolVal))
                return (T)(object)boolVal;

            Debug.Log($"Deserializing storage item '{key}': {stored}");
            return JsonUtility.FromJson<T>(stored);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to get storage item '{key}': {e}");
            return default;
        }
    }

    public string GetStorageItem(string key) => GetStorageItem<string>(key);
    public int GetStorageItemInt(string key) => GetStorageItem<int>(key);
    public float GetStorageItemFloat(string key) => GetStorageItem<float>(key);
    public bool GetStorageItemBool(string key) => GetStorageItem<bool>(key);

    public void RemoveItem(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.RemoveStorageItem(key);
#else
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
#endif
        if (_storageCache.ContainsKey(key)) _storageCache.Remove(key);
        
        _warnedIndividualOverLimit.Remove(key);
        _warnedIndividual50Percent.Remove(key);
        _warnedTotalOverLimit.Remove(key);
        _warnedTotal50Percent.Remove(key);
    }

    public void ClearStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.ClearStorage();
#else
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
#endif
        _storageCache.Clear();
        
        _warnedIndividualOverLimit.Clear();
        _warnedIndividual50Percent.Clear();
        _warnedTotalOverLimit.Clear();
        _warnedTotal50Percent.Clear();
    }
}
