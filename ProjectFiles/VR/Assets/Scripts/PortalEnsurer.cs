using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ana sahne yuklendiginde PyramidPortalTrigger varligi garanti edilir.
/// Yoksa runtime'da otomatik olarak kurar.
/// Sebep: bazen ic sahneden donunce ya da ilk acilista trigger eksik kalabiliyor.
/// </summary>
public static class PortalEnsurer
{
    private const string MAIN_SCENE_NAME = "SampleScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckAndSetup(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndSetup(scene);
    }

    static void CheckAndSetup(Scene scene)
    {
        if (scene.name != MAIN_SCENE_NAME) return;
        // Coroutine yerine duz cagri yeterli - bir frame sonra aktif olur
        EnsurePortal();
    }

    static void EnsurePortal()
    {
        // Zaten varsa atla
        if (GameObject.Find("PyramidPortalTrigger") != null)
        {
            Debug.Log("[PortalEnsurer] Portal zaten mevcut.");
            return;
        }

        // Louvre objesini bul
        var louvre = GameObject.Find("Louvre");
        if (louvre == null)
        {
            Debug.LogWarning("[PortalEnsurer] Louvre objesi yok, portal kurulamadi.");
            return;
        }

        // Piramit mesh'ini bul - once isimle
        MeshRenderer pyramid = null;
        var renderers = louvre.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in renderers)
        {
            string n = mr.gameObject.name.ToLower();
            if (n.Contains("pyramid") || n.Contains("piramit") || n.Contains("pyramide"))
            {
                pyramid = mr;
                Debug.Log($"[PortalEnsurer] Piramit isimden bulundu: {mr.gameObject.name}");
                break;
            }
        }

        // Isimle bulunamadiysa boyut filtreli ara
        if (pyramid == null && renderers.Length > 0)
        {
            Bounds louvreBounds = renderers[0].bounds;
            foreach (var mr in renderers) louvreBounds.Encapsulate(mr.bounds);
            Vector3 center = louvreBounds.center;

            MeshRenderer best = null;
            float bestScore = float.MaxValue;
            foreach (var mr in renderers)
            {
                Vector3 sz = mr.bounds.size;
                if (sz.x < 15f || sz.x > 50f) continue;
                if (sz.z < 15f || sz.z > 50f) continue;
                if (sz.y < 8f || sz.y > 30f) continue;
                float ratio = Mathf.Abs(sz.x - sz.z) / Mathf.Max(sz.x, sz.z);
                if (ratio > 0.3f) continue;
                if (sz.y > (sz.x + sz.z) * 0.5f) continue;
                float distXZ = new Vector2(mr.bounds.center.x - center.x, mr.bounds.center.z - center.z).magnitude;
                float score = distXZ + Mathf.Abs(sz.x - 35f) + Mathf.Abs(sz.y - 22f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = mr;
                }
            }
            pyramid = best;
            if (pyramid != null)
                Debug.Log($"[PortalEnsurer] Piramit boyut filtresi ile bulundu: {pyramid.gameObject.name}, sz={pyramid.bounds.size}");
        }

        if (pyramid == null)
        {
            Debug.LogWarning("[PortalEnsurer] Piramit bulunamadi, portal kurulamadi.");
            return;
        }

        // Portal trigger GameObject'i kur
        Vector3 pyramidCenter = pyramid.bounds.center;
        Vector3 pyramidSize = pyramid.bounds.size;

        var triggerGO = new GameObject("PyramidPortalTrigger");
        triggerGO.transform.position = pyramidCenter;

        var portal = triggerGO.AddComponent<PyramidPortal>();
        portal.targetSceneName = "LouvreInteriorOptimized";
        portal.promptDistance = 0.05f;
        portal.usePyramidBounds = true;
        portal.useXZOnly = true;
        portal.pyramidCenter = pyramidCenter;
        portal.pyramidSize = pyramidSize;

        Debug.Log($"[PortalEnsurer] Portal runtime'da kuruldu - piramit merkez={pyramidCenter}, boyut={pyramidSize}");
    }
}
