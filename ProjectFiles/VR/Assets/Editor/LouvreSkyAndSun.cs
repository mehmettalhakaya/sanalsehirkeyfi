using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Louvre dis sahnesi icin gercekci panoramik gokyuzu + gunes (Directional Light)
/// otomatik kurar. Procedural sky texture (mavi gradient + bulutlar + gunes diski + halo),
/// Skybox/Panoramic shader, soft shadows. VR uyumlu - butun lighting baked degil
/// realtime ama tek directional light + skybox ambient (ucuz).
/// Auto-run. Backup yok. Idempotent.
/// </summary>
[InitializeOnLoad]
public static class LouvreSkyAndSun
{
    const string SKY_TEX_PATH = "Assets/Textures/louvre_sky_panorama.png";
    const string SKY_MAT_PATH = "Assets/Materials/LouvreSkybox.mat";
    const string SUN_NAME     = "Sun_DirectionalLight";

    const int SKY_W = 1024;
    const int SKY_H = 512;

    // Gunes yonu: 35deg yukseklik, -45deg azimuth (oglen sonrasi Paris isigi hissi)
    static readonly Vector3 SUN_EULER = new Vector3(35f, -45f, 0f);

    private const string SESSION_KEY = "LouvreSkyAndSun_RanThisSession_v3";

    static LouvreSkyAndSun()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        if (!SessionState.GetBool(SESSION_KEY, false))
            EditorApplication.delayCall += () => CheckAndApply(EditorSceneManager.GetActiveScene());
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += () => CheckAndApply(scene);
    }

    static void CheckAndApply(Scene scene)
    {
        if (scene.name != "SampleScene") return;

        // Idempotent kontrol: sun + skybox + ana kameranin clear flag'i Skybox olmali
        Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(SKY_MAT_PATH);
        var existingSun = GameObject.Find(SUN_NAME);
        bool camsOk = true;
        var mainCam = Camera.main;
        if (mainCam != null && mainCam.clearFlags != CameraClearFlags.Skybox) camsOk = false;
        if (existingSun != null && existingMat != null && RenderSettings.skybox == existingMat && camsOk)
            return;

        SessionState.SetBool(SESSION_KEY, true);
        Apply(scene);
    }

    static void Apply(Scene scene)
    {
        EnsureDir("Assets/Textures");
        EnsureDir("Assets/Materials");

        // === Panoramik gokyuzu texture ===
        if (!File.Exists(SKY_TEX_PATH))
        {
            Debug.Log("[SkyAndSun] Panoramik gokyuzu uretiliyor (gunes + bulutlar)...");
            var tex = GenerateSky(SKY_W, SKY_H, SUN_EULER);
            File.WriteAllBytes(SKY_TEX_PATH, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SKY_TEX_PATH, ImportAssetOptions.ForceUpdate);
            var imp = AssetImporter.GetAtPath(SKY_TEX_PATH) as TextureImporter;
            if (imp != null)
            {
                imp.textureShape = TextureImporterShape.Texture2D;
                imp.textureType = TextureImporterType.Default;
                imp.sRGBTexture = true;
                imp.mipmapEnabled = true;
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.filterMode = FilterMode.Trilinear;
                imp.maxTextureSize = 2048;
                imp.SaveAndReimport();
            }
        }
        var skyTexAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(SKY_TEX_PATH);

        // === Skybox materyali (Panoramic shader) ===
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(SKY_MAT_PATH);
        if (skyMat == null)
        {
            var shader = Shader.Find("Skybox/Panoramic");
            if (shader == null) shader = Shader.Find("Skybox/Procedural");
            skyMat = new Material(shader);
            AssetDatabase.CreateAsset(skyMat, SKY_MAT_PATH);
        }
        if (skyMat.HasProperty("_MainTex"))    skyMat.SetTexture("_MainTex", skyTexAsset);
        if (skyMat.HasProperty("_Tint"))       skyMat.SetColor("_Tint", new Color(0.55f, 0.55f, 0.55f, 1f));
        if (skyMat.HasProperty("_Exposure"))   skyMat.SetFloat("_Exposure", 1.1f);
        if (skyMat.HasProperty("_Rotation"))   skyMat.SetFloat("_Rotation", 0f);
        if (skyMat.HasProperty("_Mapping"))    skyMat.SetInt("_Mapping", 1);   // Lat-Long
        if (skyMat.HasProperty("_ImageType"))  skyMat.SetInt("_ImageType", 0); // 360
        if (skyMat.HasProperty("_Mirror"))     skyMat.SetInt("_Mirror", 0);
        if (skyMat.HasProperty("_Layout"))     skyMat.SetInt("_Layout", 0);
        EditorUtility.SetDirty(skyMat);

        RenderSettings.skybox = skyMat;

        // === Eski directional lightlari temizle, yeni gunesi kur ===
        GameObject sunGO = GameObject.Find(SUN_NAME);
#if UNITY_2023_1_OR_NEWER
        var allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var allLights = Object.FindObjectsOfType<Light>(true);
#endif
        foreach (var l in allLights)
        {
            if (l == null) continue;
            if (l.type != LightType.Directional) continue;
            if (sunGO != null && l.gameObject == sunGO) continue;
            // Eski default Directional Light - sil
            Object.DestroyImmediate(l.gameObject);
        }

        if (sunGO == null)
        {
            sunGO = new GameObject(SUN_NAME);
            sunGO.AddComponent<Light>();
        }
        var sun = sunGO.GetComponent<Light>();
        if (sun == null) sun = sunGO.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1.00f, 0.96f, 0.86f); // sicak gunes
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.55f;
        sun.shadowBias = 0.05f;
        sun.shadowNormalBias = 0.4f;
        sun.shadowNearPlane = 0.2f;
        sunGO.transform.rotation = Quaternion.Euler(SUN_EULER);
        sunGO.transform.position = new Vector3(0f, 10f, 0f);

        RenderSettings.sun = sun;

        // === Ambient + Reflection (ucuz, skybox tabanli) ===
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.reflectionIntensity = 0.7f;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        RenderSettings.defaultReflectionResolution = 128;

        // Sahneye derinlik veren hafif fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.80f, 0.86f, 0.90f);
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 250f;

        DynamicGI.UpdateEnvironment();

        // === Tum sahne kameralarinin Clear Flags'ini Skybox'a getir ===
        // (Solid Color veya Depth Only ise skybox gorunmez, bu en kritik fix)
#if UNITY_2023_1_OR_NEWER
        var allCams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var allCams = Object.FindObjectsOfType<Camera>(true);
#endif
        foreach (var c in allCams)
        {
            if (c == null) continue;
            // UI overlay kameralarini bozma (cullingMask sadece UI layer'i icine alanlari atla)
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0 && c.cullingMask == (1 << uiLayer)) continue;
            if (c.clearFlags != CameraClearFlags.Skybox)
            {
                c.clearFlags = CameraClearFlags.Skybox;
                EditorUtility.SetDirty(c);
            }
        }

        EditorUtility.SetDirty(sunGO);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SkyAndSun] Gokyuzu + gunes + ambient + kamera clear flags ayarlandi.");
    }

    // ============================================================
    // PROCEDURAL SKY (equirectangular, 3D-pos based noise -> seamless)
    // ============================================================
    static Texture2D GenerateSky(int w, int h, Vector3 sunEuler)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, true, false);
        var pixels = new Color[w * h];

        // Gunesin GOK'teki yonu = -ışık yönü
        Vector3 lightForward = Quaternion.Euler(sunEuler) * Vector3.forward;
        Vector3 sunDir = -lightForward;
        sunDir.Normalize();

        Color horizonCol = new Color(0.82f, 0.86f, 0.90f); // sisli ufuk
        Color zenithCol  = new Color(0.32f, 0.55f, 0.85f); // koyu mavi tepe
        Color groundCol  = new Color(0.45f, 0.45f, 0.42f); // alt yari (gorunmez)

        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1);
            float phi = (v - 0.5f) * Mathf.PI;
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);
                float theta = (u - 0.5f) * 2f * Mathf.PI;

                Vector3 dir = new Vector3(
                    Mathf.Cos(phi) * Mathf.Sin(theta),
                    Mathf.Sin(phi),
                    Mathf.Cos(phi) * Mathf.Cos(theta));

                Color skyC;
                if (phi >= 0f)
                {
                    float elevT = Mathf.Clamp01(phi / (Mathf.PI * 0.5f));
                    skyC = Color.Lerp(horizonCol, zenithCol, Mathf.Pow(elevT, 0.55f));

                    // Gunes diski + halo
                    float sunDot = Vector3.Dot(dir, sunDir);
                    float sunAngle = Mathf.Acos(Mathf.Clamp(sunDot, -1f, 1f));
                    if (sunAngle < 0.030f)
                    {
                        skyC = new Color(1f, 0.97f, 0.85f);
                    }
                    else if (sunAngle < 0.30f)
                    {
                        float halo = Mathf.SmoothStep(0.30f, 0.03f, sunAngle);
                        skyC = Color.Lerp(skyC, new Color(1f, 0.93f, 0.76f), halo * 0.55f);
                    }

                    // Bulutlar (3D pos-based Perlin -> seamless wrap)
                    float cs = 3f;
                    float c1 = Mathf.PerlinNoise(dir.x * cs + 100f, dir.z * cs + 100f);
                    float c2 = Mathf.PerlinNoise(dir.x * cs * 2.1f + 200f, dir.z * cs * 2.1f + 200f);
                    float c3 = Mathf.PerlinNoise(dir.x * cs * 4.3f + 300f, dir.z * cs * 4.3f + 300f);
                    float cloud = c1 * 0.55f + c2 * 0.30f + c3 * 0.15f;
                    cloud = Mathf.SmoothStep(0.50f, 0.78f, cloud);
                    cloud *= Mathf.SmoothStep(0.05f, 0.45f, elevT);

                    float sunInfl = Mathf.Max(0f, sunDot * 0.5f + 0.5f);
                    Color cloudC = Color.Lerp(
                        new Color(0.78f, 0.80f, 0.83f),
                        new Color(1.00f, 0.98f, 0.92f),
                        sunInfl);
                    skyC = Color.Lerp(skyC, cloudC, cloud * 0.85f);
                }
                else
                {
                    // Alt yari (gorunmemeli ama tutarli olsun)
                    float gradT = Mathf.Clamp01(-phi / (Mathf.PI * 0.5f));
                    skyC = Color.Lerp(horizonCol, groundCol, gradT);
                }
                pixels[y * w + x] = skyC;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static void EnsureDir(string p)
    {
        if (!Directory.Exists(p))
        {
            Directory.CreateDirectory(p);
            AssetDatabase.Refresh();
        }
    }
}
