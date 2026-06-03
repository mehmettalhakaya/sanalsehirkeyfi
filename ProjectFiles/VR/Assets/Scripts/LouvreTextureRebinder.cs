using UnityEngine;

/// <summary>
/// Sahne her yuklendiginde Louvre material'lerine dokuyu yeniden bagliyor.
/// Sebep: FBX'in embedded material'leri var, sahne degisince Unity onlari
/// FBX'ten yeniden yukluyor ve editor'da yapilan SetTexture kayboluyor.
/// Bu script Start()'ta calisip texture'i tekrar set ediyor.
/// </summary>
public class LouvreTextureRebinder : MonoBehaviour
{
    [Header("Doku")]
    [Tooltip("Louvre atlas dokusu - LouvreSetup tarafindan otomatik atanir")]
    public Texture2D atlas;

    [Header("Atlanacak Material Isimleri")]
    public string[] skipKeywords = new[] { "Ground", "Border", "Stone", "Pavement" };

    void Awake()
    {
        Rebind();
    }

    void Start()
    {
        // Bazi Awake'ler sirasinda renderer hazir olmayabilir, tekrar dene
        Rebind();
    }

    public void Rebind()
    {
        // Fallback: atlas null ise sahnede yuklu tum texture'lar arasinda Louvre Atlas ara
        if (atlas == null)
        {
            var allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (var t in allTextures)
            {
                if (t == null) continue;
                if (t.name.Contains("Louvre_Atlas") || t.name.Contains("Atlas_2k") ||
                    t.name.Contains("Atlas_Original") || t.name.Contains("Building_Atlas"))
                {
                    atlas = t;
                    Debug.Log($"[TextureRebinder] Atlas fallback ile bulundu: {t.name}");
                    break;
                }
            }
        }

        if (atlas == null)
        {
            Debug.LogWarning("[TextureRebinder] Atlas bulunamadi (ne ref ne de fallback)!");
            return;
        }

        var renderers = GetComponentsInChildren<MeshRenderer>(true);
        var processedMats = new System.Collections.Generic.HashSet<Material>();
        int count = 0;

        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null || processedMats.Contains(mat)) continue;

                bool skip = false;
                foreach (var kw in skipKeywords)
                {
                    if (mat.name.Contains(kw)) { skip = true; break; }
                }
                processedMats.Add(mat);
                if (skip) continue;

                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", atlas);
                    count++;
                }
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", atlas);
                }
                if (mat.HasProperty("_BaseColorMap"))
                {
                    mat.SetTexture("_BaseColorMap", atlas);
                }
                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", Color.white);
                }
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", Color.white);
                }
            }
        }
        Debug.Log($"[TextureRebinder] {count} material'e doku yeniden baglandi.");
    }
}
