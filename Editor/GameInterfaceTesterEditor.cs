#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameInterfaceTester))]
public class GameInterfaceTesterEditor : Editor
{
    private SerializedObject so;
    private bool featuresFoldout = true;

    public override void OnInspectorGUI()
    {
        if (target == null) return;

        if (so == null)
            so = new SerializedObject(target);

        so.Update();

        // Features foldout (names fixed, only toggles editable)
        featuresFoldout = EditorGUILayout.Foldout(featuresFoldout, "Features", true);
        if (featuresFoldout)
        {
            EditorGUI.indentLevel++;
            DrawFeatureToggle("audio", "Audio");
            DrawFeatureToggle("copyright", "Copyright");
            DrawFeatureToggle("credits", "Credits");
            DrawFeatureToggle("iap", "IAP");
            DrawFeatureToggle("pause", "Pause");
            DrawFeatureToggle("privacy", "Privacy");
            DrawFeatureToggle("progress", "Progress");
            DrawFeatureToggle("rewarded", "Rewarded");
            DrawFeatureToggle("score", "Score");
            DrawFeatureToggle("tutorial", "Tutorial");
            DrawFeatureToggle("version", "Version");
            DrawFeatureToggle("visibilitychange", "Visibility Change");
            EditorGUI.indentLevel--;
        }

        // Core fields
        EditorGUILayout.PropertyField(so.FindProperty("_rewardedAdAvailable"), new GUIContent("Rewarded Ad Available"));
        EditorGUILayout.PropertyField(so.FindProperty("_isRewardGranted"), new GUIContent("Is Reward Granted"));
        EditorGUILayout.PropertyField(so.FindProperty("_isMuted"), new GUIContent("Is Muted"));
        EditorGUILayout.PropertyField(so.FindProperty("_isPaused"), new GUIContent("Is Paused"));
        EditorGUILayout.PropertyField(so.FindProperty("_offsets"), true);
        EditorGUILayout.PropertyField(so.FindProperty("_logoUrl"), new GUIContent("Logo URL"));
        EditorGUILayout.PropertyField(so.FindProperty("_language"), new GUIContent("Language"));

        EditorGUILayout.Space();



        so.ApplyModifiedProperties();
    }

    private void DrawFeatureToggle(string propertyName, string label)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(label));
        }
    }
}
#endif


