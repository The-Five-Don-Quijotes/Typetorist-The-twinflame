using UnityEngine;
using UnityEditor;
using TMPro;

public class FontChangerEditor : EditorWindow
{
    private TMP_FontAsset newFont;

    // Creates a new menu item under "Tools/TMP Font Changer"
    [MenuItem("Tools/TMP Font Changer")]
    public static void ShowWindow()
    {
        GetWindow<FontChangerEditor>("TMP Font Changer");
    }

    private void OnGUI()
    {
        // Window UI
        GUILayout.Label("Assign the new Font Asset to replace all fonts", EditorStyles.boldLabel);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace All Fonts in Project"))
        {
            if (newFont == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a new Font Asset first.", "OK");
                return;
            }

            // Find all prefabs in the project
            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefabGUID in allPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGUID);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UpdateFontsInGameObject(prefab);
            }

            // Find and update all scenes
            string[] allScenes = AssetDatabase.FindAssets("t:Scene");
            foreach (string sceneGUID in allScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);

                // Find all TextMeshPro components in the open scene
                TextMeshProUGUI[] textsInSceneUI = FindObjectsOfType<TextMeshProUGUI>();
                foreach (var text in textsInSceneUI)
                {
                    text.font = newFont;
                    EditorUtility.SetDirty(text); // Mark the object as "dirty" to ensure changes are saved
                }

                TextMeshPro[] textsInScene3D = FindObjectsOfType<TextMeshPro>();
                foreach (var text in textsInScene3D)
                {
                    text.font = newFont;
                    EditorUtility.SetDirty(text);
                }

                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            }

            EditorUtility.DisplayDialog("Success", $"All TextMeshPro fonts have been changed to {newFont.name}!", "OK");
        }
    }

    private void UpdateFontsInGameObject(GameObject obj)
    {
        // Get all TextMeshPro components in the object and its children
        TextMeshProUGUI[] textsUI = obj.GetComponentsInChildren<TextMeshProUGUI>(true); // true includes inactive objects
        foreach (var text in textsUI)
        {
            text.font = newFont;
            EditorUtility.SetDirty(text);
        }

        TextMeshPro[] texts3D = obj.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var text in texts3D)
        {
            text.font = newFont;
            EditorUtility.SetDirty(text);
        }
    }
}