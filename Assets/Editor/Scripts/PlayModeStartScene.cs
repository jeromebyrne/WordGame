using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class PlayModeStartScene
{
    private const string PreviousScenePathKey = "PreviousScenePath";

    static PlayModeStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Save the current scene path before switching to the GameStart scene
            string previousScenePath = EditorSceneManager.GetActiveScene().path;
            EditorPrefs.SetString(PreviousScenePathKey, previousScenePath);

            // Change this to the path of your GameStart scene
            string gameStartScenePath = "Assets/Scenes/GameStart.unity";

            if (EditorSceneManager.GetActiveScene().path != gameStartScenePath)
            {
                bool saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                if (saved)
                {
                    EditorSceneManager.OpenScene(gameStartScenePath);
                }
                else
                {
                    // User canceled the save operation
                    EditorApplication.isPlaying = false;
                }
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // When exiting play mode, return to the previous scene
            string previousScenePath = EditorPrefs.GetString(PreviousScenePathKey, string.Empty);
            if (!string.IsNullOrEmpty(previousScenePath) && EditorSceneManager.GetActiveScene().path != previousScenePath)
            {
                EditorSceneManager.OpenScene(previousScenePath);
                EditorPrefs.DeleteKey(PreviousScenePathKey);
            }
        }
    }
}