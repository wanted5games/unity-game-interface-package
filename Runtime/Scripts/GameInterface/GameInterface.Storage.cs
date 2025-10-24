using UnityEngine;
using System;

public partial class GameInterface
{
    public void SetStorageItem(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.SetStorageItem(key, value);
#else
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
#endif
    }

    public void SetStorageItem(string key, int value) => SetStorageItem(key, value.ToString());
    public void SetStorageItem(string key, float value) => SetStorageItem(key, value.ToString());
    public void SetStorageItem(string key, bool value) => SetStorageItem(key, value.ToString());


    public void SetStorageItem<T>(string key, T value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonUtility.ToJson(value);
        GameInterfaceBridge.SetStorageItem(key, json);
#else
        string json = JsonUtility.ToJson(value);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
#endif
    }

    // ---- GET STORAGE (Generic) ----
    public T GetStorageItem<T>(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string stored = GameInterfaceBridge.GetStorageItem(key);
#else
        string stored = PlayerPrefs.GetString(key, null);
#endif
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

            // For objects, deserialize from JSON
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
    }

    public void ClearStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.ClearStorage();
#else
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
#endif
    }
}
