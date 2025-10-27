using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GameInterface
{
    /// <summary>
    /// Games must track helpful data for optimization. For more information, see GameAnalytics documentation.
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    public void Track(GAEventType eventType, Dictionary<string, object>? data = null)
    {
        string json = data != null ? JsonUtility.ToJson(new SerializableDictionary(data)) : "{}";
        string eventName = GAEventTypeToString(eventType);

#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.Track(eventName, json);
#else
        Debug.Log($"[GameInterface Tester] Track called with event: {eventName}, data: {json}");
#endif
    }

    /// <summary>
    /// Games must track helpful data for optimization. For more information, see GameAnalytics documentation.
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Track(Dictionary<string, object> data)
    {
        // Ensure "event" key exists and is a GAEventType
        if (!data.TryGetValue("event", out var eventValue))
        {
            throw new ArgumentException("The data dictionary must contain an 'event' key of type GAEventType.");
        }

        if (eventValue is not GAEventType gaEvent)
        {
            throw new ArgumentException("'event' must be of type GAEventType.");
        }

        // Convert GAEventType to string
        string eventName = GAEventTypeToString(gaEvent);

        if (eventName == null)
        {
            throw new ArgumentException("Invalid GAEventType provided in 'event' key.");
        }
        data["event"] = eventName; // Replace enum with string for serialization

        string json = data != null ? JsonUtility.ToJson(new SerializableDictionary(data)) : "{}";

#if UNITY_WEBGL && !UNITY_EDITOR
        GameInterfaceBridge.Track(json, null);
#else
        Debug.Log($"[GameInterface Tester] Track called with event: {eventName}, data: {json}");
#endif
    }

    private string GAEventTypeToString(GAEventType eventType)
    {
        return eventType switch
        {
            GAEventType.BUSINESS => "GA:Business",
            GAEventType.PROGRESSION => "GA:Progression",
            GAEventType.RESOURCE => "GA:Resource",
            GAEventType.ERROR => "GA:Error",
            GAEventType.DESIGN => "GA:Design",

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // Helper class to serialize a dictionary to JSON
    [System.Serializable]
    public class SerializableDictionary
    {
        public string[] keys;
        public string[] values;

        public SerializableDictionary(System.Collections.Generic.Dictionary<string, object> dict)
        {
            keys = new string[dict.Count];
            values = new string[dict.Count];
            int i = 0;
            foreach (var kvp in dict)
            {
                keys[i] = kvp.Key;
                values[i] = kvp.Value?.ToString() ?? "";
                i++;
            }
        }
    }
}


public enum GAEventType
{
    BUSINESS,
    PROGRESSION,
    RESOURCE,
    ERROR,
    DESIGN,
}

