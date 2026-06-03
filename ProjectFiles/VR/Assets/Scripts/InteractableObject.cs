using System;
using UnityEngine;

/// <summary>
/// Bir obje uzerine eklendiginde, oyuncu ona tikladiginda
/// (raycast ile) bilgi panelinin acilmasini saglar.
///
/// Kullanim:
///   1) Sahneye yerlestirilmis Louvre objesinin alt parcalarindan
///      etkilesimli olmasini istedigin (orn. ana piramit, bati pavyon)
///      bir GameObject sec.
///   2) Inspector'da Add Component -> InteractableObject ekle.
///   3) Asagidaki alanlari doldur:
///      - title: panelde gosterilecek baslik
///      - description: panel metni
///      - imageOptional: panele gorsel istersen surukle
///   4) Objenin Collider'i olmali (Mesh Collider veya Box Collider).
///      FBX import'tan gelmiyorsa: Add Component -> Mesh Collider.
///
/// Etkilesim mantigi PlayerInteraction script'i tarafindan yonetilir.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Panel Icerigi")]
    [Tooltip("Panelde gorunen baslik (orn. 'Cam Piramit')")]
    public string title = "Bilinmeyen Yapi";

    [TextArea(3, 10)]
    [Tooltip("Panelde gorunen aciklama metni")]
    public string description = "Aciklama yazilmadi.";

    [Tooltip("Opsiyonel: Panel ust kisminda gosterilecek gorsel (Sprite)")]
    public Sprite imageOptional;

    [Header("Vurgu (opsiyonel)")]
    [Tooltip("Mouse uzerine geldiginde objenin renk degistirmesini saglar")]
    public bool highlightOnHover = true;
    public Color highlightColor = new Color(1f, 0.85f, 0.4f, 1f);

    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isHighlighted = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
            else
                originalColors[i] = Color.white;
        }
    }

    /// <summary>PlayerInteraction tarafindan cagrilir.</summary>
    public void SetHover(bool on)
    {
        if (!highlightOnHover || isHighlighted == on) return;
        isHighlighted = on;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].material.HasProperty("_Color")) continue;
            renderers[i].material.color = on ? highlightColor : originalColors[i];
        }
    }

    internal void SetDescription(string text)
    {
        throw new NotImplementedException();
    }
}
