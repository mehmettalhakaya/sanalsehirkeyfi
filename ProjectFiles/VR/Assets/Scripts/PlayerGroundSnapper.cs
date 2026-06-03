using UnityEngine;

/// <summary>
/// Ic sahnedeki ReturnPortal'in yaptigi raycast yontemini ana sahnede de uygular.
/// Sahne basladiginda Player'i hizla zemine snap eder.
/// LouvreGround plane'i varsa onun Y'sini direkt kullanir (en guvenilir).
/// Yoksa raycast ile zemini bulur.
/// </summary>
public class PlayerGroundSnapper : MonoBehaviour
{
    [Header("Hedef X/Z (ana sahne icin)")]
    public bool useFixedXZ = true;
    public float targetX = 0.2314512f;
    public float targetZ = 19.23787f;

    [Header("Raycast (yedek)")]
    public float rayStartHeight = 1000f;
    public float rayLength = 5000f;

    private bool didSnap = false;
    private int snapAttempts = 0;

    void Start()
    {
        TrySnap();
    }

    void Update()
    {
        // Ilk Start'ta collider'lar henuz hazir olmayabilir; birkac frame dene
        if (!didSnap && snapAttempts < 10)
        {
            TrySnap();
        }
    }

    void TrySnap()
    {
        snapAttempts++;

        var cc = GetComponent<CharacterController>();
        float x = useFixedXZ ? targetX : transform.position.x;
        float z = useFixedXZ ? targetZ : transform.position.z;

        // 1) LouvreGround plane varsa direkt onu kullan (en guvenilir)
        var groundGO = GameObject.Find("Pavement_Main");
        if (groundGO == null) groundGO = GameObject.Find("LouvreGround");

        float groundY = float.NaN;
        string source = "?";

        if (groundGO != null)
        {
            // Plane'in y pozisyonu = zemin yuksekligi
            // (LouvreGround root'sa, alt obje "Pavement_Main"i bul)
            Transform pavement = groundGO.transform.Find("Pavement_Main");
            if (pavement != null)
            {
                groundY = pavement.position.y;
                source = "Pavement_Main";
            }
            else if (groundGO.name == "Pavement_Main")
            {
                groundY = groundGO.transform.position.y;
                source = "Pavement_Main";
            }
            else
            {
                groundY = groundGO.transform.position.y;
                source = groundGO.name;
            }
        }

        // 2) LouvreGround yoksa raycast (interior tarzi - cameradan +2m asagi)
        if (float.IsNaN(groundY))
        {
            Camera mainCam = Camera.main;
            Vector3 rayOrigin;
            if (mainCam != null)
            {
                rayOrigin = new Vector3(x, mainCam.transform.position.y + 2f, z);
            }
            else
            {
                rayOrigin = new Vector3(x, rayStartHeight, z);
            }

            // RaycastAll - en alt kabul edilebilir hit'i bul
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayLength);
            float lowest = float.MaxValue;
            foreach (var h in hits)
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (h.collider.gameObject == gameObject) continue;
                if (h.point.y < lowest)
                {
                    lowest = h.point.y;
                    source = h.collider.gameObject.name;
                }
            }
            if (lowest < float.MaxValue)
            {
                groundY = lowest;
            }
        }

        if (float.IsNaN(groundY))
        {
            // Henuz collider'lar yok, bir sonraki frame'de tekrar dene
            return;
        }

        // CharacterController'i kapatip snap, sonra ac
        if (cc != null) cc.enabled = false;
        transform.position = new Vector3(x, groundY + 0.01f, z);
        if (cc != null) cc.enabled = true;

        Debug.Log($"[GroundSnapper] Player snap edildi: Y={groundY + 0.01f:F3} (source: {source})");
        didSnap = true;
        Invoke(nameof(DisableSelf), 1f);
    }

    void DisableSelf()
    {
        enabled = false;
    }
}
