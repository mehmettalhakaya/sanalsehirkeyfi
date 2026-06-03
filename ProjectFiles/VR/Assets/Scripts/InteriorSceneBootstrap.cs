using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LouvreInteriorOptimized sahnesi yuklendiginde OTOMATIK olarak
/// bir GameObject ekler ve ona ReturnPortal scripti baglar.
/// ReturnPortal.Start() Player + Camera + HumanoidController + UI kurar.
/// Boylece kullanici ekstra setup yapmaz, sahne degisir degismez gezilebilir.
/// </summary>
public static class InteriorSceneBootstrap
{
    private const string INTERIOR_SCENE_NAME = "LouvreInteriorOptimized";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Game basladiginda mevcut sahneyi de kontrol et
        CheckAndSetup(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndSetup(scene);
    }

    static void CheckAndSetup(Scene scene)
    {
        if (scene.name != INTERIOR_SCENE_NAME) return;

        // Mevcut Player varsa atla
        if (GameObject.Find("Player") != null)
        {
            Debug.Log("[InteriorBootstrap] Player zaten var, atla.");
            return;
        }

        // ReturnPortal'i tasiyan bos bir bootstrap obje yarat
        var bootstrap = GameObject.Find("_InteriorBootstrap");
        if (bootstrap == null)
        {
            bootstrap = new GameObject("_InteriorBootstrap");
        }

        if (bootstrap.GetComponent<ReturnPortal>() == null)
        {
            var rp = bootstrap.AddComponent<ReturnPortal>();
            rp.mainSceneName = "Outside_Museum_VR";
            rp.readFromPrefs = true;
            rp.autoCreateButton = true;
            rp.autoSetupPlayer = true;
        }

        Debug.Log("[InteriorBootstrap] Ic sahne kuruldu - Player + ReturnPortal aktif.");
    }
}
