#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class GameInterfaceTesterWindow : EditorWindow
{
    [MenuItem("Game Interface/Tester")]
    public static void Open() => GetWindow<GameInterfaceTesterWindow>("Game Interface Tester");

    public static readonly string PREF_KEY = "GameInterfaceTesterWindow_LastGUID";
    public static readonly string PREF_PATH = "Packages/com.famobi.game-interface/Samples/Settings/GameInterfaceTester.asset";

    private GameInterfaceTester settings;
    private SerializedObject serializedSettings;
    private Vector2 scrollPos;
    private bool featuresFoldout = true;

    private void OnEnable()
    {
        // Try to restore last assigned asset from EditorPrefs
        string guid = EditorPrefs.GetString(PREF_KEY, "");

        string path = !string.IsNullOrEmpty(guid)
            ? AssetDatabase.GUIDToAssetPath(guid) : PREF_PATH;

        settings = AssetDatabase.LoadAssetAtPath<GameInterfaceTester>(path);

        if (settings == null && path != PREF_PATH) {
            path = PREF_PATH;
            settings = AssetDatabase.LoadAssetAtPath<GameInterfaceTester>(path);
        }

        if (settings)
            serializedSettings = new SerializedObject(settings);
    }

    private void OnGUI()
    {
        // Begin scroll view
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Game Interface Tester", EditorStyles.boldLabel);

        string _path = settings ? AssetDatabase.GetAssetPath(settings) : "";

        if (string.IsNullOrEmpty(_path))
            _path = "No asset assigned.";

        EditorGUILayout.LabelField("Asset Path:", _path);
        EditorGUILayout.Space();

        // Asset assignment field
        EditorGUI.BeginChangeCheck();
        var newSettings = (GameInterfaceTester)EditorGUILayout.ObjectField("Settings Asset", settings, typeof(GameInterfaceTester), false);
        if (EditorGUI.EndChangeCheck())
        {
            settings = newSettings;
            serializedSettings = settings ? new SerializedObject(settings) : null;

            if (settings)
            {
                string path = AssetDatabase.GetAssetPath(settings);
                string guid = AssetDatabase.AssetPathToGUID(path);
                EditorPrefs.SetString(PREF_KEY, guid); // Persist GUID
            }
            else
            {
                EditorPrefs.DeleteKey(PREF_KEY);
            }
        }

        if (settings == null)
        {
            EditorGUILayout.HelpBox("No GameInterfaceTester assigned.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }

        // Draw serialized fields
        if (serializedSettings != null)
        {
            serializedSettings.Update();

            // Features foldout: draw new individual feature toggles
            featuresFoldout = EditorGUILayout.Foldout(featuresFoldout, "Features", true);
            if (featuresFoldout)
            {
                EditorGUI.indentLevel++;
                DrawFeatureToggle(serializedSettings, "audio", "Audio");
                DrawFeatureToggle(serializedSettings, "copyright", "Copyright");
                DrawFeatureToggle(serializedSettings, "credits", "Credits");
                DrawFeatureToggle(serializedSettings, "iap", "IAP");
                DrawFeatureToggle(serializedSettings, "pause", "Pause");
                DrawFeatureToggle(serializedSettings, "privacy", "Privacy");
                DrawFeatureToggle(serializedSettings, "progress", "Progress");
                DrawFeatureToggle(serializedSettings, "rewarded", "Rewarded");
                DrawFeatureToggle(serializedSettings, "score", "Score");
                DrawFeatureToggle(serializedSettings, "tutorial", "Tutorial");
                DrawFeatureToggle(serializedSettings, "version", "Version");
                DrawFeatureToggle(serializedSettings, "visibilitychange", "Visibility Change");
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("_rewardedAdAvailable"), new GUIContent("Rewarded Ad Available"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("_isRewardGranted"), new GUIContent("Is Reward Granted"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("_isMuted"), new GUIContent("Is Muted"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("_isPaused"), new GUIContent("Is Paused"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("_offsets"), true);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("_logoUrl"), new GUIContent("Logo URL"));

            SerializedProperty langProp = serializedSettings.FindProperty("_language");
            EditorGUILayout.PropertyField(langProp, new GUIContent("Language"));

            serializedSettings.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(settings);
        }

        // Buttons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        DrawButtons(settings);

        EditorGUILayout.EndScrollView();
    }

    private void DrawFeatureToggle(SerializedObject so, string propertyName, string label)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(label));
        }
    }

    private void DrawButtons(GameInterfaceTester target)
    {
        if (target == null) return;

        var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            if (method.GetCustomAttributes(typeof(ContextMenu), true).Length > 0)
            {
                if (method.Name == "GoToLevel")
                {
                    EditorGUILayout.BeginHorizontal();
                    // Flexible button
                    if (GUILayout.Button("Go To Level", GUILayout.ExpandWidth(true)))
                    {
                        method.Invoke(target, null);
                        EditorUtility.SetDirty(target);
                    }
                    // Integer field fixed width, aligned right
                    target.targetLevel = EditorGUILayout.IntField(target.targetLevel, GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button(method.Name))
                    {
                        method.Invoke(target, null);
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }
    }
}
#endif
