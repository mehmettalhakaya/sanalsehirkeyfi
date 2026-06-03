using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Avlu zeminini PBR tas dosemeye otomatik cevirir.
/// Menu YOK - sahne acildiginda otomatik kontrol edip uygular (zaten uygulanmissa atlar).
/// Backup olusturmaz.
/// </summary>
[InitializeOnLoad]
public static class PavementUpgrader
{
    private const string TEXTURE_PATH  = "Assets/Textures/courtyard_pavement.png";
    private const string NORMAL_PATH   = "Assets/Textures/courtyard_pavement_normal.png";
    private const string MATERIAL_PATH = "Assets/Materials/CourtyardPavement.mat";
    private const int TEX_SIZE = 1024;
    private const int TILES_PER_SIDE = 8;
    private const int GROUT_WIDTH = 5;

    private const string SESSION_KEY = "PavementUpgrader_RanThisSession";

    static PavementUpgrader()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        // Editor zaten acikken ilk init'te de kontrol et
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

        var pavement = GameObject.Find("Pavement_Main");
        if (pavement == null) return;

        var mr = pavement.GetComponent<MeshRenderer>();
        if (mr == null) return;

        // Zaten upgrade edilmis material atanmissa atla
        if (mr.sharedMaterial != null && mr.sharedMaterial.name == "CourtyardPavement")
            return;

        SessionState.SetBool(SESSION_KEY, true);
        ApplyUpgrade(mr, scene);
    }

    static void ApplyUpgrade(MeshRenderer pavementMR, Scene scene)
    {
        EnsureDir("Assets/Textures");
        EnsureDir("Assets/Materials");

        // Texture yoksa olustur
        if (!File.Exists(TEXTURE_PATH))
        {
            Debug.Log("[PavementUpgrader] Albedo texture uretiliyor...");
            Texture2D albedo = GenerateStoneAlbedo(TEX_SIZE, TILES_PER_SIDE, GROUT_WIDTH);
            SaveTextureAsPNG(albedo, TEXTURE_PATH);
            AssetDatabase.ImportAsset(TEXTURE_PATH, ImportAssetOptions.ForceUpdate);
            ConfigureTextureImporter(TEXTURE_PATH, false);
        }
        if (!File.Exists(NORMAL_PATH))
        {
            Debug.Log("[PavementUpgrader] Normal map uretiliyor...");
            Texture2D normal = GenerateStoneNormal(TEX_SIZE, TILES_PER_SIDE, GROUT_WIDTH);
            SaveTextureAsPNG(normal, NORMAL_PATH);
            AssetDatabase.ImportAsset(NORMAL_PATH, ImportAssetOptions.ForceUpdate);
            ConfigureTextureImporter(NORMAL_PATH, true);
        }

        var albedoAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(TEXTURE_PATH);
        var normalAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(NORMAL_PATH);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, MATERIAL_PATH);
        }
        mat.SetTexture("_MainTex", albedoAsset);
        if (mat.HasProperty("_BumpMap"))    mat.SetTexture("_BumpMap", normalAsset);
        if (mat.HasProperty("_BumpScale"))  mat.SetFloat("_BumpScale", 0.6f);
        mat.SetColor("_Color", Color.white);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.25f);
        if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0f);
        mat.EnableKeyword("_NORMALMAP");
        mat.mainTextureScale = new Vector2(2.5f, 5f);
        if (mat.HasProperty("_BumpMap"))    mat.SetTextureScale("_BumpMap", new Vector2(2.5f, 5f));
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();

        pavementMR.sharedMaterial = mat;
        EditorUtility.SetDirty(pavementMR.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[PavementUpgrader] Avlu zemini PBR tas dosemeye cevrildi (otomatik).");
    }

    static void ConfigureTextureImporter(string path, bool isNormal)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        imp.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        imp.sRGBTexture = !isNormal;
        imp.mipmapEnabled = true;
        imp.maxTextureSize = 1024;
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.filterMode = FilterMode.Trilinear;
        imp.anisoLevel = 4;
        imp.SaveAndReimport();
    }

    // ============================================================
    // TEXTURE URETIM (degismedi)
    // ============================================================

    static Texture2D GenerateStoneAlbedo(int size, int tiles, int grout)
    {
        int tileSize = size / tiles;
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];

        Color[] tilePalette = new Color[]
        {
            new Color(0.78f, 0.74f, 0.68f),
            new Color(0.74f, 0.70f, 0.65f),
            new Color(0.72f, 0.69f, 0.66f),
            new Color(0.76f, 0.73f, 0.68f),
            new Color(0.70f, 0.67f, 0.62f),
            new Color(0.75f, 0.72f, 0.67f),
            new Color(0.73f, 0.70f, 0.65f),
            new Color(0.77f, 0.73f, 0.67f),
        };
        Color groutColor = new Color(0.32f, 0.30f, 0.27f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int localX = x % tileSize;
                int localY = y % tileSize;
                bool isGrout = localX < grout || localY < grout ||
                               localX >= tileSize - grout || localY >= tileSize - grout;

                Color c;
                if (isGrout)
                {
                    c = groutColor;
                    float gn = (Mathf.PerlinNoise(x * 0.18f, y * 0.18f) - 0.5f) * 0.06f;
                    c.r += gn; c.g += gn; c.b += gn;
                }
                else
                {
                    int tileIdxX = x / tileSize;
                    int tileIdxY = y / tileSize;
                    int paletteIdx = (tileIdxX * 31 + tileIdxY * 17 + 7) % tilePalette.Length;
                    if (paletteIdx < 0) paletteIdx += tilePalette.Length;
                    c = tilePalette[paletteIdx];

                    float n1 = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                    float n2 = Mathf.PerlinNoise(x * 0.12f + 100, y * 0.12f + 100);
                    float noise = (n1 - 0.5f) * 0.10f + (n2 - 0.5f) * 0.05f;
                    c.r = Mathf.Clamp01(c.r + noise);
                    c.g = Mathf.Clamp01(c.g + noise * 0.95f);
                    c.b = Mathf.Clamp01(c.b + noise * 0.90f);

                    float distFromCenter = Mathf.Max(
                        Mathf.Abs(localX - tileSize * 0.5f) / (tileSize * 0.5f),
                        Mathf.Abs(localY - tileSize * 0.5f) / (tileSize * 0.5f));
                    float vignette = Mathf.SmoothStep(0.6f, 1f, distFromCenter) * 0.08f;
                    c.r -= vignette; c.g -= vignette; c.b -= vignette;
                }
                pixels[y * size + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateStoneNormal(int size, int tiles, int grout)
    {
        int tileSize = size / tiles;
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, true);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int localX = x % tileSize;
                int localY = y % tileSize;
                int dLeft = localX, dRight = tileSize - 1 - localX;
                int dBottom = localY, dTop = tileSize - 1 - localY;
                float nx = 0.5f, ny = 0.5f, nz = 1f;
                int rampDist = grout + 3;
                if (dLeft < rampDist)        { float t = 1f - (dLeft / (float)rampDist);   nx = 0.5f - t * 0.3f; }
                else if (dRight < rampDist)  { float t = 1f - (dRight / (float)rampDist);  nx = 0.5f + t * 0.3f; }
                if (dBottom < rampDist)      { float t = 1f - (dBottom / (float)rampDist); ny = 0.5f - t * 0.3f; }
                else if (dTop < rampDist)    { float t = 1f - (dTop / (float)rampDist);    ny = 0.5f + t * 0.3f; }
                float noise = (Mathf.PerlinNoise(x * 0.08f, y * 0.08f) - 0.5f) * 0.04f;
                nx += noise; ny += noise * 0.9f;
                pixels[y * size + x] = new Color(Mathf.Clamp01(nx), Mathf.Clamp01(ny), nz, 1f);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static void SaveTextureAsPNG(Texture2D tex, string path)
    {
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);
        Object.DestroyImmediate(tex);
    }

    static void EnsureDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
