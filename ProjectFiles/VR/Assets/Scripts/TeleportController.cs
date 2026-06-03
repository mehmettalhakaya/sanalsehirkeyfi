using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Sag tik basili tutarken zemin uzerinde marker gosterir,
/// birakildiginda oyuncu o noktaya isinlanir.
///
/// PlayerController ayni objeye eklenmis olmali.
/// </summary>
public class TeleportController : MonoBehaviour
{
    [Header("Teleport")]
    [Tooltip("Maksimum teleport mesafesi (metre)")]
    public float maxDistance = 200f;

    [Tooltip("Teleport noktasinda gosterilecek marker (otomatik olusur eger bos)")]
    public GameObject markerPrefab;

    [Tooltip("Teleport sonrasi karaktere eklenecek dikey ofset (m)")]
    public float verticalOffset = 1.6f;

    [Header("Layers")]
    [Tooltip("Teleport edilebilir yuzeyler (varsayilan: tum)")]
    public LayerMask teleportLayers = ~0;

    private GameObject marker;
    private bool isAiming = false;
    private Vector3 lastValidPoint;
    private bool hasValidPoint = false;
    private PlayerController player;
    private Camera cam;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // Settings menu / info panel acikken devre disi
        if (player == null) return;

        bool aimDown = ReadAimDown();
        bool aimUp = ReadAimUp();
        bool aimHeld = ReadAimHeld();

        if (aimDown)
        {
            isAiming = true;
            CreateMarkerIfNeeded();
        }

        if (isAiming && aimHeld)
        {
            UpdateAim();
        }

        if (isAiming && aimUp)
        {
            isAiming = false;
            if (hasValidPoint)
            {
                Vector3 target = lastValidPoint + Vector3.up * verticalOffset;
                player.TeleportTo(target);
            }
            HideMarker();
            hasValidPoint = false;
        }
    }

    private void UpdateAim()
    {
        if (cam == null) return;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, teleportLayers))
        {
            // Ust bakan yuzey gerek (zemin gibi)
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
            {
                lastValidPoint = hit.point;
                hasValidPoint = true;
                if (marker != null)
                {
                    marker.SetActive(true);
                    marker.transform.position = hit.point + Vector3.up * 0.05f;
                }
                return;
            }
        }
        hasValidPoint = false;
        if (marker != null) marker.SetActive(false);
    }

    private void CreateMarkerIfNeeded()
    {
        if (marker != null) return;
        if (markerPrefab != null)
        {
            marker = Instantiate(markerPrefab);
        }
        else
        {
            // Varsayilan marker: yatay daire (silindir)
            marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "TeleportMarker";
            // Collider'i kapat ki raycast etkilenmesin
            var col = marker.GetComponent<Collider>();
            if (col != null) Destroy(col);
            marker.transform.localScale = new Vector3(2.5f, 0.05f, 2.5f);
            // Materyal
            var rend = marker.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.95f, 1f, 0.6f);
                mat.SetFloat("_Mode", 3);  // transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                rend.material = mat;
            }
        }
        marker.SetActive(false);
    }

    private void HideMarker()
    {
        if (marker != null) marker.SetActive(false);
    }

    // Input ---------------------------------------------------------------

    private bool ReadAimDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.rightButton.wasPressedThisFrame;
#endif
        return Input.GetMouseButtonDown(1);
    }

    private bool ReadAimUp()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.rightButton.wasReleasedThisFrame;
#endif
        return Input.GetMouseButtonUp(1);
    }

    private bool ReadAimHeld()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.rightButton.isPressed;
#endif
        return Input.GetMouseButton(1);
    }
}
