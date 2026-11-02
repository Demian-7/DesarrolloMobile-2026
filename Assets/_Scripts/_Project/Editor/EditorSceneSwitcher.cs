using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EditorSceneSwitcher
{
    [MenuItem("Tools/Scenes/Gameplay &1")]
    public static void OpenMainMenu() => LoadBuildScene("ResolutionTest");

    [MenuItem("Tools/Scenes/Old Gameplay &2")]
    public static void OpenLobby() => LoadBuildScene("GameplayScreen");

    private static void LoadBuildScene(string sceneName)
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Exit Play mode to switch scenes via shortcuts.");
            return;
        }

        // Prompts to save any unsaved work before switching
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) 
        {
            return;
        }

        // Look up the exact path from Build Settings to avoid hitting stale backup scenes
        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            if (buildScene.path.EndsWith($"/{sceneName}.unity"))
            {
                EditorSceneManager.OpenScene(buildScene.path);
                return;
            }
        }

        Debug.LogError($"Scene '{sceneName}' not found in Build Settings. Please add it via File > Build Settings.");
    }
}