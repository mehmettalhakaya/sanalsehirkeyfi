using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Louvre cevre peyzajini otomatik kurar - v3.
/// v3: Klasik Paris tarzi lamba direkleri, taş banklar, gravel yuruyus hatlari,
/// tas bordurler, dekoratif calilar, 3 tipte agac cesitliligi (sutun/yuvarlak/topiary).
/// Procedural textured materials + multi-sphere foliage. VR-friendly.
/// Auto-run. Backup yok. Idempotent (versiyon marker ile - v1/v2 -> v3 upgrade).
/// </summary>
[InitializeOnLoad]
public static class LouvreLandscapeBuilder
{
    // Tint renkleri (texture varlik uzerinden uygulanir)
    static readonly Color GRASS_TINT   = new Color(0.90f, 1.00f, 0.85f);
    static readonly Color HEDGE_TINT   = new Color(0.85f, 0.95f, 0.80f);
    static readonly Color TRUNK_TINT   = new Color(0.95f, 0.92f, 0.88f);
    static readonly Color FOLIAGE_TINT = new Color(0.92f, 1.00f, 0.85f);
    static readonly Color GRAVEL_TINT  = new Color(0.95f, 0.92f, 0.88f);
    static readonly Color STONE_TINT   = new Color(0.95f, 0.94f, 0.90f);
    static readonly Color WOOD_TINT    = new Color(0.85f, 0.78f, 0.68f);
    static readonly Color METAL_TINT   = new Color(0.20f, 0.18f, 0.16f);
    static readonly Color GLOBE_TINT   = new Color(0.95f, 0.93f, 0.85f);

    const float PAVE_HALF_X = 10f;
    const float PAVE_HALF_Z = 20f;
    const float GROUND_Y    = -1.06f;

    const string LANDSCAPE_ROOT = "LouvreLandscape";
    const string VERSION_MARKER = "__v30__";
    const string MAT_DIR = "Assets/Materials";
    const string TEX_DIR = "Assets/Textures";

    // Texture paths
    const string GRASS_TEX  = "Assets/Textures/louvre_grass.png";
    const string GRASS_NRM  = "Assets/Textures/louvre_grass_normal.png";
    const string HEDGE_TEX  = "Assets/Textures/louvre_hedge.png";
    const string HEDGE_NRM  = "Assets/Textures/louvre_hedge_normal.png";
    const string BARK_TEX   = "Assets/Textures/louvre_bark.png";
    const string BARK_NRM   = "Assets/Textures/louvre_bark_normal.png";
    const string LEAF_TEX   = "Assets/Textures/louvre_foliage.png";
    const string LEAF_NRM   = "Assets/Textures/louvre_foliage_normal.png";
    const string GRAVEL_TEX = "Assets/Textures/louvre_gravel.png";
    const string GRAVEL_NRM = "Assets/Textures/louvre_gravel_normal.png";
    const string STONE_TEX  = "Assets/Textures/louvre_stone.png";
    const string WOOD_TEX   = "Assets/Textures/louvre_wood.png";

    private const string SESSION_KEY = "LouvreLandscapeBuilder_RanThisSession_v30";

    // === v7: GERCEK FBX agac modelleri ===
    const string BEECH_FBX     = "Assets/Models/Trees/Beech/beech.fbx";
    const string BEECH_BARK    = "Assets/Models/Trees/Beech/bark01.png";
    const string BEECH_BARK_N  = "Assets/Models/Trees/Beech/bark01_normal.png";
    const string BEECH_LEAF    = "Assets/Models/Trees/Beech/beech leaf.png";
    const string BEECH_LEAF_N  = "Assets/Models/Trees/Beech/beech leaf_Normal.png";
    const string OAK_FBX       = "Assets/Models/Trees/Oak/oak.fbx";
    const string OAK_LEAF      = "Assets/Models/Trees/Oak/leaf01.png";

    // === v11: Gercek PBR ground + Trafalgar lamb ===
    const string GROUND_PBR_DIFFUSE = "Assets/Models/GroundPBR/ground_close_04_basecolor.jpg";
    const string GROUND_PBR_NORMAL  = "Assets/Models/GroundPBR/ground_close_04_normal.jpg";
    const string GROUND_PBR_AO      = "Assets/Models/GroundPBR/ground_close_04_oclusion.jpg";
    const string LAMP_OBJ           = "Assets/Models/Lamps/TrafalgarSquareLampWest01.obj";
    const string LAMP_TEX           = "Assets/Models/Lamps/TrafalgarSquareLampWest01_Model_4_u1_v1.jpg";

    // v13: Grass clump FBX (rostlinka_07c)
    const string GRASS_CLUMP_FBX    = "Assets/Models/Grass/grass_clump.fbx";
    const string GRASS_CLUMP_TEX    = "Assets/Models/Grass/grass_clump.png";

    static LouvreLandscapeBuilder()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        if (!SessionState.GetBool(SESSION_KEY, false))
            EditorApplication.delayCall += () => CheckAndBuild(EditorSceneManager.GetActiveScene());
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += () => CheckAndBuild(scene);
    }

    static void CheckAndBuild(Scene scene)
    {
        if (scene.name != "SampleScene") return;
        if (GameObject.Find("Pavement_Main") == null) return;

        var existing = GameObject.Find(LANDSCAPE_ROOT);
        if (existing != null)
        {
            if (existing.transform.Find(VERSION_MARKER) != null) return; // zaten v3
            Debug.Log("[Landscape] Eski versiyon -> v3 yapisina yukseltiliyor...");
            Object.DestroyImmediate(existing);
        }

        SessionState.SetBool(SESSION_KEY, true);
        Build(scene);
    }

    static void Build(Scene scene)
    {
        EnsureDir(TEX_DIR);
        EnsureDir(MAT_DIR);

        // Texture'lari (yoksa) uret
        EnsureTexture(GRASS_TEX,  512, false, () => GenerateGrassAlbedo(512));
        EnsureTexture(GRASS_NRM,  512, true,  () => GenerateGrassNormal(512));
        EnsureTexture(HEDGE_TEX,  512, false, () => GenerateHedgeAlbedo(512));
        EnsureTexture(HEDGE_NRM,  512, true,  () => GenerateHedgeNormal(512));
        EnsureTexture(BARK_TEX,   256, false, () => GenerateBarkAlbedo(256));
        EnsureTexture(BARK_NRM,   256, true,  () => GenerateBarkNormal(256));
        EnsureTexture(LEAF_TEX,   512, false, () => GenerateFoliageAlbedo(512));
        EnsureTexture(LEAF_NRM,   512, true,  () => GenerateFoliageNormal(512));
        EnsureTexture(GRAVEL_TEX, 512, false, () => GenerateGravelAlbedo(512));
        EnsureTexture(GRAVEL_NRM, 512, true,  () => GenerateGravelNormal(512));
        EnsureTexture(STONE_TEX,  256, false, () => GenerateStoneAlbedo(256));
        EnsureTexture(WOOD_TEX,   256, false, () => GenerateWoodAlbedo(256));

        var grassTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(GRASS_TEX);
        var grassNrm  = AssetDatabase.LoadAssetAtPath<Texture2D>(GRASS_NRM);
        var hedgeTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(HEDGE_TEX);
        var hedgeNrm  = AssetDatabase.LoadAssetAtPath<Texture2D>(HEDGE_NRM);
        var barkTex   = AssetDatabase.LoadAssetAtPath<Texture2D>(BARK_TEX);
        var barkNrm   = AssetDatabase.LoadAssetAtPath<Texture2D>(BARK_NRM);
        var leafTex   = AssetDatabase.LoadAssetAtPath<Texture2D>(LEAF_TEX);
        var leafNrm   = AssetDatabase.LoadAssetAtPath<Texture2D>(LEAF_NRM);
        var gravelTex = AssetDatabase.LoadAssetAtPath<Texture2D>(GRAVEL_TEX);
        var gravelNrm = AssetDatabase.LoadAssetAtPath<Texture2D>(GRAVEL_NRM);
        var stoneTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(STONE_TEX);
        var woodTex   = AssetDatabase.LoadAssetAtPath<Texture2D>(WOOD_TEX);

        // v16: PBR ground gri cikiyordu - procedural Perlin yesil grass'a geri don
        Material matGrass = GetOrCreateMat("Grass", GRASS_TINT, 0.05f, 0f,
            grassTex, grassNrm, new Vector2(25f, 25f), 0.5f);
        var grassTint = new Color(0.45f, 0.85f, 0.35f);
        matGrass.color = grassTint;
        if (matGrass.HasProperty("_Color")) matGrass.SetColor("_Color", grassTint);
        EditorUtility.SetDirty(matGrass);
        Material matHedge   = GetOrCreateMat("Hedge",       HEDGE_TINT,   0.08f, 0f, hedgeTex,  hedgeNrm,  new Vector2(2.5f, 1f), 0.8f);
        Material matTrunk   = GetOrCreateMat("TreeTrunk",   TRUNK_TINT,   0.06f, 0f, barkTex,   barkNrm,   new Vector2(1f, 1.5f), 1.0f);
        Material matFoliage = GetOrCreateMat("TreeFoliage", FOLIAGE_TINT, 0.08f, 0f, leafTex,   leafNrm,   new Vector2(3f, 3f),   0.7f);
        Material matGravel  = GetOrCreateMat("Gravel",      GRAVEL_TINT,  0.10f, 0f, gravelTex, gravelNrm, new Vector2(3f, 3f),   0.6f);
        Material matStone   = GetOrCreateMat("StoneCurb",   STONE_TINT,   0.10f, 0f, stoneTex,  null,      new Vector2(2f, 1f),   0f);
        Material matWood    = GetOrCreateMat("BenchWood",   WOOD_TINT,    0.15f, 0f, woodTex,   null,      new Vector2(2f, 1f),   0f);
        Material matMetal   = GetOrCreateMat("LampMetal",   METAL_TINT,   0.55f, 0.7f, null,    null,      new Vector2(1f, 1f),   0f);
        Material matGlobe   = GetOrCreateMat("LampGlobe",   GLOBE_TINT,   0.20f, 0f, null,      null,      new Vector2(1f, 1f),   0f);

        var root = new GameObject(LANDSCAPE_ROOT);
        var marker = new GameObject(VERSION_MARKER);
        marker.transform.SetParent(root.transform, false);

        var grassGroup    = new GameObject("Grass");      grassGroup.transform.SetParent(root.transform, false);
        var pathsGroup    = new GameObject("GravelPaths");pathsGroup.transform.SetParent(root.transform, false);
        var curbsGroup    = new GameObject("StoneCurbs"); curbsGroup.transform.SetParent(root.transform, false);
        var railsGroup    = new GameObject("MetalRailings"); railsGroup.transform.SetParent(root.transform, false);
        var treeGroup     = new GameObject("Trees");      treeGroup.transform.SetParent(root.transform, false);
        var lampGroup     = new GameObject("Lamps");      lampGroup.transform.SetParent(root.transform, false);

        BuildGrass(grassGroup.transform, matGrass);
        BuildGravelPaths(pathsGroup.transform, matGravel);
        BuildStoneCurbs(curbsGroup.transform, matStone);
        BuildMetalRailings(railsGroup.transform, matMetal);
        // v11: BuildHedges kaldirildi - ag ac gorusunu engelliyordu, yesil blok kotu duruyordu.
        BuildTrees(treeGroup.transform, matTrunk, matFoliage);
        // v25: Trafalgar lamb tekrar denendi - sadece 1 instance (149MB OBJ cok agir)
        BuildTrafalgarLamps(lampGroup.transform);

        MarkAllStatic(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Landscape] v3 cevre peyzaji kuruldu (zenginlestirilmis - lamba/bank/curb/path).");
    }

    // ============================================================
    // PEYZAJ GRUPLARI
    // ============================================================

    static void BuildGrass(Transform parent, Material mat)
    {
        // v6: TEK dev grass plane (150x150m) - pavement altinda + cevrenin her tarafinda.
        // Pavement onun ustunde (5mm yukseklikte) - gormez. Boylelikle hicbir
        // yerde skybox alta sızmaz. Tiling 20x20 (her tile ~7.5m).
        float yG = GROUND_Y - 0.005f;
        CreatePlane(parent, "Grass_Base",
            new Vector3(0f, yG, 0f),
            new Vector3(15f, 1f, 15f), mat); // Plane primitive 10x10 -> 150x150m
    }

    static void BuildGravelPaths(Transform parent, Material mat)
    {
        // Curb hemen disinde 1.5m genis yuruyus yolu (4 kenar)
        float pathW = 1.5f;
        float yPath = GROUND_Y - 0.003f;

        // Kuzey-guney
        CreatePlane(parent, "Path_North", new Vector3(0f, yPath,  PAVE_HALF_Z + 0.35f + pathW * 0.5f),
            new Vector3((PAVE_HALF_X * 2f + 1f) / 10f, 1f, pathW / 10f), mat);
        CreatePlane(parent, "Path_South", new Vector3(0f, yPath, -PAVE_HALF_Z - 0.35f - pathW * 0.5f),
            new Vector3((PAVE_HALF_X * 2f + 1f) / 10f, 1f, pathW / 10f), mat);
        // Dogu-bati (kosede curbe degmemesi icin daha kisa)
        float pathLen_EW = (PAVE_HALF_Z * 2f + 1f);
        CreatePlane(parent, "Path_East",  new Vector3( PAVE_HALF_X + 0.35f + pathW * 0.5f, yPath, 0f),
            new Vector3(pathW / 10f, 1f, pathLen_EW / 10f), mat);
        CreatePlane(parent, "Path_West",  new Vector3(-PAVE_HALF_X - 0.35f - pathW * 0.5f, yPath, 0f),
            new Vector3(pathW / 10f, 1f, pathLen_EW / 10f), mat);
    }

    static void BuildStoneCurbs(Transform parent, Material mat)
    {
        // Pavement hemen disinda alcak (15cm) tas bordur (4 kenar)
        float h = 0.15f;
        float w = 0.30f;
        float y = GROUND_Y + h * 0.5f;

        CreateCube(parent, "Curb_North", new Vector3(0f, y,  PAVE_HALF_Z + w * 0.5f),
            new Vector3(PAVE_HALF_X * 2f + w * 2f, h, w), mat);
        CreateCube(parent, "Curb_South", new Vector3(0f, y, -PAVE_HALF_Z - w * 0.5f),
            new Vector3(PAVE_HALF_X * 2f + w * 2f, h, w), mat);
        CreateCube(parent, "Curb_East",  new Vector3( PAVE_HALF_X + w * 0.5f, y, 0f),
            new Vector3(w, h, PAVE_HALF_Z * 2f), mat);
        CreateCube(parent, "Curb_West",  new Vector3(-PAVE_HALF_X - w * 0.5f, y, 0f),
            new Vector3(w, h, PAVE_HALF_Z * 2f), mat);
    }

    static void BuildHedges(Transform parent, Material mat)
    {
        // v5: Hedges artik KORKULUKLARIN DISINDA, yesil alanin on kenarinda.
        // Karakter korkuluk yuzunden onlara erişemez - guvenli + dekoratif.
        float hedgeH = 0.65f, hedgeT = 0.50f;
        float hedgeY = GROUND_Y + hedgeH * 0.5f;
        float outset = 2.5f; // korkuluk = PAVE_HALF + 0.35, hedge = PAVE_HALF + 2.5 -> grass icinde
        float lenN = PAVE_HALF_X * 2f + 4.5f;  // 24.5m kesintisiz
        float lenE = PAVE_HALF_Z * 2f + 4.5f;  // 44.5m kesintisiz

        CreateCube(parent, "Hedge_North",
            new Vector3(0f, hedgeY,  PAVE_HALF_Z + outset),
            new Vector3(lenN, hedgeH, hedgeT), mat);
        CreateCube(parent, "Hedge_South",
            new Vector3(0f, hedgeY, -PAVE_HALF_Z - outset),
            new Vector3(lenN, hedgeH, hedgeT), mat);
        CreateCube(parent, "Hedge_East",
            new Vector3( PAVE_HALF_X + outset, hedgeY, 0f),
            new Vector3(hedgeT, hedgeH, lenE), mat);
        CreateCube(parent, "Hedge_West",
            new Vector3(-PAVE_HALF_X - outset, hedgeY, 0f),
            new Vector3(hedgeT, hedgeH, lenE), mat);
    }

    static void BuildParterre(Transform parent, Material hedgeMat, Material stoneMat)
    {
        // 4 koseye buyuk dekoratif parterre (tas zemin + hedge cerceve)
        float baseSize = 2.4f, baseH = 0.18f;
        float innerSize = 1.8f, innerH = 0.45f;
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-PAVE_HALF_X + 2.5f, GROUND_Y,  PAVE_HALF_Z - 2.5f),
            new Vector3( PAVE_HALF_X - 2.5f, GROUND_Y,  PAVE_HALF_Z - 2.5f),
            new Vector3(-PAVE_HALF_X + 2.5f, GROUND_Y, -PAVE_HALF_Z + 2.5f),
            new Vector3( PAVE_HALF_X - 2.5f, GROUND_Y, -PAVE_HALF_Z + 2.5f),
        };
        for (int i = 0; i < positions.Length; i++)
        {
            // Tas base (genis, alcak)
            CreateCube(parent, $"Parterre_Base_{i:00}",
                positions[i] + new Vector3(0f, baseH * 0.5f, 0f),
                new Vector3(baseSize, baseH, baseSize), stoneMat);
            // Hedge kute (dar, yuksek - ortada)
            CreateCube(parent, $"Parterre_Hedge_{i:00}",
                positions[i] + new Vector3(0f, baseH + innerH * 0.5f, 0f),
                new Vector3(innerSize, innerH, innerSize), hedgeMat);
        }
    }

    static void BuildTrees(Transform parent, Material trunkMat, Material foliageMat)
    {
        // v7: GERCEK FBX agac modelleri (beech + oak)
        // Primitive agaclar tamamen kaldirildi.
        AssetDatabase.Refresh();

        // v24: oak.fbx 11MB - import import settings zorla, sonra reimport
        ConfigureFbxImport(OAK_FBX);
        ConfigureFbxImport(BEECH_FBX);

        var beechPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BEECH_FBX);
        var oakPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>(OAK_FBX);

        if (beechPrefab == null && oakPrefab == null)
        {
            Debug.LogError("[Landscape] Beech.fbx ve oak.fbx Assets/Models/Trees/ icinde bulunamadi!");
            return;
        }
        Debug.Log($"[Landscape] Prefab loaded: beech={(beechPrefab != null ? "OK" : "NULL")} oak={(oakPrefab != null ? "OK" : "NULL")}");
        if (beechPrefab == null) Debug.LogWarning("[Landscape] beech.fbx bulunamadi, sadece oak kullanilacak.");
        if (oakPrefab == null)   Debug.LogWarning("[Landscape] oak.fbx bulunamadi, sadece beech kullanilacak.");

        // Yaprak texture'larin alpha + sRGB ayarlarini garanti et
        ConfigureLeafTexture(BEECH_LEAF);
        ConfigureLeafTexture(OAK_LEAF);

        // Custom materyaller (FBX'in default materyallerinin yerine)
        var bark   = AssetDatabase.LoadAssetAtPath<Texture2D>(BEECH_BARK);
        var barkN  = AssetDatabase.LoadAssetAtPath<Texture2D>(BEECH_BARK_N);
        var leafB  = AssetDatabase.LoadAssetAtPath<Texture2D>(BEECH_LEAF);
        var leafBN = AssetDatabase.LoadAssetAtPath<Texture2D>(BEECH_LEAF_N);
        var leafO  = AssetDatabase.LoadAssetAtPath<Texture2D>(OAK_LEAF);

        if (barkN != null)
        {
            var nimp = AssetImporter.GetAtPath(BEECH_BARK_N) as TextureImporter;
            if (nimp != null && nimp.textureType != TextureImporterType.NormalMap)
            { nimp.textureType = TextureImporterType.NormalMap; nimp.SaveAndReimport(); }
        }
        if (leafBN != null)
        {
            var nimp = AssetImporter.GetAtPath(BEECH_LEAF_N) as TextureImporter;
            if (nimp != null && nimp.textureType != TextureImporterType.NormalMap)
            { nimp.textureType = TextureImporterType.NormalMap; nimp.SaveAndReimport(); }
        }

        // v29: bark sicak kahverengi tint (texture brown ama uzaktan gri gorunuyordu)
        Material matBark = GetOrCreateMat("BeechBark", new Color(0.85f, 0.65f, 0.45f), 0.10f, 0f,
            bark, barkN, new Vector2(1f, 1f), 1.0f);

        // v24: Oak kendi leaf01 texture'ine geri (v23 reverted). Sorun model yuklemiyor olmasi
        // v30: Oak'in gri leaf01 texture'ı autumn/kahverengi gorunuyordu.
        // Oak material'i BEECH leaf texture'i (gercek yesil) + alpha kullanir.
        // Geometri farkli oldugu icin gorsel cesitlilik yine var (oak genis canopy, beech ince).
        Material matLeafBeech = GetOrCreateCutoutMat("BeechLeaf", leafB, leafBN, 0.4f);
        Material matLeafOak   = GetOrCreateCutoutMat("OakLeaf",   leafB, leafBN, 0.4f);
        // v29: daha guclu yesil tint - yapraklar daha canli yesil olsun
        var leafBeechTint = new Color(0.55f, 1.00f, 0.40f);
        var leafOakTint   = new Color(0.40f, 0.95f, 0.30f);
        matLeafBeech.color = leafBeechTint;
        matLeafOak.color = leafOakTint;
        if (matLeafBeech.HasProperty("_Color")) matLeafBeech.SetColor("_Color", leafBeechTint);
        if (matLeafOak.HasProperty("_Color"))   matLeafOak.SetColor("_Color",   leafOakTint);
        EditorUtility.SetDirty(matLeafBeech);
        EditorUtility.SetDirty(matLeafOak);

        // === Yerlesim: 8 agac, hafif rastgele (tamamen simetrik degil) ===
        // System.Random sabit seed (tekrar olusturma gucunluk)
        var rand = new System.Random(20260523);

        // v10: ORMAN DENSITY - algoritmik random yerlesim
        // Pavement disindaki grass ring tum cevresine 34 agac (4 oak max + 30 beech)
        // Min spacing 2.6m, pavement'ten 1.8m uzak. Koseler dolu, karisik tipler yan yana.
        var placements = new System.Collections.Generic.List<(Vector3 pos, bool isOak)>();
        var usedPositions = new System.Collections.Generic.List<Vector3>();
        int targetTotal = 55;
        // v22: oak orani %20 -> %40, max 5 -> 15 (daha gorunur cesitlilik)
        int maxOak = 15;
        int oakPlaced = 0;
        int attempts = 0;

        while (placements.Count < targetTotal && attempts < 2500)
        {
            attempts++;
            // Grass ring icinde random nokta (-38..38 X/Z)
            float x = ((float)rand.NextDouble() - 0.5f) * 76f;
            float z = ((float)rand.NextDouble() - 0.5f) * 76f;

            // Pavement+korkuluk yakininda olmasin
            if (Mathf.Abs(x) < PAVE_HALF_X + 1.8f && Mathf.Abs(z) < PAVE_HALF_Z + 1.8f)
                continue;

            // Grass plane sınırları icinde kal (grass 150x150, ama 38'lik ring yeterli)
            if (Mathf.Abs(x) > 38f || Mathf.Abs(z) > 38f) continue;

            Vector3 pos = new Vector3(x, GROUND_Y, z);

            // Min spacing: 2.0m (orman density)
            bool tooClose = false;
            for (int p = 0; p < usedPositions.Count; p++)
            {
                if (Vector3.Distance(usedPositions[p], pos) < 2.0f)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // v24: Deterministic - her 3 agacta 1'i oak (random'a guvenme)
            // Yani index 0,3,6,9,... -> oak; digerleri -> beech
            bool isOak = (oakPlaced < maxOak) && (placements.Count % 3 == 0);
            if (isOak) oakPlaced++;

            placements.Add((pos, isOak));
            usedPositions.Add(pos);
        }
        Debug.Log($"[Landscape] v10: {placements.Count} agac yerlesti ({oakPlaced} oak + {placements.Count - oakPlaced} beech).");

        for (int i = 0; i < placements.Count; i++)
        {
            var (pos, isOak) = placements[i];
            var prefab = isOak ? oakPrefab : beechPrefab;
            if (prefab == null) prefab = isOak ? beechPrefab : oakPrefab; // fallback
            if (prefab == null) continue;

            // v22: Oak biraz daha buyuk (gorunur olsun) 0.10-0.18 -> 0.14-0.24
            float scale = isOak ? (0.14f + (float)rand.NextDouble() * 0.10f)
                                : (0.90f + (float)rand.NextDouble() * 0.25f);
            float rotY = (float)rand.NextDouble() * 360f;
            // Pozisyona kucuk rastgele jitter (±0.6m)
            float jx = ((float)rand.NextDouble() - 0.5f) * 1.2f;
            float jz = ((float)rand.NextDouble() - 0.5f) * 1.2f;
            Vector3 finalPos = pos + new Vector3(jx, 0f, jz);

            PlaceFbxTree(parent, prefab,
                $"{(isOak ? "Oak" : "Beech")}_{i:00}",
                finalPos, scale, rotY,
                matBark, isOak ? matLeafOak : matLeafBeech);
        }
    }

    static void PlaceFbxTree(Transform parent, GameObject prefab, string name,
        Vector3 pos, float scale, float rotY, Material bark, Material leaf)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        if (go == null) go = Object.Instantiate(prefab, parent);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale, scale, scale);
        go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        // v13: Prefab override sorununu engellemek icin unpack
        if (PrefabUtility.IsPartOfPrefabInstance(go))
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // v13: Sub-mesh tabanli ayrim - icinde 2 submesh varsa biri trunk biri yaprak.
        // Tek bir Mesh icinde 2 submesh -> sharedMaterials.Length == 2
        // Standart konvansiyon: submesh[0] = trunk (bark), submesh[1] = leaf
        foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
        {
            var origMats = mr.sharedMaterials;
            var newMats = new Material[origMats.Length];

            // Renderer-level hint: gameObject ismi veya mesh ismi
            string objName = mr.gameObject.name.ToLower();
            var mf = mr.GetComponent<MeshFilter>();
            string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name.ToLower() : "";
            bool rendererIsLeaf = objName.Contains("leaf") || objName.Contains("foliage") ||
                                   meshName.Contains("leaf") || meshName.Contains("foliage");
            bool rendererIsBark = objName.Contains("trunk") || objName.Contains("bark") || objName.Contains("wood") ||
                                   meshName.Contains("trunk") || meshName.Contains("bark");

            for (int i = 0; i < origMats.Length; i++)
            {
                bool isLeaf = false;
                if (origMats[i] != null)
                {
                    string mn = (origMats[i].name ?? "").ToLower();
                    if (mn.Contains("leaf") || mn.Contains("foliage") || mn.Contains("yaprak"))
                        isLeaf = true;
                    // Mat ana texture'i kontrol et
                    if (!isLeaf && origMats[i].mainTexture != null)
                    {
                        string tn = origMats[i].mainTexture.name.ToLower();
                        if (tn.Contains("leaf") || tn.Contains("foliage"))
                            isLeaf = true;
                    }
                }
                // Renderer hint (en guclu signal)
                if (rendererIsLeaf) isLeaf = true;
                else if (rendererIsBark) isLeaf = false;
                // Final fallback: 2 mat varsa slot 0 = bark, slot 1+ = leaf (standart)
                else if (!isLeaf && origMats.Length >= 2 && i >= 1) isLeaf = true;

                newMats[i] = isLeaf ? leaf : bark;
            }

            Debug.Log($"[Tree] '{go.name}' rend='{mr.gameObject.name}' mesh='{meshName}' " +
                      $"mats=[{string.Join(",", System.Array.ConvertAll(origMats, m => m == null ? "null" : m.name))}] " +
                      $"-> [{string.Join(",", System.Array.ConvertAll(newMats, m => m == null ? "null" : m.name))}]");

            mr.sharedMaterials = newMats;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        }
    }

    static void ConfigureFbxImport(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as ModelImporter;
        if (imp == null)
        {
            Debug.LogError($"[Landscape] FBX yuklenemedi (import yok): {path}");
            return;
        }
        bool changed = false;
        if (imp.materialImportMode != ModelImporterMaterialImportMode.None)
        {
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            changed = true;
        }
        if (imp.meshCompression != ModelImporterMeshCompression.Off)
        {
            // Decompress for faster load
            imp.meshCompression = ModelImporterMeshCompression.Off;
            changed = true;
        }
        if (changed)
        {
            imp.SaveAndReimport();
            Debug.Log($"[Landscape] FBX import settings updated: {path}");
        }
    }

    static void ConfigureLeafTexture(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        bool changed = false;
        if (imp.textureType != TextureImporterType.Default)
        {
            imp.textureType = TextureImporterType.Default;
            changed = true;
        }
        if (imp.alphaSource != TextureImporterAlphaSource.FromInput)
        {
            imp.alphaSource = TextureImporterAlphaSource.FromInput;
            changed = true;
        }
        if (!imp.alphaIsTransparency)
        {
            imp.alphaIsTransparency = true;
            changed = true;
        }
        if (!imp.sRGBTexture)
        {
            imp.sRGBTexture = true;
            changed = true;
        }
        if (changed) imp.SaveAndReimport();
    }

    static Material GetOrCreateCutoutMat(string name, Texture2D albedo, Texture2D normal, float cutoff)
    {
        string path = $"{MAT_DIR}/{name}.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.shader = Shader.Find("Standard");
        // Standard shader cutout mode setup
        m.SetFloat("_Mode", 1f); // 1 = Cutout
        m.SetOverrideTag("RenderType", "TransparentCutout");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        m.SetInt("_ZWrite", 1);
        m.EnableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 2450; // AlphaTest queue
        m.SetFloat("_Cutoff", cutoff);
        m.color = Color.white;
        if (m.HasProperty("_Color"))     m.SetColor("_Color", Color.white);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.10f);
        if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", 0f);
        if (albedo != null) { m.mainTexture = albedo; m.mainTextureScale = Vector2.one; }
        if (normal != null && m.HasProperty("_BumpMap"))
        {
            m.SetTexture("_BumpMap", normal);
            m.EnableKeyword("_NORMALMAP");
            if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", 1f);
        }
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    static void BuildLamps(Transform parent, Material metalMat, Material globeMat)
    {
        // v16: Klasik 6 lamba (primitive, garantili calisir)
        float lx = PAVE_HALF_X + 1.05f;
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-lx, GROUND_Y, -12f),
            new Vector3(-lx, GROUND_Y,   0f),
            new Vector3(-lx, GROUND_Y,  12f),
            new Vector3( lx, GROUND_Y, -12f),
            new Vector3( lx, GROUND_Y,   0f),
            new Vector3( lx, GROUND_Y,  12f),
        };
        for (int i = 0; i < positions.Length; i++)
            CreateLamp(parent, $"Lamp_{i:00}", positions[i], metalMat, globeMat);
    }

    static void BuildTrafalgarLamps(Transform parent)
    {
        // v17: OBJ 1M+ vertex (cok agır). Sadece 2 lamb. Z-up modellenmis -> X rot -90
        // Bounds tabanli ground snap.
        ConfigureLampImport();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LAMP_OBJ);
        if (prefab == null)
        {
            Debug.LogError("[Landscape] LAMP OBJ yuklenemedi: " + LAMP_OBJ);
            return;
        }
        Debug.Log("[Landscape] LAMP prefab yuklendi: " + prefab.name);

        var lampTex = AssetDatabase.LoadAssetAtPath<Texture2D>(LAMP_TEX);
        Material matLampObj = GetOrCreateMat("LampObj", new Color(0.35f, 0.32f, 0.28f), 0.30f, 0.5f,
            lampTex, null, Vector2.one, 0f);

        // OBJ bounds: X 11.78, Y 12.50, Z 23.73 (Z en buyuk = lamb yuksekligi, Z-up)
        // Hedef lamb yukseligi ~3m -> scale = 3/23.73 = 0.126
        float scale = 0.13f;

        // v29: Lamb COK disarida - korkuluga hic degmesin
        float ox = PAVE_HALF_X + 4.0f;
        float oz = PAVE_HALF_Z + 4.0f;
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-ox, GROUND_Y,  oz),    // NW kose
            new Vector3( ox, GROUND_Y,  oz),    // NE kose
            new Vector3(-ox, GROUND_Y, -oz),    // SW kose
            new Vector3( ox, GROUND_Y, -oz),    // SE kose
            new Vector3(-ox, GROUND_Y,  0f),    // W orta
            new Vector3( ox, GROUND_Y,  0f),    // E orta
        };
        // Avluya bakar (icine yonelik) - kose icin diyagonal, orta icin yatay
        float[] rotY = new float[] { 135f, -135f, 45f, -45f, 90f, -90f };

        for (int i = 0; i < positions.Length; i++)
        {
            // Parent GO + child prefab yapisi (rotation/offset duzeltme icin)
            GameObject lampParent = new GameObject($"TrafLamp_{i:00}");
            lampParent.transform.SetParent(parent, false);
            lampParent.transform.position = positions[i];
            lampParent.transform.rotation = Quaternion.Euler(0f, rotY[i], 0f);

            GameObject lamp = (GameObject)PrefabUtility.InstantiatePrefab(prefab, lampParent.transform);
            if (lamp == null) lamp = Object.Instantiate(prefab, lampParent.transform);
            lamp.name = "Mesh";
            // Z-up -> Y-up icin X -90 rotasyon
            lamp.transform.localPosition = Vector3.zero;
            lamp.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            lamp.transform.localScale = new Vector3(scale, scale, scale);

            if (PrefabUtility.IsPartOfPrefabInstance(lamp))
                PrefabUtility.UnpackPrefabInstance(lamp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Material atama
            foreach (var mr in lamp.GetComponentsInChildren<MeshRenderer>(true))
            {
                var origMats = mr.sharedMaterials;
                var newMats = new Material[origMats.Length];
                for (int j = 0; j < origMats.Length; j++)
                    newMats[j] = matLampObj;
                mr.sharedMaterials = newMats;
            }

            // Bounds tabanli ground snap - lamb alti GROUND_Y'da olsun
            var rends = lamp.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds combined = rends[0].bounds;
                for (int j = 1; j < rends.Length; j++) combined.Encapsulate(rends[j].bounds);
                float yShift = GROUND_Y - combined.min.y;
                lampParent.transform.position = new Vector3(positions[i].x, positions[i].y + yShift, positions[i].z);
                Debug.Log($"[Landscape] {lampParent.name}: bounds min.y={combined.min.y:F2} max.y={combined.max.y:F2} -> yShift={yShift:F2}");
            }
        }
    }

    static void BuildGrassClumps(Transform parent)
    {
        // v13: Gercek grass FBX (rostlinka_07c_ske) scatter - alpha cutout yapraklar
        ConfigureGrassClumpImport();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GRASS_CLUMP_FBX);
        if (prefab == null)
        {
            Debug.LogWarning("[Landscape] grass_clump.fbx bulunamadi: " + GRASS_CLUMP_FBX);
            return;
        }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(GRASS_CLUMP_TEX);
        if (tex == null)
        {
            Debug.LogWarning("[Landscape] grass_clump.png bulunamadi.");
            return;
        }

        // Cutout material + yesil tint (texture aslen yesil ama kontrast/canlilik icin)
        Material matClump = GetOrCreateCutoutMat("GrassClump", tex, null, 0.35f);
        var clumpTint = new Color(0.65f, 0.95f, 0.40f);
        matClump.color = clumpTint;
        if (matClump.HasProperty("_Color")) matClump.SetColor("_Color", clumpTint);
        EditorUtility.SetDirty(matClump);

        var rand = new System.Random(789456123);
        // v20: 100 (user istegi)
        int target = 100;
        int placed = 0;
        int attempts = 0;
        var used = new System.Collections.Generic.List<Vector3>();

        while (placed < target && attempts < 25000)
        {
            attempts++;
            float x = ((float)rand.NextDouble() - 0.5f) * 68f;
            float z = ((float)rand.NextDouble() - 0.5f) * 68f;
            // Pavement uzaginda olsun
            if (Mathf.Abs(x) < PAVE_HALF_X + 1.0f && Mathf.Abs(z) < PAVE_HALF_Z + 1.0f) continue;
            if (Mathf.Abs(x) > 34f || Mathf.Abs(z) > 34f) continue;

            Vector3 pos = new Vector3(x, GROUND_Y, z);
            // v20: 100 clump icin daha gevsek spacing 2.0m
            bool tooClose = false;
            for (int p = 0; p < used.Count; p++)
                if (Vector3.Distance(used[p], pos) < 2.0f) { tooClose = true; break; }
            if (tooClose) continue;

            // v18: Yeni Grass.fbx normal metre-scale modellenmis (sanirim)
            // Bounds-based ground snap ile farketmez, ama scale 0.3-1.0
            float scale = 0.3f + (float)rand.NextDouble() * 0.7f; // 0.3-1.0
            float rotY = (float)rand.NextDouble() * 360f;

            // v19: Hizli instantiate - PrefabUtility unpack yok, bounds snap yok (per-clump)
            GameObject clump = Object.Instantiate(prefab, parent);
            clump.name = $"Grass_{placed:00}";
            clump.transform.position = pos;
            clump.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            clump.transform.localScale = new Vector3(scale, scale, scale);

            // Material ata
            foreach (var mr in clump.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = mr.sharedMaterials;
                var newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++) newMats[i] = matClump;
                mr.sharedMaterials = newMats;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            }

            used.Add(pos);
            placed++;
        }
        Debug.Log($"[Landscape] v13: {placed} grass clump yerlesti.");
    }

    static void ConfigureGrassClumpImport()
    {
        // FBX scale - rostlinka_07c FBX'in scale'i unbekannt. Default 1.
        var mi = AssetImporter.GetAtPath(GRASS_CLUMP_FBX) as ModelImporter;
        if (mi != null && mi.materialImportMode != ModelImporterMaterialImportMode.None)
        {
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.SaveAndReimport();
        }
        // Texture: default + alpha + sRGB
        var ti = AssetImporter.GetAtPath(GRASS_CLUMP_TEX) as TextureImporter;
        if (ti != null)
        {
            bool changed = false;
            if (ti.textureType != TextureImporterType.Default) { ti.textureType = TextureImporterType.Default; changed = true; }
            if (ti.alphaSource != TextureImporterAlphaSource.FromInput) { ti.alphaSource = TextureImporterAlphaSource.FromInput; changed = true; }
            if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; changed = true; }
            if (!ti.sRGBTexture) { ti.sRGBTexture = true; changed = true; }
            if (changed) ti.SaveAndReimport();
        }
    }

    static void ConfigureLampImport()
    {
        var imp = AssetImporter.GetAtPath(LAMP_OBJ) as ModelImporter;
        if (imp == null) return;
        bool changed = false;
        // Unity 6.3: importMaterials obsolete -> materialImportMode kullanilir
        if (imp.materialImportMode != ModelImporterMaterialImportMode.None)
        {
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            changed = true;
        }
        if (changed) imp.SaveAndReimport();

        var ti = AssetImporter.GetAtPath(LAMP_TEX) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.Default)
        {
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.SaveAndReimport();
        }
    }

    static void ConfigureGroundPbrTextures()
    {
        // Diffuse: default sRGB
        var di = AssetImporter.GetAtPath(GROUND_PBR_DIFFUSE) as TextureImporter;
        if (di != null && (di.textureType != TextureImporterType.Default || !di.sRGBTexture))
        {
            di.textureType = TextureImporterType.Default;
            di.sRGBTexture = true;
            di.wrapMode = TextureWrapMode.Repeat;
            di.SaveAndReimport();
        }
        // Normal map
        var ni = AssetImporter.GetAtPath(GROUND_PBR_NORMAL) as TextureImporter;
        if (ni != null && ni.textureType != TextureImporterType.NormalMap)
        {
            ni.textureType = TextureImporterType.NormalMap;
            ni.wrapMode = TextureWrapMode.Repeat;
            ni.SaveAndReimport();
        }
        // Occlusion: default linear (sRGB false)
        var oi = AssetImporter.GetAtPath(GROUND_PBR_AO) as TextureImporter;
        if (oi != null && (oi.textureType != TextureImporterType.Default || oi.sRGBTexture))
        {
            oi.textureType = TextureImporterType.Default;
            oi.sRGBTexture = false;
            oi.wrapMode = TextureWrapMode.Repeat;
            oi.SaveAndReimport();
        }
    }

    static Material GetOrCreateGroundPbrMat(string name, Texture2D albedo, Texture2D normal,
        Texture2D occlusion, Vector2 tiling)
    {
        string path = $"{MAT_DIR}/{name}.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = Color.white;
        if (m.HasProperty("_Color"))     m.SetColor("_Color", Color.white);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
        if (m.HasProperty("_Metallic"))  m.SetFloat("_Metallic", 0f);
        if (albedo != null) { m.mainTexture = albedo; m.mainTextureScale = tiling; }
        if (normal != null && m.HasProperty("_BumpMap"))
        {
            m.SetTexture("_BumpMap", normal);
            m.SetTextureScale("_BumpMap", tiling);
            m.EnableKeyword("_NORMALMAP");
            if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", 1.0f);
        }
        if (occlusion != null && m.HasProperty("_OcclusionMap"))
        {
            m.SetTexture("_OcclusionMap", occlusion);
            m.SetTextureScale("_OcclusionMap", tiling);
            if (m.HasProperty("_OcclusionStrength")) m.SetFloat("_OcclusionStrength", 1.0f);
        }
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    static void BuildMetalRailings(Transform parent, Material metalMat)
    {
        // v6: Daha UZUN demir korkuluk (1.5m) + arkasinda gorunmez 2.5m duvar collider
        // -> karakter zıplayarak da gecemez
        float railY1 = GROUND_Y + 1.50f;   // ust ray (1.5m)
        float railY2 = GROUND_Y + 0.30f;   // alt ray (30cm)
        float postBaseY = GROUND_Y + 0.55f;
        float curbOuter = 0.35f;

        // === N edge
        BuildRailingSegment(parent, "Rail_N",
            new Vector3(-PAVE_HALF_X - curbOuter, postBaseY, PAVE_HALF_Z + curbOuter),
            new Vector3( PAVE_HALF_X + curbOuter, postBaseY, PAVE_HALF_Z + curbOuter),
            railY1, railY2, metalMat, isVertical: false);
        BuildInvisibleWall(parent, "InvWall_N",
            new Vector3(-PAVE_HALF_X - curbOuter, GROUND_Y, PAVE_HALF_Z + curbOuter),
            new Vector3( PAVE_HALF_X + curbOuter, GROUND_Y, PAVE_HALF_Z + curbOuter),
            isVertical: false);

        // === S edge
        BuildRailingSegment(parent, "Rail_S",
            new Vector3(-PAVE_HALF_X - curbOuter, postBaseY, -PAVE_HALF_Z - curbOuter),
            new Vector3( PAVE_HALF_X + curbOuter, postBaseY, -PAVE_HALF_Z - curbOuter),
            railY1, railY2, metalMat, isVertical: false);
        BuildInvisibleWall(parent, "InvWall_S",
            new Vector3(-PAVE_HALF_X - curbOuter, GROUND_Y, -PAVE_HALF_Z - curbOuter),
            new Vector3( PAVE_HALF_X + curbOuter, GROUND_Y, -PAVE_HALF_Z - curbOuter),
            isVertical: false);

        // === E edge
        BuildRailingSegment(parent, "Rail_E",
            new Vector3( PAVE_HALF_X + curbOuter, postBaseY, -PAVE_HALF_Z - curbOuter),
            new Vector3( PAVE_HALF_X + curbOuter, postBaseY,  PAVE_HALF_Z + curbOuter),
            railY1, railY2, metalMat, isVertical: true);
        BuildInvisibleWall(parent, "InvWall_E",
            new Vector3( PAVE_HALF_X + curbOuter, GROUND_Y, -PAVE_HALF_Z - curbOuter),
            new Vector3( PAVE_HALF_X + curbOuter, GROUND_Y,  PAVE_HALF_Z + curbOuter),
            isVertical: true);

        // === W edge
        BuildRailingSegment(parent, "Rail_W",
            new Vector3(-PAVE_HALF_X - curbOuter, postBaseY, -PAVE_HALF_Z - curbOuter),
            new Vector3(-PAVE_HALF_X - curbOuter, postBaseY,  PAVE_HALF_Z + curbOuter),
            railY1, railY2, metalMat, isVertical: true);
        BuildInvisibleWall(parent, "InvWall_W",
            new Vector3(-PAVE_HALF_X - curbOuter, GROUND_Y, -PAVE_HALF_Z - curbOuter),
            new Vector3(-PAVE_HALF_X - curbOuter, GROUND_Y,  PAVE_HALF_Z + curbOuter),
            isVertical: true);
    }

    static void BuildInvisibleWall(Transform parent, string name, Vector3 start, Vector3 end, bool isVertical)
    {
        // Gorunmez yuksek collider duvari - karakter zıplayarak da gecemez
        float wallHeight = 3.0f;
        float wallThickness = 0.25f;
        Vector3 mid = (start + end) * 0.5f;
        float length = Vector3.Distance(start, end);
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = new Vector3(mid.x, GROUND_Y + wallHeight * 0.5f, mid.z);
        wall.transform.localScale = isVertical
            ? new Vector3(wallThickness, wallHeight, length)
            : new Vector3(length, wallHeight, wallThickness);
        // Gorunmez yap - MeshRenderer'i destroy et, BoxCollider kalir
        var mr = wall.GetComponent<MeshRenderer>();
        if (mr != null) Object.DestroyImmediate(mr);
        var mf = wall.GetComponent<MeshFilter>();
        if (mf != null) Object.DestroyImmediate(mf);
    }

    static void BuildRailingSegment(Transform parent, string nameBase,
        Vector3 start, Vector3 end, float topRailY, float botRailY, Material mat, bool isVertical)
    {
        Vector3 mid = (start + end) * 0.5f;
        float length = Vector3.Distance(start, end);

        // Top rail (long thin cube)
        Vector3 topPos = new Vector3(mid.x, topRailY, mid.z);
        Vector3 topScale = isVertical ? new Vector3(0.05f, 0.05f, length)
                                       : new Vector3(length, 0.05f, 0.05f);
        CreateCube(parent, $"{nameBase}_TopRail", topPos, topScale, mat);

        // Bottom rail
        Vector3 botPos = new Vector3(mid.x, botRailY, mid.z);
        Vector3 botScale = isVertical ? new Vector3(0.04f, 0.04f, length)
                                       : new Vector3(length, 0.04f, 0.04f);
        CreateCube(parent, $"{nameBase}_BotRail", botPos, botScale, mat);

        // Posts every 2.5m (sturdy square posts + finial)
        float postSpacing = 2.5f;
        int numPosts = Mathf.Max(2, Mathf.FloorToInt(length / postSpacing) + 1);
        for (int i = 0; i < numPosts; i++)
        {
            float t = (numPosts == 1) ? 0.5f : (float)i / (numPosts - 1);
            Vector3 pPos = Vector3.Lerp(start, end, t);
            pPos.y = (topRailY + GROUND_Y + 0.05f) * 0.5f; // ortala
            float postH = topRailY - (GROUND_Y + 0.05f) + 0.08f;
            CreateCube(parent, $"{nameBase}_Post_{i:00}",
                pPos, new Vector3(0.10f, postH, 0.10f), mat);
            // Finial (kucuk kup ust)
            CreateCube(parent, $"{nameBase}_Finial_{i:00}",
                new Vector3(pPos.x, topRailY + 0.10f, pPos.z),
                new Vector3(0.14f, 0.10f, 0.14f), mat);
            // Spike (cylinder)
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = $"{nameBase}_Spike_{i:00}";
            spike.transform.SetParent(parent, false);
            spike.transform.position = new Vector3(pPos.x, topRailY + 0.22f, pPos.z);
            spike.transform.localScale = new Vector3(0.04f, 0.06f, 0.04f);
            ApplyMaterial(spike, mat);
        }

        // Balusters (vertical bars) - klasik korkuluk gorunumu icin
        float balSpacing = 0.30f;
        int numBals = Mathf.FloorToInt(length / balSpacing);
        for (int i = 0; i < numBals; i++)
        {
            float t = (i + 0.5f) / numBals;
            Vector3 bPos = Vector3.Lerp(start, end, t);
            float balH = topRailY - botRailY;
            bPos.y = (topRailY + botRailY) * 0.5f;
            CreateCube(parent, $"{nameBase}_Bal_{i:00}",
                bPos, new Vector3(0.025f, balH, 0.025f), mat);
        }
    }

    static void BuildShrubs(Transform parent, Material foliageMat)
    {
        // Lamba diplerine ve bos noktalara kucuk dekoratif top calilar
        float lx = PAVE_HALF_X + 1.05f;
        float r = 0.55f;
        // Lamba diplerinde (6 lamba x 1 calı yan tarafta)
        Vector3[] shrubs = new Vector3[]
        {
            new Vector3(-lx + 0.9f, GROUND_Y + r * 0.7f, -12f),
            new Vector3(-lx + 0.9f, GROUND_Y + r * 0.7f,   0f),
            new Vector3(-lx + 0.9f, GROUND_Y + r * 0.7f,  12f),
            new Vector3( lx - 0.9f, GROUND_Y + r * 0.7f, -12f),
            new Vector3( lx - 0.9f, GROUND_Y + r * 0.7f,   0f),
            new Vector3( lx - 0.9f, GROUND_Y + r * 0.7f,  12f),
            // Avlu kuzey-guney ortasinda hedge bosluk yaninda
            new Vector3(-2.2f, GROUND_Y + r * 0.7f,  PAVE_HALF_Z - 1.0f),
            new Vector3( 2.2f, GROUND_Y + r * 0.7f,  PAVE_HALF_Z - 1.0f),
            new Vector3(-2.2f, GROUND_Y + r * 0.7f, -PAVE_HALF_Z + 1.0f),
            new Vector3( 2.2f, GROUND_Y + r * 0.7f, -PAVE_HALF_Z + 1.0f),
        };
        for (int i = 0; i < shrubs.Length; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = $"Shrub_{i:00}";
            s.transform.SetParent(parent, false);
            s.transform.position = shrubs[i];
            s.transform.localScale = new Vector3(r * 2f, r * 1.4f, r * 2f);
            var col = s.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            ApplyMaterial(s, foliageMat);
        }
    }

    // ============================================================
    // PRIMITIVE CREATORS
    // ============================================================

    static GameObject CreatePlane(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        ApplyMaterial(go, mat);
        return go;
    }

    static GameObject CreateCube(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        ApplyMaterial(go, mat);
        return go;
    }

    static GameObject CreateTree(Transform parent, string name, Vector3 basePos, int treeType,
                                  int randSeed, Material trunkMat, Material foliageMat)
    {
        var treeRoot = new GameObject(name);
        treeRoot.transform.SetParent(parent, false);
        treeRoot.transform.position = basePos;
        var rand = new System.Random(randSeed);
        float rotY = (float)(rand.NextDouble() * 360f);
        treeRoot.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        if (treeType == 0) BuildColumnarTree(treeRoot.transform, rand, trunkMat, foliageMat);
        else if (treeType == 1) BuildRoundTree(treeRoot.transform, rand, trunkMat, foliageMat);
        else BuildTopiaryTree(treeRoot.transform, rand, trunkMat, foliageMat);

        return treeRoot;
    }

    static void BuildColumnarTree(Transform root, System.Random rand, Material trunkMat, Material foliageMat)
    {
        // Sutun agac (cypress benzeri): ince yuksek govde + 4 sphere yukseginde sıkı dizilim
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(root, false);
        trunk.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        trunk.transform.localScale = new Vector3(0.18f, 0.9f, 0.18f);
        ApplyMaterial(trunk, trunkMat);

        // 4 sphere yukari dogru daralan
        float[] ys     = new float[] { 1.6f, 2.4f, 3.2f, 3.85f };
        float[] scales = new float[] { 0.95f, 0.85f, 0.70f, 0.45f };
        for (int i = 0; i < ys.Length; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = $"Foliage_{i}";
            s.transform.SetParent(root, false);
            s.transform.localPosition = new Vector3(
                ((float)rand.NextDouble() - 0.5f) * 0.10f,
                ys[i],
                ((float)rand.NextDouble() - 0.5f) * 0.10f);
            float sc = scales[i] * (0.95f + (float)rand.NextDouble() * 0.10f);
            s.transform.localScale = new Vector3(sc, sc * 1.15f, sc);
            var col = s.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            ApplyMaterial(s, foliageMat);
        }
    }

    static void BuildRoundTree(Transform root, System.Random rand, Material trunkMat, Material foliageMat)
    {
        // Yuvarlak dogal agac (mesalim) - 5 sphere organik kume
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(root, false);
        trunk.transform.localPosition = new Vector3(0f, 1.15f, 0f);
        trunk.transform.localScale = new Vector3(0.22f, 1.15f, 0.22f);
        ApplyMaterial(trunk, trunkMat);

        Vector3[] offsets = new Vector3[]
        {
            new Vector3( 0.00f, 2.85f,  0.00f),
            new Vector3( 0.55f, 2.60f,  0.10f),
            new Vector3(-0.50f, 2.55f,  0.20f),
            new Vector3( 0.10f, 2.65f, -0.55f),
            new Vector3(-0.20f, 2.95f,  0.35f),
        };
        float[] scales = new float[] { 1.55f, 1.20f, 1.25f, 1.15f, 1.05f };
        for (int i = 0; i < offsets.Length; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = $"Foliage_{i}";
            s.transform.SetParent(root, false);
            Vector3 jitter = new Vector3(
                ((float)rand.NextDouble() - 0.5f) * 0.14f,
                ((float)rand.NextDouble() - 0.5f) * 0.10f,
                ((float)rand.NextDouble() - 0.5f) * 0.14f);
            s.transform.localPosition = offsets[i] + jitter;
            float sc = scales[i] * (0.92f + (float)rand.NextDouble() * 0.16f);
            float yMul = (i == 0) ? 1.10f : 1.00f;
            s.transform.localScale = new Vector3(sc, sc * yMul, sc);
            var col = s.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            ApplyMaterial(s, foliageMat);
        }
    }

    static void BuildTopiaryTree(Transform root, System.Random rand, Material trunkMat, Material foliageMat)
    {
        // Topiary (formal kesim) - kisa govde + tek buyuk sphere top
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(root, false);
        trunk.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        trunk.transform.localScale = new Vector3(0.16f, 0.55f, 0.16f);
        ApplyMaterial(trunk, trunkMat);

        // Tek buyuk yuvarlak (formal kesim, hafif oval)
        var top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        top.name = "Foliage_Main";
        top.transform.SetParent(root, false);
        top.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        top.transform.localScale = new Vector3(1.40f, 1.30f, 1.40f);
        var col = top.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        ApplyMaterial(top, foliageMat);

        // Ust kucuk knot (gercekciligi artiran)
        var knot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knot.name = "Foliage_Top";
        knot.transform.SetParent(root, false);
        knot.transform.localPosition = new Vector3(0f, 2.55f, 0f);
        knot.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        var col2 = knot.GetComponent<Collider>();
        if (col2 != null) Object.DestroyImmediate(col2);
        ApplyMaterial(knot, foliageMat);
    }

    static void CreateLamp(Transform parent, string name, Vector3 basePos, Material metalMat, Material globeMat)
    {
        // Klasik Paris tarzi sokak lambasi: base + post + cap + globe (4 parca)
        var lampRoot = new GameObject(name);
        lampRoot.transform.SetParent(parent, false);
        lampRoot.transform.position = basePos;

        // Base (kalin alt taban)
        var basePart = GameObject.CreatePrimitive(PrimitiveType.Cube);
        basePart.name = "Base";
        basePart.transform.SetParent(lampRoot.transform, false);
        basePart.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        basePart.transform.localScale = new Vector3(0.35f, 0.30f, 0.35f);
        ApplyMaterial(basePart, metalMat);

        // Post (uzun ince silindir)
        var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "Post";
        post.transform.SetParent(lampRoot.transform, false);
        post.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        post.transform.localScale = new Vector3(0.08f, 1.30f, 0.08f);
        ApplyMaterial(post, metalMat);

        // Cap (post ustu kucuk seki)
        var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "Cap";
        cap.transform.SetParent(lampRoot.transform, false);
        cap.transform.localPosition = new Vector3(0f, 3.00f, 0f);
        cap.transform.localScale = new Vector3(0.25f, 0.06f, 0.25f);
        ApplyMaterial(cap, metalMat);

        // Globe (kure ışık golgeligi)
        var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        globe.name = "Globe";
        globe.transform.SetParent(lampRoot.transform, false);
        globe.transform.localPosition = new Vector3(0f, 3.30f, 0f);
        globe.transform.localScale = new Vector3(0.35f, 0.40f, 0.35f);
        var gcol = globe.GetComponent<Collider>();
        if (gcol != null) Object.DestroyImmediate(gcol);
        ApplyMaterial(globe, globeMat);

        // Cap top spike (sade dekoratif)
        var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spike.name = "Finial";
        spike.transform.SetParent(lampRoot.transform, false);
        spike.transform.localPosition = new Vector3(0f, 3.70f, 0f);
        spike.transform.localScale = new Vector3(0.04f, 0.10f, 0.04f);
        ApplyMaterial(spike, metalMat);
    }

    static void CreateBench(Transform parent, string name, Vector3 basePos, float rotY,
                            Material stoneMat, Material woodMat)
    {
        // Sade tas bacakli ahsap oturma bank: 2 yan ayak + 1 seat + 1 backrest
        var benchRoot = new GameObject(name);
        benchRoot.transform.SetParent(parent, false);
        benchRoot.transform.position = basePos;
        benchRoot.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        // Sol ayak
        var legL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legL.name = "Leg_L";
        legL.transform.SetParent(benchRoot.transform, false);
        legL.transform.localPosition = new Vector3(-0.65f, 0.22f, 0f);
        legL.transform.localScale = new Vector3(0.18f, 0.44f, 0.40f);
        ApplyMaterial(legL, stoneMat);

        // Sag ayak
        var legR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legR.name = "Leg_R";
        legR.transform.SetParent(benchRoot.transform, false);
        legR.transform.localPosition = new Vector3(0.65f, 0.22f, 0f);
        legR.transform.localScale = new Vector3(0.18f, 0.44f, 0.40f);
        ApplyMaterial(legR, stoneMat);

        // Oturak (ahsap)
        var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "Seat";
        seat.transform.SetParent(benchRoot.transform, false);
        seat.transform.localPosition = new Vector3(0f, 0.48f, 0f);
        seat.transform.localScale = new Vector3(1.70f, 0.07f, 0.42f);
        ApplyMaterial(seat, woodMat);

        // Arkalik
        var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.name = "Back";
        back.transform.SetParent(benchRoot.transform, false);
        back.transform.localPosition = new Vector3(0f, 0.78f, -0.18f);
        back.transform.localScale = new Vector3(1.70f, 0.50f, 0.05f);
        ApplyMaterial(back, woodMat);
    }

    static void ApplyMaterial(GameObject go, Material mat)
    {
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;
    }

    static void MarkAllStatic(GameObject root)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.isStatic = true;
    }

    static void EnsureDir(string p)
    {
        if (!Directory.Exists(p))
        {
            Directory.CreateDirectory(p);
            AssetDatabase.Refresh();
        }
    }

    static void EnsureTexture(string path, int size, bool isNormal, System.Func<Texture2D> gen)
    {
        if (File.Exists(path)) return;
        Debug.Log($"[Landscape] Texture uretiliyor: {Path.GetFileName(path)}");
        var tex = gen();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            imp.sRGBTexture = !isNormal;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.filterMode = FilterMode.Trilinear;
            imp.anisoLevel = 4;
            imp.mipmapEnabled = true;
            imp.maxTextureSize = size;
            imp.SaveAndReimport();
        }
    }

    static Material GetOrCreateMat(string name, Color tint, float smooth, float metallic,
        Texture2D albedo, Texture2D normal, Vector2 tiling, float bumpScale)
    {
        string path = $"{MAT_DIR}/{name}.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = tint;
        if (m.HasProperty("_Color"))      m.SetColor("_Color", tint);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", metallic);
        if (albedo != null) { m.mainTexture = albedo; m.mainTextureScale = tiling; }
        else { m.mainTexture = null; }
        if (normal != null && m.HasProperty("_BumpMap"))
        {
            m.SetTexture("_BumpMap", normal);
            m.SetTextureScale("_BumpMap", tiling);
            m.EnableKeyword("_NORMALMAP");
            if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", bumpScale);
        }
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    // ============================================================
    // PROCEDURAL TEXTURE GENERATORS
    // ============================================================

    static Texture2D GenerateGrassAlbedo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
            float n2 = Mathf.PerlinNoise(x * 0.12f + 100, y * 0.12f + 100);
            float n3 = Mathf.PerlinNoise(x * 0.35f + 200, y * 0.35f + 200);
            float patch = Mathf.PerlinNoise(x * 0.012f + 50, y * 0.012f + 50);
            float blade = (Mathf.PerlinNoise(x * 0.8f + 300, y * 0.8f + 300) - 0.5f) * 0.04f;

            float r = 0.26f + n1 * 0.10f + (n2 - 0.5f) * 0.05f;
            float g = 0.46f + n1 * 0.16f + (n2 - 0.5f) * 0.07f - (n3 - 0.5f) * 0.04f;
            float b = 0.18f + n1 * 0.08f + (n2 - 0.5f) * 0.03f;

            if (patch > 0.65f) { r += 0.04f; g += 0.10f; b += 0.03f; }
            else if (patch < 0.30f) { r -= 0.05f; g -= 0.09f; b -= 0.05f; }
            r += blade; g += blade; b += blade;

            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateGrassNormal(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, true);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float h00 = Mathf.PerlinNoise(x * 0.25f, y * 0.25f);
            float h10 = Mathf.PerlinNoise((x + 1) * 0.25f, y * 0.25f);
            float h01 = Mathf.PerlinNoise(x * 0.25f, (y + 1) * 0.25f);
            float dx = (h10 - h00) * 0.7f;
            float dy = (h01 - h00) * 0.7f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(0.5f + dx), Mathf.Clamp01(0.5f + dy), 1f, 1f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateHedgeAlbedo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n1 = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
            float n2 = Mathf.PerlinNoise(x * 0.20f + 100, y * 0.20f + 100);
            float leaf = Mathf.PerlinNoise(x * 0.55f + 200, y * 0.55f + 200);
            float dark = Mathf.PerlinNoise(x * 0.15f + 300, y * 0.15f + 300);

            float r = 0.14f + n1 * 0.09f + (n2 - 0.5f) * 0.05f;
            float g = 0.34f + n1 * 0.13f + (n2 - 0.5f) * 0.09f;
            float b = 0.12f + n1 * 0.07f + (n2 - 0.5f) * 0.03f;

            if (dark < 0.35f) { r -= 0.08f; g -= 0.12f; b -= 0.06f; }
            if (leaf > 0.65f) { r += 0.05f; g += 0.08f; b += 0.04f; }

            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateHedgeNormal(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, true);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float h00 = Mathf.PerlinNoise(x * 0.15f, y * 0.15f)
                      + Mathf.PerlinNoise(x * 0.45f, y * 0.45f) * 0.5f;
            float h10 = Mathf.PerlinNoise((x + 1) * 0.15f, y * 0.15f)
                      + Mathf.PerlinNoise((x + 1) * 0.45f, y * 0.45f) * 0.5f;
            float h01 = Mathf.PerlinNoise(x * 0.15f, (y + 1) * 0.15f)
                      + Mathf.PerlinNoise(x * 0.45f, (y + 1) * 0.45f) * 0.5f;
            float dx = (h10 - h00) * 0.7f;
            float dy = (h01 - h00) * 0.7f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(0.5f + dx), Mathf.Clamp01(0.5f + dy), 1f, 1f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateBarkAlbedo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float stripe = Mathf.PerlinNoise(x * 0.08f, y * 0.015f);
            float crack  = Mathf.PerlinNoise(x * 0.35f + 50, y * 0.06f + 50);
            float fine   = (Mathf.PerlinNoise(x * 1.0f + 200, y * 0.3f + 200) - 0.5f) * 0.05f;
            bool isCrack = crack < 0.30f;

            float r = 0.40f + stripe * 0.15f - (isCrack ? 0.18f : 0f);
            float g = 0.27f + stripe * 0.10f - (isCrack ? 0.13f : 0f);
            float b = 0.16f + stripe * 0.07f - (isCrack ? 0.10f : 0f);
            r += fine; g += fine; b += fine;
            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateBarkNormal(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, true);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float h00 = Mathf.PerlinNoise(x * 0.18f, y * 0.04f);
            float h10 = Mathf.PerlinNoise((x + 1) * 0.18f, y * 0.04f);
            float h01 = Mathf.PerlinNoise(x * 0.18f, (y + 1) * 0.04f);
            float dx = (h10 - h00) * 1.2f;
            float dy = (h01 - h00) * 0.4f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(0.5f + dx), Mathf.Clamp01(0.5f + dy), 1f, 1f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateFoliageAlbedo(int size)
    {
        // v4: cok daha gorunur yaprak izleri (yuksek kontrast + kucuk yaprak kumeleri)
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Buyuk yaprak kumeleri
            float cluster  = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
            // Orta detay
            float midDet   = Mathf.PerlinNoise(x * 0.25f + 100, y * 0.25f + 100);
            // Kucuk yaprak ayrim cizgileri (en gorunur olan)
            float leafGap  = Mathf.PerlinNoise(x * 1.2f + 200, y * 1.2f + 200);
            // Yaprak govde isareti
            float leafBody = Mathf.PerlinNoise(x * 0.6f + 300, y * 0.6f + 300);
            // Aydınlık yaprak yuzeyi
            float bright   = Mathf.PerlinNoise(x * 0.18f + 400, y * 0.18f + 400);

            // Base yesil ton (orta canli)
            float r = 0.25f + cluster * 0.10f + (midDet - 0.5f) * 0.06f;
            float g = 0.46f + cluster * 0.18f + (midDet - 0.5f) * 0.12f;
            float b = 0.20f + cluster * 0.10f + (midDet - 0.5f) * 0.04f;

            // Yaprak ayrim cizgileri - guclu koyu (KEY: yaprak izleri burada)
            if (leafGap < 0.32f)
            {
                float t = (0.32f - leafGap) / 0.32f;
                r -= 0.16f * t; g -= 0.24f * t; b -= 0.13f * t;
            }

            // Yaprak govdesi - hafif koyu/medium
            if (leafBody < 0.42f)
            {
                r -= 0.05f; g -= 0.08f; b -= 0.04f;
            }

            // Aydinlik yaprak vurgu
            if (bright > 0.72f)
            {
                float t = (bright - 0.72f) / 0.28f;
                r += 0.10f * t; g += 0.16f * t; b += 0.08f * t;
            }

            // Sarimsi tek tuk yaprak (sonbahar dokunusu)
            if (cluster > 0.78f && leafBody > 0.65f)
            {
                r += 0.06f; g += 0.02f; b -= 0.04f;
            }

            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateFoliageNormal(int size)
    {
        // Yaprak kabarklığı icin normal map (cok katmanlı Perlin)
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, true);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Kucuk yaprak yumrukleri (key)
            float h00 = Mathf.PerlinNoise(x * 0.8f, y * 0.8f) * 0.8f
                      + Mathf.PerlinNoise(x * 1.8f, y * 1.8f) * 0.4f
                      + Mathf.PerlinNoise(x * 0.25f, y * 0.25f) * 0.3f;
            float h10 = Mathf.PerlinNoise((x + 1) * 0.8f, y * 0.8f) * 0.8f
                      + Mathf.PerlinNoise((x + 1) * 1.8f, y * 1.8f) * 0.4f
                      + Mathf.PerlinNoise((x + 1) * 0.25f, y * 0.25f) * 0.3f;
            float h01 = Mathf.PerlinNoise(x * 0.8f, (y + 1) * 0.8f) * 0.8f
                      + Mathf.PerlinNoise(x * 1.8f, (y + 1) * 1.8f) * 0.4f
                      + Mathf.PerlinNoise(x * 0.25f, (y + 1) * 0.25f) * 0.3f;
            float dx = (h10 - h00) * 1.5f;
            float dy = (h01 - h00) * 1.5f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(0.5f + dx), Mathf.Clamp01(0.5f + dy), 1f, 1f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // === YENI: Gravel, Stone, Wood ===

    static Texture2D GenerateGravelAlbedo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Tane tane Perlin + dark/light spots = cakil/gravel
            float n1 = Mathf.PerlinNoise(x * 0.30f, y * 0.30f);
            float n2 = Mathf.PerlinNoise(x * 0.8f + 100, y * 0.8f + 100);
            float n3 = Mathf.PerlinNoise(x * 1.6f + 200, y * 1.6f + 200);
            float patch = Mathf.PerlinNoise(x * 0.04f + 50, y * 0.04f + 50);

            float base_ = 0.52f + (patch - 0.5f) * 0.08f;
            float fine = (n1 - 0.5f) * 0.10f + (n2 - 0.5f) * 0.07f + (n3 - 0.5f) * 0.05f;
            float v = base_ + fine;

            // Sicak gri ton
            float r = v * 1.02f;
            float g = v * 1.00f;
            float b = v * 0.94f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateGravelNormal(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, true);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float h00 = Mathf.PerlinNoise(x * 0.6f, y * 0.6f) +
                        Mathf.PerlinNoise(x * 1.3f + 50, y * 1.3f + 50) * 0.5f;
            float h10 = Mathf.PerlinNoise((x+1) * 0.6f, y * 0.6f) +
                        Mathf.PerlinNoise((x+1) * 1.3f + 50, y * 1.3f + 50) * 0.5f;
            float h01 = Mathf.PerlinNoise(x * 0.6f, (y+1) * 0.6f) +
                        Mathf.PerlinNoise(x * 1.3f + 50, (y+1) * 1.3f + 50) * 0.5f;
            float dx = (h10 - h00) * 0.9f;
            float dy = (h01 - h00) * 0.9f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(0.5f + dx), Mathf.Clamp01(0.5f + dy), 1f, 1f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateStoneAlbedo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Sicak bej tas ton (Louvre cephesi rengine yakin)
            float n1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
            float n2 = Mathf.PerlinNoise(x * 0.2f + 100, y * 0.2f + 100);
            float grit = (Mathf.PerlinNoise(x * 1.5f + 200, y * 1.5f + 200) - 0.5f) * 0.04f;
            float dark = Mathf.PerlinNoise(x * 0.012f + 50, y * 0.012f + 50);

            float v = 0.72f + n1 * 0.08f + (n2 - 0.5f) * 0.05f + grit;
            if (dark < 0.30f) v -= 0.06f;

            float r = v;
            float g = v * 0.97f;
            float b = v * 0.91f;
            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateWoodAlbedo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Yatay tahta cizgileri (rotated bank icin uygun)
            float grain = Mathf.PerlinNoise(x * 0.02f, y * 0.18f);
            float crack = Mathf.PerlinNoise(x * 0.5f + 100, y * 0.04f + 100);
            float fine = (Mathf.PerlinNoise(x * 1.5f + 200, y * 0.3f + 200) - 0.5f) * 0.04f;
            bool dark = crack < 0.30f;

            float r = 0.45f + grain * 0.10f - (dark ? 0.10f : 0f) + fine;
            float g = 0.30f + grain * 0.07f - (dark ? 0.08f : 0f) + fine;
            float b = 0.18f + grain * 0.05f - (dark ? 0.05f : 0f) + fine;
            pixels[y * size + x] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
