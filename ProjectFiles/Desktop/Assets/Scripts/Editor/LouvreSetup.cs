#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// LOUVRE SAHNESI OTOMATIK KURULUMU (v3 - Komple)
/// ================================================
/// Unity menusunden "Louvre > Sahneyi Kur (Otomatik)" sec, hepsi olusur:
///   - Player GameObject (CharacterController ile)
///     * MainCamera child olarak (goz yuksekliginde)
///     * PlayerController (WASD, mouse, ucus modu)
///     * PlayerInteraction (raycast tiklama)
///     * TeleportController (sag tik teleport)
///   - Canvas (UI Scale ile responsive)
///     * InfoPanel
///     * SettingsMenu (ses, grafik, mouse hassasiyeti)
///     * NavigationHUD (kontroller yardimi)
///   - Louvre modeline collider'lar
///   - 8 onemli yapida InteractableObject
///   - Tum baglantilar otomatik
/// </summary>
public class LouvreSetup
{
    private const string MENU_ROOT = "Louvre";

    // [MenuItem(MENU_ROOT + "/0) SIFIRDAN BASLAT (Minimal)", priority = -100)]  // gizli
    public static void StartFromScratch()
    {
        Debug.Log("[Louvre] SIFIRDAN BASLAT v2 - HER SEY temizleniyor, orijinal model kullanilacak...");

        // 1) SAHNEDEKI HER SEYI SIL (Louvre dahil)
        var allRoots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in allRoots)
        {
            Object.DestroyImmediate(go);
        }

        // 2) FBX import settings'i SIFIRLA (Decimate gormeyen Louvre_Original tercih edilecek)
        var modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" });
        foreach (var guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.isReadable = true;
            importer.optimizeMeshPolygons = false;
            importer.optimizeMeshVertices = false;
            importer.weldVertices = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.Import;
            importer.generateSecondaryUV = false;
            importer.SaveAndReimport();
            Debug.Log($"  -> {path}: import RESET (Mesh Compression Off, normals Import, tangents Import)");
        }

        // 3) Material instancing'i kapat
        var matGuids = AssetDatabase.FindAssets("t:Material");
        foreach (var guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) mat.enableInstancing = false;
        }
        AssetDatabase.SaveAssets();

        // 4) LOUVRE_ORIGINAL'i sahneye ekle (Optimize edilmis Louvre_Decimate'den degil)
        GameObject louvreInstance = null;
        string originalPath = "Assets/Models/Louvre_Original.fbx";
        var originalAsset = AssetDatabase.LoadAssetAtPath<GameObject>(originalPath);
        if (originalAsset != null)
        {
            louvreInstance = (GameObject)PrefabUtility.InstantiatePrefab(originalAsset);
            louvreInstance.name = "Louvre";
            Debug.Log($"  -> Louvre_Original sahneye eklendi");
        }
        else
        {
            // Yedek: Optimized
            string optPath = "Assets/Models/Louvre_Optimized.fbx";
            var optAsset = AssetDatabase.LoadAssetAtPath<GameObject>(optPath);
            if (optAsset != null)
            {
                louvreInstance = (GameObject)PrefabUtility.InstantiatePrefab(optAsset);
                louvreInstance.name = "Louvre";
                Debug.LogWarning($"  -> Louvre_Original yok, Louvre_Optimized kullaniliyor");
            }
        }
        if (louvreInstance != null)
        {
            louvreInstance.transform.position = Vector3.zero;
            louvreInstance.transform.rotation = Quaternion.identity;
            louvreInstance.transform.localScale = Vector3.one;
        }

        // 5) Bounds hesapla
        Bounds bounds = ComputeSceneBounds();
        float diag = bounds.size.magnitude;

        // 5b) ONCE MeshCollider'lari ekle ki raycast zemini bulabilsin
        if (louvreInstance != null)
        {
            BindLouvreTextures(louvreInstance);

            int colliderCount = 0;
            var meshRenderers = louvreInstance.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in meshRenderers)
            {
                if (mr.GetComponent<Collider>() == null)
                {
                    var mc = mr.gameObject.AddComponent<MeshCollider>();
                    mc.convex = false;
                    colliderCount++;
                }
            }
            Debug.Log($"  -> {colliderCount} Louvre mesh'ine MeshCollider eklendi (carpisma)");

            // Physics sync - eklenen collider'lar bu frame'de aktif olsun
            Physics.SyncTransforms();
        }

        // 6) INSANSI Player - Y tam LouvreGround plane uzerine (ayaklar yapisik)
        float targetX = 0.2314512f;
        float targetZ = 19.23787f;
        // LouvreGround plane Y = bounds.min.y - 0.02 (BuildLouvreGround icinde boyle)
        // Player feet TAM bu noktada => yere yapisik
        float targetY = bounds.min.y - 0.02f;
        Vector3 startPos = new Vector3(targetX, targetY, targetZ);
        Debug.Log($"  -> Player pos: {startPos} (Y = LouvreGround tam ustu, ayaklar yapisik)");

        var playerGO = new GameObject("Player");
        playerGO.transform.position = startPos;
        playerGO.transform.rotation = Quaternion.Euler(0, 180f, 0);
        // KULLANICI ISTEGI: dis sahnede scale Y = 0
        playerGO.transform.localScale = new Vector3(1f, 0f, 1f);

        // Insansi boy (1.85m)
        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.85f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.925f, 0);
        cc.skinWidth = 0.05f;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        // Main Camera AYRI top-level obje (scale Y=0 oldugu icin Player parent OLAMAZ)
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        // standCameraY = 0 oldugu icin kamera Player'in pos'unda
        camGO.transform.position = startPos;
        camGO.transform.rotation = playerGO.transform.rotation;

        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = Mathf.Max(diag * 5f, 5000f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.45f, 0.47f, 1f);
        camGO.AddComponent<AudioListener>();

        // HumanoidController world-space camera mode (LateUpdate kamerayi takip ettirir)
        var humanoid = playerGO.AddComponent<HumanoidController>();
        humanoid.cameraTransform = camGO.transform;
        humanoid.useWorldSpaceCamera = true;

        // 7a) Profesyonel tas zemin (Sketchfab tarzi, otomatik)
        BuildLouvreGround(bounds);

        // 7) Light kontrol
        EnsureDirectionalLight();

        // 8) PIRAMIT PORTAL'I OTOMATIK KUR
        AutoSetupPyramidPortal(louvreInstance);

        // 11) Sahneyi kaydet - eger Untitled ise SampleScene yoluna kaydet
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        if (string.IsNullOrEmpty(activeScene.path))
        {
            string scenesDir = "Assets/Scenes";
            if (!System.IO.Directory.Exists(scenesDir))
                System.IO.Directory.CreateDirectory(scenesDir);
            EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/SampleScene.unity");
        }
        else
        {
            EditorSceneManager.SaveScene(activeScene);
        }

        Debug.Log($"[Louvre] SIFIRDAN v5 (Humanoid + Portal) tamamlandi. Player: {startPos}, bounds: {bounds.size}");
        EditorUtility.DisplayDialog("SIFIRDAN BASLAT v5 - Humanoid Mode",
            "Sahne TAMAMEN sifirlandi.\n\n" +
            "SAHNE ICERIGI:\n" +
            "  - Louvre (orijinal model + doku bagli)\n" +
            "  - LouvreGround (gri tas dosseme zemin)\n" +
            "  - Directional Light\n" +
            "  - Player (insansi - boy 1.35m)\n" +
            "  - Main Camera (Player altinda, goz hizasi 1.25m)\n" +
            "  - PyramidPortalTrigger (Muzeye gir prompt)\n\n" +
            "KONTROLLER (Play):\n" +
            "  WASD     - Yuru\n" +
            "  Shift    - Kos\n" +
            "  Space    - Zipla (30cm)\n" +
            "  Ctrl/C   - Egil (boy 1.85 -> 0.9, neredeyse yari)\n" +
            "  Mouse    - Bak\n" +
            "  ESC      - Cursor kilidini ac/kapat\n\n" +
            "PIRAMIT PORTALI:\n" +
            "  Cam piramide 0.05m yaklas → 'Muzeye Gir (ESC)' butonu cikar\n" +
            "  ESC tusu (ya da butona tikla) → ic sahneye gec\n" +
            "  Ic sahnede ESC ya da sol ust 'Disari Cik' butonu → geri don\n\n" +
            "OTOMATIK SISTEMLER:\n" +
            "  - PlayerGroundSnapper: her sahnede ayaklari zemine yapistirir\n" +
            "  - LouvreTextureRebinder: sahne degisince dokuyu tekrar baglar\n" +
            "  - PortalEnsurer: portal eksikse runtime'da otomatik kurar",
            "Tamam");
    }

    /// <summary>
    /// Louvre Cour Napoleon stilinde bej tas dosseme zemin olusturur.
    /// Modelin altinda, tum cevresinde 1.5x model boyutu kadar genis bir alan.
    /// </summary>
    // [MenuItem(MENU_ROOT + "/P) Piramit Portal Kur (Ic sahneye gecis)")]  // gizli
    public static void SetupPyramidPortal()
    {
        Debug.Log("[Louvre] Piramit portal kurulumu basliyor...");

        // 1) Build Settings'e sahneleri ekle
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        string mainScene = "Assets/Scenes/SampleScene.unity";
        string interiorScene = "Assets/Scenes/LouvreInteriorOptimized.unity";
        if (System.IO.File.Exists(mainScene))
            sceneList.Add(new EditorBuildSettingsScene(mainScene, true));
        if (System.IO.File.Exists(interiorScene))
            sceneList.Add(new EditorBuildSettingsScene(interiorScene, true));
        EditorBuildSettings.scenes = sceneList.ToArray();
        Debug.Log($"  -> Build Settings'e {sceneList.Count} sahne eklendi");

        // 2) Sahnedeki en buyuk cam piramit'i bul (Louvre'un alt mesh'leri arasinda)
        var louvre = GameObject.Find("Louvre");
        if (louvre == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede 'Louvre' objesi yok. Once SIFIRDAN BASLAT calistir.", "Tamam");
            return;
        }

        // En buyuk pyramid-like mesh'i bul - genelde "pyramid" veya "Pyramid" iceren
        MeshRenderer pyramid = null;
        var renderers = louvre.GetComponentsInChildren<MeshRenderer>();
        // Once isimde pyramid arayalim
        foreach (var mr in renderers)
        {
            if (mr.gameObject.name.ToLower().Contains("pyramid") ||
                mr.gameObject.name.ToLower().Contains("piramit"))
            {
                pyramid = mr;
                break;
            }
        }
        // Bulamadiysak modelin merkezindeki kucuk mesh'i bul (ortadaki cam piramit)
        if (pyramid == null)
        {
            Bounds louvreBounds = louvre.GetComponentsInChildren<MeshRenderer>()[0].bounds;
            foreach (var mr in renderers) louvreBounds.Encapsulate(mr.bounds);
            Vector3 center = louvreBounds.center;
            float minDist = float.MaxValue;
            foreach (var mr in renderers)
            {
                // Kucuk obje, merkeze yakin = piramit
                float size = mr.bounds.size.magnitude;
                if (size > 60f || size < 5f) continue;
                float dist = Vector3.Distance(mr.bounds.center, center);
                if (dist < minDist)
                {
                    minDist = dist;
                    pyramid = mr;
                }
            }
        }

        if (pyramid == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "Piramit otomatik bulunamadi.\n\n" +
                "Hierarchy'de cam piramit objesini sec, Inspector'da Add Component -> PyramidPortal ekle. " +
                "Cam Piramit'e tiklayinca ic sahneye gecsin.",
                "Tamam");
            return;
        }

        // 3) Piramide MeshCollider + PyramidPortal ekle
        if (pyramid.GetComponent<Collider>() == null)
        {
            var mc = pyramid.gameObject.AddComponent<MeshCollider>();
            mc.convex = true;  // tiklama icin convex iyi
        }
        var existing = pyramid.GetComponent<PyramidPortal>();
        if (existing == null)
        {
            var portal = pyramid.gameObject.AddComponent<PyramidPortal>();
            portal.targetSceneName = "LouvreInteriorOptimized";
            portal.triggerDistance = 2f;
            portal.promptMessage = "Cam piramide geldiniz.\nLouvre'un icine girmek ister misiniz?";
        }
        Debug.Log($"  -> Piramit secildi: {pyramid.gameObject.name} (Size: {pyramid.bounds.size.magnitude:F1}m)");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Piramit Portal Hazir",
            $"Cam piramide tiklayinca ic sahneye gec.\n\n" +
            $"PIRAMIT: {pyramid.gameObject.name}\n" +
            "GECIS YONTEMLERI:\n" +
            "  - Sol tik (uzaktan)\n" +
            "  - Yaklas + E tusu\n\n" +
            "Ic sahnede:\n" +
            "  - Sol ust 'Disari Cik' butonu (otomatik olusur)\n" +
            "  - Esc tusu\n\n" +
            "Once ic sahneye gec: File > Open Scene > LouvreInteriorOptimized.unity\n" +
            "Bos bir GameObject ekle, ona ReturnPortal scripti ekle.",
            "Tamam");
    }

    // [MenuItem(MENU_ROOT + "/A) Zemin Ekle (Cour Napoleon)")]  // gizli - StartFromScratch otomatik yapiyor
    public static void AddGround()
    {
        // Mevcut zemin varsa once temizle
        RemoveGroundSilent();
        Bounds bounds = ComputeSceneBounds();
        BuildLouvreGround(bounds);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Zemin Eklendi",
            "Louvre Cour Napoleon stili tas zemin sahneye eklendi.\n\n" +
            "Kaldirmak istersen: Louvre > B) Zemin Kaldir",
            "Tamam");
    }

    // [MenuItem(MENU_ROOT + "/B) Zemin Kaldir")]  // gizli
    public static void RemoveGround()
    {
        int count = RemoveGroundSilent();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Zemin Kaldirildi",
            count + " zemin objesi silindi.\n\nSahne tekrar zeminsiz halde.",
            "Tamam");
    }

    private static int RemoveGroundSilent()
    {
        int count = 0;
        string[] groundNames = { "LouvreGround", "Pavement_Main", "Pavement_Outer",
                                  "Border_0", "Border_1", "Border_2", "Border_3",
                                  "BackupGround", "LouvreStoneGround" };
        foreach (var name in groundNames)
        {
            var go = GameObject.Find(name);
            while (go != null)
            {
                Object.DestroyImmediate(go);
                count++;
                go = GameObject.Find(name);
            }
        }
        return count;
    }

    /// <summary>
    /// StartFromScratch icinden cagrilir: piramit yakininda KUCUK bir trigger sphere kurar.
    /// Piramidin kendi collider'ini bozmaz (duvar carpismasi icin lazim).
    /// </summary>
    private static void AutoSetupPyramidPortal(GameObject louvreInstance)
    {
        Debug.Log("[Louvre] Otomatik piramit portal kurulumu...");

        // 1) Build Settings'e sahneleri ekle
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        string mainScene = "Assets/Scenes/SampleScene.unity";
        string interiorScene = "Assets/Scenes/LouvreInteriorOptimized.unity";
        if (System.IO.File.Exists(mainScene))
            sceneList.Add(new EditorBuildSettingsScene(mainScene, true));
        if (System.IO.File.Exists(interiorScene))
            sceneList.Add(new EditorBuildSettingsScene(interiorScene, true));
        EditorBuildSettings.scenes = sceneList.ToArray();

        if (louvreInstance == null)
        {
            Debug.LogWarning("[Louvre] louvreInstance null, portal kurulamadi.");
            return;
        }

        // 2) Piramit'i bul - STRICT detection
        //    Gercek cam piramit: ~35m x 22m x ~35m (X, Y, Z)
        //    Kare taban (X ≈ Z), uzun degil (Y < X)
        MeshRenderer pyramid = null;
        var renderers = louvreInstance.GetComponentsInChildren<MeshRenderer>();

        // Once isimle ara (en guvenli)
        foreach (var mr in renderers)
        {
            string n = mr.gameObject.name.ToLower();
            if (n.Contains("pyramid") || n.Contains("piramit") ||
                n.Contains("pyramide"))
            {
                pyramid = mr;
                Debug.Log($"  -> Piramit isimden bulundu: {mr.gameObject.name}");
                break;
            }
        }

        // Isimle bulunamadiysa SIKI boyut filtresi ile ara
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
                // Sıkı boyut filtresi: pyramid base 15-50m, height 8-30m
                if (sz.x < 15f || sz.x > 50f) continue;
                if (sz.z < 15f || sz.z > 50f) continue;
                if (sz.y < 8f || sz.y > 30f) continue;
                // Kare taban (X ve Z benzer olmali, fark < %30)
                float ratio = Mathf.Abs(sz.x - sz.z) / Mathf.Max(sz.x, sz.z);
                if (ratio > 0.3f) continue;
                // Yukseklik tabandan kucuk olmali (Y < ortalama X+Z)
                if (sz.y > (sz.x + sz.z) * 0.5f) continue;
                // Merkez yakininda olmali
                float distXZ = new Vector2(mr.bounds.center.x - center.x, mr.bounds.center.z - center.z).magnitude;
                // Puanlama: merkeze yakin + makul boy
                float score = distXZ + Mathf.Abs(sz.x - 35f) + Mathf.Abs(sz.y - 22f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = mr;
                }
            }
            pyramid = best;
            if (pyramid != null)
            {
                Debug.Log($"  -> Piramit boyut filtresi ile bulundu: {pyramid.gameObject.name}, sz={pyramid.bounds.size}");
            }
        }

        // Bulunamadiysa modelin merkezini fallback olarak kullan - portal MUTLAKA kurulacak
        Vector3 fallbackCenter = Vector3.zero;
        Vector3 fallbackSize = new Vector3(35f, 22f, 35f);  // cam piramit varsayilan boyutu
        bool usingFallback = false;
        if (pyramid == null && renderers.Length > 0)
        {
            Bounds louvreBounds = renderers[0].bounds;
            foreach (var mr in renderers) louvreBounds.Encapsulate(mr.bounds);
            fallbackCenter = new Vector3(louvreBounds.center.x, louvreBounds.min.y + 11f, louvreBounds.center.z);
            usingFallback = true;
            Debug.LogWarning($"[Louvre] Piramit mesh'i bulunamadi - fallback ile model merkezine portal kuruluyor: {fallbackCenter}");
        }

        // 3) PyramidPortal'i bir GameObject'e koy ve gercek piramit bounds'unu aktar.
        Vector3 pyramidCenter = usingFallback ? fallbackCenter : pyramid.bounds.center;
        Vector3 pyramidSize = usingFallback ? fallbackSize : pyramid.bounds.size;
        Debug.Log($"  -> Piramit: {(usingFallback ? "FALLBACK" : pyramid.gameObject.name)}, merkez={pyramidCenter}, boyut={pyramidSize}");

        // Eski portal trigger varsa sil
        var oldTrigger = GameObject.Find("PyramidPortalTrigger");
        if (oldTrigger != null) Object.DestroyImmediate(oldTrigger);

        var triggerGO = new GameObject("PyramidPortalTrigger");
        triggerGO.transform.position = pyramidCenter;

        var portal = triggerGO.AddComponent<PyramidPortal>();
        portal.targetSceneName = "LouvreInteriorOptimized";
        portal.promptDistance = 0.05f;     // kullanici istegi - cok yakin
        portal.usePyramidBounds = true;
        portal.useXZOnly = true;
        portal.pyramidCenter = pyramidCenter;
        portal.pyramidSize = pyramidSize;

        Debug.Log($"  -> PyramidPortal kuruldu - prompt 0.05m mesafede acilacak.");
    }

    private static void BuildLouvreGround(Bounds bounds)
    {
        Debug.Log("[Louvre] Plane zemin olusturuluyor (gri)...");

        var groundRoot = new GameObject("LouvreGround");

        // Boyut: modelin %50 disina cikar
        float groundSize = Mathf.Max(bounds.size.x, bounds.size.z) * 1.50f;
        float groundY = bounds.min.y - 0.02f;

        // Tek parca temiz plane
        var mainGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mainGround.name = "Pavement_Main";
        mainGround.transform.SetParent(groundRoot.transform);
        mainGround.transform.position = new Vector3(bounds.center.x, groundY, bounds.center.z);
        mainGround.transform.localScale = new Vector3(2f, 1f, 4f);  // Sabit 2-1-4 (kullanici istegi)

        // Gri taş/beton materyal
        var pavementMat = new Material(Shader.Find("Standard"));
        pavementMat.name = "PavementMaterial";
        pavementMat.color = new Color(0.55f, 0.55f, 0.55f);  // notral gri
        pavementMat.SetFloat("_Glossiness", 0.15f);
        pavementMat.SetFloat("_Metallic", 0.0f);
        mainGround.GetComponent<Renderer>().material = pavementMat;

        Debug.Log($"  -> Zemin: {groundSize:F1} m, gri (notral)");
    }

    private static void AddPavementBorder(Transform parent, Bounds bounds, float groundY)
    {
        // Border yok - tek parca temiz zemin
    }

    // [MenuItem(MENU_ROOT + "/8) Dokuyu Modele Yeniden Bagla")]  // gizli
    public static void RebindTextureManual()
    {
        var louvre = GameObject.Find("Louvre") ?? GameObject.Find("Louvre_Original") ?? GameObject.Find("Louvre_Optimized");
        if (louvre == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede Louvre objesi bulunamadi. Once SIFIRDAN BASLAT calistir.", "Tamam");
            return;
        }
        BindLouvreTextures(louvre);
        EditorUtility.DisplayDialog("Tamamlandi", "Doku tekrar baglandi. Game window'a bak.", "Tamam");
    }

    /// <summary>
    /// Louvre modelinin tum materyallerini bulup Louvre_Atlas_*.png/.tga dokusunu
    /// Base Color (Albedo / Main Texture) olarak baglar.
    /// </summary>
    private static void BindLouvreTextures(GameObject louvre)
    {
        Debug.Log("[Louvre] Doku baglaniyor...");

        // Dokuyu bul (PNG once, sonra TGA)
        Texture2D atlas = null;
        string[] candidates = {
            "Assets/Textures/Louvre_Atlas_Original.png",
            "Assets/Textures/Louvre_Atlas_Original.tga",
            "Assets/Textures/Louvre_Atlas_2k.png",
            "Assets/Textures/Louvre_Building_Atlas_8k.tga",
        };
        foreach (var p in candidates)
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (t != null) { atlas = t; Debug.Log($"  -> Doku bulundu: {p}"); break; }
        }
        if (atlas == null)
        {
            // Genis arama: Textures klasorundeki herhangi bir Louvre/Atlas isimli
            var guids = AssetDatabase.FindAssets("Atlas t:Texture2D", new[] { "Assets/Textures" });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (atlas != null) { Debug.Log($"  -> Doku bulundu (genis arama): {p}"); break; }
            }
        }
        if (atlas == null)
        {
            Debug.LogWarning("[Louvre] Doku bulunamadi! Model beyaz kalacak. Textures klasorune Louvre_Atlas_*.png koy.");
            return;
        }

        // Tum MeshRenderer'lardaki materyalleri bul
        var renderers = louvre.GetComponentsInChildren<MeshRenderer>();
        int materialCount = 0;
        var processedMats = new System.Collections.Generic.HashSet<Material>();

        foreach (var r in renderers)
        {
            // sharedMaterials degil, materials kullansak sahne instance'i olusur.
            // Asset materyalini degistirelim ki kalici olsun.
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null || processedMats.Contains(mat)) continue;
                // Ground veya Border materyallerini atla - Louvre dokusu degil
                if (mat.name.Contains("Ground") || mat.name.Contains("Border") ||
                    mat.name.Contains("Stone") || mat.name.Contains("Pavement"))
                {
                    processedMats.Add(mat);
                    continue;
                }
                processedMats.Add(mat);

                // Built-in Standard shader -> _MainTex
                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", atlas);
                    materialCount++;
                }
                // URP Lit shader -> _BaseMap
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", atlas);
                }
                // HDRP -> _BaseColorMap
                if (mat.HasProperty("_BaseColorMap"))
                {
                    mat.SetTexture("_BaseColorMap", atlas);
                }
                // Generic Color
                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", Color.white);
                }
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", Color.white);
                }
                EditorUtility.SetDirty(mat);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Louvre] Doku {materialCount} materyale baglandi (toplam {processedMats.Count} unique).");

        // Runtime'da da bagli kalsin: LouvreTextureRebinder ekle ve atlas'i assign et
        // (Sahne degisince FBX material'leri resetlendiginde tekrar baglayacak)
        var rebinder = louvre.GetComponent<LouvreTextureRebinder>();
        if (rebinder == null) rebinder = louvre.AddComponent<LouvreTextureRebinder>();
        rebinder.atlas = atlas;
        EditorUtility.SetDirty(louvre);
        Debug.Log("[Louvre] LouvreTextureRebinder eklendi - sahne degisince tekrar baglar.");
    }

    // [MenuItem(MENU_ROOT + "/1) Sahneyi Kur (Otomatik)")]  // gizli - 0) SIFIRDAN BASLAT yeterli
    public static void SetupAll()
    {
        if (!EditorUtility.DisplayDialog("Louvre Otomatik Kurulum",
            "Bu, sahneye Player, Canvas, InfoPanel, SettingsMenu, HUD ve " +
            "etkilesimli objeleri otomatik ekleyecek. Eski Main Camera " +
            "varsa bos kalacak (yeni MainCamera Player altinda olusur).\n\n" +
            "Devam edilsin mi?",
            "Evet, kur", "Iptal"))
        {
            return;
        }
        SetupAllInternal();
    }

    /// <summary>
    /// Komut satirindan -executeMethod LouvreSetup.AutoSetup ile cagrilir.
    /// Dialog gostermez, mevcut kurulumu temizler ve yeniden kurar.
    /// </summary>
    public static void AutoSetup()
    {
        Debug.Log("[Louvre] OTOMATIK kurulum (komut satiri / .bat tetikledi)...");
        // Once mevcut kurulumu temizle
        RemoveAllSilent();
        // Sonra yeniden kur
        SetupAllInternal();
        Debug.Log("[Louvre] OTOMATIK kurulum tamamlandi.");
    }

    private static void SetupAllInternal()
    {
        Debug.Log("[Louvre] Kurulum basliyor...");

        Bounds bounds = ComputeSceneBounds();

        // 1) Eski Main Camera'yi sil (Player altinda yeni olusacak)
        RemoveOldMainCamera();

        // 2) Player olustur
        var player = SetupPlayer(bounds);

        // 3) Canvas + EventSystem
        var canvas = SetupCanvas();

        // 4) InfoPanel
        var infoPanel = SetupInfoPanel(canvas, player);

        // 5) NavigationHUD
        SetupNavigationHUD(canvas);

        // 6) SettingsMenu
        var settings = SetupSettingsMenu(canvas, player);

        // 7) Modeli zemin haline getir + colliders
        EnsureModelColliders();

        // 8) Etkilesimli objeleri olustur
        SetupInteractables();

        // 9) Player components'lari interaction'a bagla
        var pi = player.GetComponent<PlayerInteraction>();
        if (pi != null && infoPanel != null) pi.infoPanel = infoPanel;

        // 10) Yere zemin ekle (model yok ise dusmesin diye guvenlik agi)
        EnsureBackupGround(bounds);

        // 11) Light kontrol
        EnsureDirectionalLight();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[Louvre] Kurulum tamamlandi!");
        EditorUtility.DisplayDialog("Tamam!",
            "Louvre sahnesi kuruldu.\n\n" +
            "KONTROLLER:\n" +
            "WASD = Yuru, Shift = Kos\n" +
            "Mouse = Bak\n" +
            "Sol Tik = Yapi bilgisini ac\n" +
            "Sag Tik basili tut = Teleport\n" +
            "F = Ucus modu, Space/Ctrl = Yukari/Asagi\n" +
            "H = Yardim, Esc = Settings menu\n\n" +
            "Play tusuna basabilirsin.",
            "Tamam");
    }

    // [MenuItem(MENU_ROOT + "/2) Etkilesimli Yapilari Sifirla")]  // gizli
    public static void ResetInteractables()
    {
        var existing = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach (var io in existing) Object.DestroyImmediate(io);
        Debug.Log($"[Louvre] {existing.Length} adet InteractableObject kaldirildi.");
    }

    // [MenuItem(MENU_ROOT + "/3) Tum Kurulumu Geri Al")]  // gizli
    public static void RemoveAll()
    {
        if (!EditorUtility.DisplayDialog("Louvre Geri Al",
            "Player, Canvas, InfoPanel, SettingsMenu, HUD ve InteractableObject'ler silinecek. Emin misin?",
            "Evet sil", "Iptal")) return;
        RemoveAllSilent();
        Debug.Log("[Louvre] Hepsi temizlendi.");
    }

    private static void RemoveAllSilent()
    {
        var player = GameObject.Find("Player");
        if (player != null) Object.DestroyImmediate(player);

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null) Object.DestroyImmediate(canvas.gameObject);

        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es != null) Object.DestroyImmediate(es.gameObject);

        var backup = GameObject.Find("BackupGround");
        if (backup != null) Object.DestroyImmediate(backup);

        var existing = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach (var io in existing) Object.DestroyImmediate(io);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // [MenuItem(MENU_ROOT + "/4) Player'i Modelin Onune Yeniden Konumlandir")]  // gizli
    public static void RepositionPlayer()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[Louvre] Player bulunamadi. Once Setup calistir.");
            return;
        }
        var bounds = ComputeSceneBounds();
        var pos = ComputeStartPosition(bounds);
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = pos;
        if (cc != null) cc.enabled = true;
        Debug.Log($"[Louvre] Player yeniden konumlandirildi: {pos}");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // === Setup parcalari =====================================================

    private static void RemoveOldMainCamera()
    {
        var existingPlayer = GameObject.Find("Player");
        if (existingPlayer != null)
        {
            // Mevcut Player varsa Main Camera onun altinda olabilir; biraktirma
            return;
        }
        // Sahnedeki bagimsiz Main Camera'lari bul
        var cams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var c in cams)
        {
            if (c.gameObject.CompareTag("MainCamera") && c.transform.parent == null)
            {
                Object.DestroyImmediate(c.gameObject);
            }
        }
    }

    private static GameObject SetupPlayer(Bounds bounds)
    {
        // Mevcut Player varsa kullan
        var existing = GameObject.Find("Player");
        if (existing != null)
        {
            Debug.Log("[Louvre] Mevcut Player kullaniliyor.");
            return existing;
        }

        // Player GameObject
        var player = new GameObject("Player");
        var startPos = ComputeStartPosition(bounds);
        player.transform.position = startPos;

        // CharacterController
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0.9f, 0);
        cc.skinWidth = 0.05f;

        // Camera child
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0, 1.6f, 0);  // goz seviyesi
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 65f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = Mathf.Max(bounds.size.magnitude * 3f, 3000f);
        camGO.AddComponent<AudioListener>();

        // Player kontrol scriptleri
        var pc = player.AddComponent<PlayerController>();
        pc.cameraTransform = camGO.transform;
        var pi = player.AddComponent<PlayerInteraction>();
        pi.viewCamera = cam;
        pi.playerController = pc;
        pi.maxDistance = 250f;
        var tp = player.AddComponent<TeleportController>();
        tp.maxDistance = bounds.size.magnitude * 1.5f;

        Debug.Log($"[Louvre] Player olusturuldu. Konum: {startPos}");
        return player;
    }

    private static Vector3 ComputeStartPosition(Bounds bounds)
    {
        // Modelin onunde (negatif Z), zeminde (Y=top of bounds + small offset)
        float distance = Mathf.Max(bounds.size.magnitude * 0.4f, 50f);
        return new Vector3(
            bounds.center.x,
            bounds.max.y + 2f,
            bounds.min.z - distance);
    }

    private static Canvas SetupCanvas()
    {
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null)
        {
            EnsureEventSystem();
            return existing;
        }

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModuleType != null)
        {
            es.AddComponent(inputModuleType);
            return;
        }
#endif
        es.AddComponent<StandaloneInputModule>();
    }

    private static InfoPanelController SetupInfoPanel(Canvas canvas, GameObject player)
    {
        var existing = Object.FindFirstObjectByType<InfoPanelController>();
        if (existing != null) return existing;

        var panelGO = new GameObject("InfoPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(480f, 640f);
        rt.anchoredPosition = new Vector2(-30f, 0f);
        var bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.07f, 0.10f, 0.92f);

        var titleGO = CreateText(panelGO.transform, "TitleText", "Yapi Adi",
                                  fontSize: 32, alignment: TextAnchor.UpperCenter,
                                  color: new Color(1f, 0.92f, 0.65f), bold: true);
        SetAnchors(titleGO.GetComponent<RectTransform>(), 0.05f, 0.85f, 0.95f, 0.97f);

        var descGO = CreateText(panelGO.transform, "DescriptionText",
                                 "Yapi aciklamasi burada gorunecek.",
                                 fontSize: 18, alignment: TextAnchor.UpperLeft,
                                 color: Color.white, bold: false);
        SetAnchors(descGO.GetComponent<RectTransform>(), 0.07f, 0.10f, 0.93f, 0.83f);

        // Image container
        var imgContainerGO = new GameObject("ImageContainer");
        imgContainerGO.transform.SetParent(panelGO.transform, false);
        var imgContainerRT = imgContainerGO.AddComponent<RectTransform>();
        SetAnchors(imgContainerRT, 0.1f, 0.55f, 0.9f, 0.83f);
        var imgGO = new GameObject("Image");
        imgGO.transform.SetParent(imgContainerGO.transform, false);
        var imgRT = imgGO.AddComponent<RectTransform>();
        SetAnchors(imgRT, 0f, 0f, 1f, 1f);
        var imgComp = imgGO.AddComponent<Image>();
        imgContainerGO.SetActive(false);

        // Close button
        var btnGO = CreateButton(panelGO.transform, "CloseButton", "X",
                                  new Color(0.7f, 0.2f, 0.2f, 0.9f), Color.white);
        SetAnchors(btnGO.GetComponent<RectTransform>(), 0.85f, 0.92f, 0.97f, 0.98f);
        var btn = btnGO.GetComponent<Button>();

        var ctrl = panelGO.AddComponent<InfoPanelController>();
        ctrl.titleText = titleGO.GetComponent<Text>();
        ctrl.descriptionText = descGO.GetComponent<Text>();
        ctrl.image = imgComp;
        ctrl.imageContainer = imgContainerGO;
        ctrl.closeButton = btn;

        panelGO.SetActive(false);
        Debug.Log("[Louvre] InfoPanel olusturuldu.");
        return ctrl;
    }

    private static void SetupNavigationHUD(Canvas canvas)
    {
        var existing = Object.FindFirstObjectByType<NavigationHUDController>();
        if (existing != null) return;

        var hudGO = new GameObject("NavigationHUD");
        hudGO.transform.SetParent(canvas.transform, false);
        var hudRT = hudGO.AddComponent<RectTransform>();
        SetAnchors(hudRT, 0f, 0f, 0f, 0f);
        hudRT.pivot = new Vector2(0f, 0f);
        hudRT.anchoredPosition = new Vector2(20f, 20f);
        hudRT.sizeDelta = new Vector2(360f, 300f);

        var bg = hudGO.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.7f);

        var helpGO = CreateText(hudGO.transform, "HelpText", "Yardim...",
                                 fontSize: 16, alignment: TextAnchor.UpperLeft,
                                 color: Color.white, bold: false);
        helpGO.GetComponent<Text>().supportRichText = true;
        SetAnchors(helpGO.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.95f, 0.95f);

        var hud = hudGO.AddComponent<NavigationHUDController>();
        hud.helpText = helpGO.GetComponent<Text>();
        hud.panel = hudGO;
        hud.startVisible = true;

        Debug.Log("[Louvre] NavigationHUD olusturuldu.");
    }

    private static SettingsMenuController SetupSettingsMenu(Canvas canvas, GameObject player)
    {
        var existing = Object.FindFirstObjectByType<SettingsMenuController>();
        if (existing != null) return existing;

        var menuGO = new GameObject("SettingsMenu");
        menuGO.transform.SetParent(canvas.transform, false);
        var rt = menuGO.AddComponent<RectTransform>();
        SetAnchors(rt, 0.5f, 0.5f, 0.5f, 0.5f);
        rt.sizeDelta = new Vector2(560f, 520f);
        rt.anchoredPosition = Vector2.zero;
        var bg = menuGO.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.06f, 0.10f, 0.96f);

        // Baslik
        var titleGO = CreateText(menuGO.transform, "Title", "AYARLAR",
                                  fontSize: 36, alignment: TextAnchor.MiddleCenter,
                                  color: new Color(1f, 0.92f, 0.65f), bold: true);
        SetAnchors(titleGO.GetComponent<RectTransform>(), 0.05f, 0.86f, 0.95f, 0.97f);

        // Volume label + slider
        var volLabelGO = CreateText(menuGO.transform, "VolumeLabel", "Ses: %80",
                                      fontSize: 18, alignment: TextAnchor.MiddleLeft,
                                      color: Color.white);
        SetAnchors(volLabelGO.GetComponent<RectTransform>(), 0.08f, 0.74f, 0.92f, 0.81f);
        var volSliderGO = CreateSlider(menuGO.transform, "VolumeSlider");
        SetAnchors(volSliderGO.GetComponent<RectTransform>(), 0.08f, 0.65f, 0.92f, 0.73f);

        // Quality dropdown
        var qLabelGO = CreateText(menuGO.transform, "QualityLabel", "Grafik Kalitesi:",
                                    fontSize: 18, alignment: TextAnchor.MiddleLeft,
                                    color: Color.white);
        SetAnchors(qLabelGO.GetComponent<RectTransform>(), 0.08f, 0.55f, 0.92f, 0.62f);
        var qDropGO = CreateDropdown(menuGO.transform, "QualityDropdown");
        SetAnchors(qDropGO.GetComponent<RectTransform>(), 0.08f, 0.46f, 0.92f, 0.54f);

        // Mouse sens
        var msLabelGO = CreateText(menuGO.transform, "MouseSensLabel", "Mouse Hassasiyeti: 0.15",
                                     fontSize: 18, alignment: TextAnchor.MiddleLeft,
                                     color: Color.white);
        SetAnchors(msLabelGO.GetComponent<RectTransform>(), 0.08f, 0.36f, 0.92f, 0.43f);
        var msSliderGO = CreateSlider(menuGO.transform, "MouseSensSlider");
        SetAnchors(msSliderGO.GetComponent<RectTransform>(), 0.08f, 0.27f, 0.92f, 0.35f);

        // Resume button
        var resumeGO = CreateButton(menuGO.transform, "ResumeButton", "DEVAM ET",
                                      new Color(0.2f, 0.55f, 0.3f, 1f), Color.white);
        SetAnchors(resumeGO.GetComponent<RectTransform>(), 0.08f, 0.10f, 0.48f, 0.20f);

        // Quit button
        var quitGO = CreateButton(menuGO.transform, "QuitButton", "CIK",
                                    new Color(0.7f, 0.2f, 0.2f, 1f), Color.white);
        SetAnchors(quitGO.GetComponent<RectTransform>(), 0.52f, 0.10f, 0.92f, 0.20f);

        var ctrl = menuGO.AddComponent<SettingsMenuController>();
        ctrl.volumeSlider = volSliderGO.GetComponent<Slider>();
        ctrl.volumeLabel = volLabelGO.GetComponent<Text>();
        ctrl.qualityDropdown = qDropGO.GetComponent<Dropdown>();
        ctrl.mouseSensSlider = msSliderGO.GetComponent<Slider>();
        ctrl.mouseSensLabel = msLabelGO.GetComponent<Text>();
        ctrl.resumeButton = resumeGO.GetComponent<Button>();
        ctrl.quitButton = quitGO.GetComponent<Button>();
        ctrl.playerController = player.GetComponent<PlayerController>();

        menuGO.SetActive(false);
        Debug.Log("[Louvre] SettingsMenu olusturuldu.");
        return ctrl;
    }

    // === Model + Etkilesim ==================================================

    private static void EnsureModelColliders()
    {
        var meshes = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int added = 0;
        foreach (var mr in meshes)
        {
            // Player altindakileri atla
            if (mr.transform.IsChildOf(GameObject.Find("Player")?.transform ?? mr.transform.root))
                continue;
            if (mr.GetComponent<Collider>() != null) continue;
            var mc = mr.gameObject.AddComponent<MeshCollider>();
            mc.convex = false;
            added++;
        }
        Debug.Log($"[Louvre] {added} modele MeshCollider eklendi.");
    }

    private static void SetupInteractables()
    {
        var allMeshes = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        var sorted = allMeshes
            .Where(mr => !mr.transform.IsChildOf(GameObject.Find("Player")?.transform ?? mr.transform.root))
            .Select(mr => new { mr, size = mr.bounds.size.magnitude })
            .OrderByDescending(x => x.size)
            .Take(8)
            .ToList();

        var presets = new (string title, string desc)[]
        {
            ("Cam Piramit",
             "I.M. Pei tarafindan 1989'da tasarlanan piramit, Louvre'un ana girisidir. "
             + "Taban 35.42 m, yukseklik 21.64 m. 603 elmas seklinde cam ve 70 ucgen camdan olusur."),
            ("Pavillon Sully",
             "Cour Carree'nin batisinda yer alan, Louvre'un en eski bolumlerinden biri. "
             + "Klasik Roma stilinde sutunlar ve heykellerle sus lenmis."),
            ("Denon Kanadi",
             "Louvre'un guney kanadi. Mona Lisa burada sergilenir. "
             + "Adini Napoleon'un muze yoneticisi Vivant Denon'dan alir."),
            ("Richelieu Kanadi",
             "Louvre'un kuzey kanadi. Fransiz ve Hollanda resimleri burada sergilenir. "
             + "1989-1993 arasinda muzeye eklenmistir."),
            ("Cour Napoleon",
             "Piramidin etrafindaki ana avlu. 7 yansitici havuz, "
             + "ana piramidin etrafinda simetrik bir desen olusturur."),
            ("Kucuk Piramit",
             "Ana piramidi cevreleyen 3 kucuk cam piramitten biri. "
             + "Yer alti galerilerine gun isigi saglarlar."),
            ("Pavillon Richelieu",
             "Richelieu kanadinin baslangicindaki kose pavyonu. "
             + "Klasik Fransiz Renaissance mimarisinin guzel bir ornegi."),
            ("Pavillon Denon",
             "Denon kanadinin baslangicindaki kose pavyonu. "
             + "Heykel galerilerine ev sahipligi yapar."),
        };

        int added = 0;
        for (int i = 0; i < sorted.Count && i < presets.Length; i++)
        {
            var go = sorted[i].mr.gameObject;
            if (go.GetComponent<InteractableObject>() != null) continue;
            if (go.GetComponent<Collider>() == null)
            {
                var mc = go.AddComponent<MeshCollider>();
                mc.convex = false;
            }
            var io = go.AddComponent<InteractableObject>();
            io.title = presets[i].title;
            io.description = presets[i].desc;
            io.highlightOnHover = true;
            added++;
        }
        Debug.Log($"[Louvre] {added} adet etkilesimli yapi olusturuldu.");
    }

    private static void EnsureBackupGround(Bounds bounds)
    {
        if (GameObject.Find("BackupGround") != null) return;
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "BackupGround";
        float scale = Mathf.Max(bounds.size.x, bounds.size.z) * 0.2f;
        if (scale < 50f) scale = 50f;
        ground.transform.localScale = new Vector3(scale, 1f, scale);
        ground.transform.position = new Vector3(bounds.center.x, bounds.min.y - 0.05f, bounds.center.z);
        var rend = ground.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.36f, 0.33f, 0.28f);
            rend.material = mat;
        }
    }


    // === EKSIK YARDIMCI METODLAR ===========================================

    private static Bounds ComputeSceneBounds()
    {
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        var filtered = renderers.Where(r =>
            !r.gameObject.name.Contains("BackupGround") &&
            !r.gameObject.name.Contains("LouvreGround") &&
            !r.gameObject.name.Contains("Pavement") &&
            !r.gameObject.name.Contains("Border"));
        if (!filtered.Any())
            return new Bounds(Vector3.zero, new Vector3(200f, 50f, 200f));
        Bounds b = filtered.First().bounds;
        foreach (var r in filtered.Skip(1)) b.Encapsulate(r.bounds);
        Debug.Log($"[Louvre] Sahne sinirlari: center={b.center}, size={b.size}");
        return b;
    }

    private static void EnsureDirectionalLight()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
            if (l.type == LightType.Directional) return;
        var go = new GameObject("Directional Light");
        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.88f);
        light.intensity = 1.2f;
        go.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
        Debug.Log("[Louvre] Directional Light eklendi.");
    }

    private static GameObject CreateText(Transform parent, string name, string text,
        int fontSize, TextAnchor alignment, Color color, bool bold = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = color;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Color bgColor, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        go.AddComponent<Button>();
        var labelGO = CreateText(go.transform, "Text", label, 18,
                                  TextAnchor.MiddleCenter, textColor, true);
        SetAnchors(labelGO.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
        return go;
    }

    private static GameObject CreateSlider(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var slider = go.AddComponent<Slider>();

        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        SetAnchors(bgRT, 0f, 0.4f, 1f, 0.6f);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        SetAnchors(faRT, 0f, 0.4f, 1f, 0.6f);
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        SetAnchors(fillRT, 0f, 0f, 1f, 1f);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.7f, 0.95f, 1f);

        var handleSlideArea = new GameObject("Handle Slide Area");
        handleSlideArea.transform.SetParent(go.transform, false);
        var hsaRT = handleSlideArea.AddComponent<RectTransform>();
        SetAnchors(hsaRT, 0f, 0f, 1f, 1f);
        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleSlideArea.transform, false);
        var handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 0);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
        return go;
    }

    private static GameObject CreateDropdown(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        var dd = go.AddComponent<Dropdown>();

        var labelGO = CreateText(go.transform, "Label", "Yuksek",
                                   16, TextAnchor.MiddleLeft, Color.white);
        var labelRT = labelGO.GetComponent<RectTransform>();
        SetAnchors(labelRT, 0f, 0f, 1f, 1f);
        labelRT.offsetMin = new Vector2(10f, 2f);
        labelRT.offsetMax = new Vector2(-30f, -2f);

        var template = new GameObject("Template");
        template.transform.SetParent(go.transform, false);
        var templateRT = template.AddComponent<RectTransform>();
        SetAnchors(templateRT, 0f, 0f, 1f, 0f);
        templateRT.pivot = new Vector2(0.5f, 1f);
        templateRT.anchoredPosition = new Vector2(0f, 2f);
        templateRT.sizeDelta = new Vector2(0f, 150f);
        var tImg = template.AddComponent<Image>();
        tImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        var vRT = viewport.AddComponent<RectTransform>();
        SetAnchors(vRT, 0f, 0f, 1f, 1f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.04f);

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var cRT = content.AddComponent<RectTransform>();
        SetAnchors(cRT, 0f, 1f, 1f, 1f);
        cRT.pivot = new Vector2(0.5f, 1f);
        cRT.sizeDelta = new Vector2(0f, 28f);

        var item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        var iRT = item.AddComponent<RectTransform>();
        SetAnchors(iRT, 0f, 0.5f, 1f, 0.5f);
        iRT.sizeDelta = new Vector2(0f, 20f);
        var toggle = item.AddComponent<Toggle>();
        var iBg = new GameObject("Item Background");
        iBg.transform.SetParent(item.transform, false);
        var iBgRT = iBg.AddComponent<RectTransform>();
        SetAnchors(iBgRT, 0f, 0f, 1f, 1f);
        var iBgImg = iBg.AddComponent<Image>();
        iBgImg.color = new Color(1, 1, 1, 0.05f);
        var iCheck = new GameObject("Item Checkmark");
        iCheck.transform.SetParent(item.transform, false);
        var iCheckRT = iCheck.AddComponent<RectTransform>();
        SetAnchors(iCheckRT, 0f, 0.5f, 0f, 0.5f);
        iCheckRT.sizeDelta = new Vector2(20, 20);
        iCheckRT.anchoredPosition = new Vector2(10, 0);
        var iCheckImg = iCheck.AddComponent<Image>();
        iCheckImg.color = new Color(0.4f, 0.7f, 0.95f, 1f);
        var iLabel = CreateText(item.transform, "Item Label", "Option A",
                                 14, TextAnchor.MiddleLeft, Color.white);
        var iLabelRT = iLabel.GetComponent<RectTransform>();
        SetAnchors(iLabelRT, 0f, 0f, 1f, 1f);
        iLabelRT.offsetMin = new Vector2(20, 1);
        iLabelRT.offsetMax = new Vector2(-10, -2);
        toggle.targetGraphic = iBgImg;
        toggle.graphic = iCheckImg;
        toggle.isOn = true;

        dd.template = templateRT;
        dd.captionText = labelGO.GetComponent<Text>();
        dd.itemText = iLabel.GetComponent<Text>();
        template.SetActive(false);

        return go;
    }

    private static void SetAnchors(RectTransform rt, float aMinX, float aMinY, float aMaxX, float aMaxY)
    {
        rt.anchorMin = new Vector2(aMinX, aMinY);
        rt.anchorMax = new Vector2(aMaxX, aMaxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
