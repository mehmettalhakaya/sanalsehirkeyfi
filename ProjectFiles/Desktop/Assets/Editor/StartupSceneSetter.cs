using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Unity Editor acildiginda ve Play durdugunda MainMenu sahnesine doner.
/// Ayrica Play'e basildiginda her zaman MainMenu'dan baslar.
/// </summary>
[InitializeOnLoad]
public static class StartupSceneSetter
{
    private const string MAIN_MENU_PATH = "Assets/Scenes/MainMenu.unity";

    static StartupSceneSetter()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        EditorApplication.delayCall += SetupStartupScene;
    }

    static void SetupStartupScene()
    {
        if (!SetPlayModeStartScene())
            return;

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            OpenMainMenuIfNeeded(false);
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += ReturnToMainMenuAfterPlay;
    }

    static void ReturnToMainMenuAfterPlay()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        SetPlayModeStartScene();
        OpenMainMenuIfNeeded(true);
    }

    static bool SetPlayModeStartScene()
    {
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MAIN_MENU_PATH);
        if (sceneAsset == null)
        {
            Debug.LogWarning($"[StartupScene] {MAIN_MENU_PATH} bulunamadi!");
            return false;
        }

        if (EditorSceneManager.playModeStartScene != sceneAsset)
            EditorSceneManager.playModeStartScene = sceneAsset;

        return true;
    }

    static void OpenMainMenuIfNeeded(bool force)
    {
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.path == MAIN_MENU_PATH)
            return;

        if (!force && currentScene.isDirty)
            return;

        bool opened = EditorSceneManager.OpenScene(MAIN_MENU_PATH, OpenSceneMode.Single) != null;
        if (opened) Debug.Log("[StartupScene] MainMenu sahnesi acildi.");
    }
}
