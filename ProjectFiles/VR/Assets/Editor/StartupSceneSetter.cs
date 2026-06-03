using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Unity Editor acildiginda MainMenu sahnesini otomatik acar.
/// Ayrica Play'e basildiginda her zaman MainMenu'dan baslar (hangi sahne acik olursa olsun).
///
/// SessionState ile sadece Unity'nin ILK acilisinda calisir - sonraki recompile'larda calismaz.
/// </summary>
[InitializeOnLoad]
public static class StartupSceneSetter
{
    private const string MAIN_MENU_PATH = "Assets/Scenes/MainMenu.unity";
    private const string SESSION_KEY = "StartupSceneSetter_AlreadyRan";

    static StartupSceneSetter()
    {
        // Bu Unity session'inda zaten calistiysa atla (recompile'larda tetiklenme)
        if (SessionState.GetBool(SESSION_KEY, false)) return;
        SessionState.SetBool(SESSION_KEY, true);

        EditorApplication.delayCall += SetupStartupScene;
    }

    static void SetupStartupScene()
    {
        // 1) Play Mode Start Scene = MainMenu (Play her zaman buradan baslar)
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MAIN_MENU_PATH);
        if (sceneAsset != null)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
            Debug.Log("[StartupScene] Play Mode Start Scene = MainMenu ayarlandi.");
        }
        else
        {
            Debug.LogWarning($"[StartupScene] {MAIN_MENU_PATH} bulunamadi!");
            return;
        }

        // 2) Eger su an SampleScene veya baska sahne acik ise MainMenu'ya gec
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.path == MAIN_MENU_PATH) return;  // zaten acik
        if (currentScene.isDirty) return;                  // kullanici kaydetmemis, dokunma

        bool opened = EditorSceneManager.OpenScene(MAIN_MENU_PATH, OpenSceneMode.Single) != null;
        if (opened) Debug.Log("[StartupScene] MainMenu sahnesi acildi.");
    }
}
