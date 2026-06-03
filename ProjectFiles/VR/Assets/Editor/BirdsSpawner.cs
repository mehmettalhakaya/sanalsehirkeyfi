using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SampleScene'de bird.fbx modelinden 8 kus spawn eder.
/// Procedural kus'tan vazgecildi - kullanici bird.fbx istedi.
/// Her kus kendi rastgele waypoints rotasında doner (ayni davranis).
/// Auto-run. Idempotent.
/// </summary>
[InitializeOnLoad]
public static class BirdsSpawner
{
    const string BIRD_FBX = "Assets/Models/Birds/bird.fbx";
    const string BIRDS_ROOT = "BirdFlocks";
    const string VERSION_MARKER = "__v4__";

    private const string SESSION_KEY = "BirdsSpawner_RanThisSession_v4";

    static BirdsSpawner()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        if (!SessionState.GetBool(SESSION_KEY, false))
            EditorApplication.delayCall += () => CheckAndSpawn(EditorSceneManager.GetActiveScene());
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += () => CheckAndSpawn(scene);
    }

    static void CheckAndSpawn(Scene scene)
    {
        if (scene.name != "SampleScene") return;

        var existing = GameObject.Find(BIRDS_ROOT);
        if (existing != null)
        {
            if (existing.transform.Find(VERSION_MARKER) != null) return;
            Object.DestroyImmediate(existing);
        }

        SessionState.SetBool(SESSION_KEY, true);
        Spawn(scene);
    }

    static void Spawn(Scene scene)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BIRD_FBX);
        if (prefab == null)
        {
            Debug.LogWarning("[Birds] bird.fbx bulunamadi: " + BIRD_FBX);
            return;
        }
        Debug.Log("[Birds] bird.fbx yuklendi: " + prefab.name);

        var root = new GameObject(BIRDS_ROOT);
        var marker = new GameObject(VERSION_MARKER);
        marker.transform.SetParent(root.transform, false);

        var rand = new System.Random(20260524);

        int flockCount = 8;
        for (int i = 0; i < flockCount; i++)
        {
            GameObject bird = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            if (bird == null) bird = Object.Instantiate(prefab, root.transform);
            if (PrefabUtility.IsPartOfPrefabInstance(bird))
                PrefabUtility.UnpackPrefabInstance(bird, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            bird.name = $"Flock_{i:00}";

            // Random scale (FBX boyutu bilinmiyor - test ederek 0.5-1.5 arasi)
            float scale = 0.5f + (float)rand.NextDouble() * 1.0f;
            bird.transform.localScale = new Vector3(scale, scale, scale);

            // 5 waypoint kapali dongu (3-12m yukseklik, ±25m alan)
            Vector3[] waypoints = new Vector3[5];
            for (int j = 0; j < waypoints.Length; j++)
            {
                float x = ((float)rand.NextDouble() - 0.5f) * 50f;
                float z = ((float)rand.NextDouble() - 0.5f) * 50f;
                float y = 3f + (float)rand.NextDouble() * 10f;
                waypoints[j] = new Vector3(x, y, z);
            }
            bird.transform.position = waypoints[0];

            // BirdFlock script ekle (waypoint navigasyon)
            var ctrl = bird.AddComponent<BirdFlock>();
            ctrl.waypoints = waypoints;
            ctrl.speed = 4f + (float)rand.NextDouble() * 4f;
            ctrl.turnSpeed = 2f + (float)rand.NextDouble() * 2f;
            ctrl.waypointReachDistance = 1.5f;
            ctrl.flapSpeed = 10f;
            ctrl.flapAngle = 35f;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Birds] {flockCount} bird.fbx instance spawn edildi.");
    }
}
