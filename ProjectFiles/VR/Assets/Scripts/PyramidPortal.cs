using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Piramide 1m yaklasinca sol ust kosede prompt UI gosterir
/// ("Muzeye Gir (ESC)"). ESC tusuna basildiginda ic sahneye gecer.
/// Otomatik geciş YOK - kullanici onay olarak ESC basmali.
/// </summary>
public class PyramidPortal : MonoBehaviour
{
    [Header("Sahne Gecisi")]
    public string targetSceneName = "LouvreInteriorOptimized";

    [Header("Tetikleme")]
    [Tooltip("Bu mesafede prompt UI acilir (metre)")]
    public float promptDistance = 0.05f;

    [Header("Piramit Bounds (mesafe hesabi)")]
    [Tooltip("Mesafe kutu trigger yerine bu bounds'a gore hesaplanir")]
    public bool usePyramidBounds = false;
    public Vector3 pyramidCenter = Vector3.zero;
    public Vector3 pyramidSize = Vector3.one;
    [Tooltip("Mesafe sadece XZ ile (Y dahil degil)")]
    public bool useXZOnly = true;

    [Header("Geri uyumluluk")]
    public string promptTitle = "";
    [TextArea(2, 4)]
    public string promptMessage = "";
    public string yesButtonText = "";
    public string noButtonText = "";
    public float triggerDistance = 0f;

    [Header("Fade")]
    public string fadeMessage = "Louvre'a giriliyor...";

    // Runtime
    private bool transitioning = false;
    private float transitionStart = -1f;
    private const float FADE_DURATION = 0.5f;

    private GameObject promptUI;
    private GameObject playerCache;

    void Start()
    {
        CreatePromptUI();
    }

    void Update()
    {
        if (transitioning) return;

        // Player'i bul
        if (playerCache == null) playerCache = GameObject.Find("Player");
        if (playerCache == null) return;

        // Mesafeyi piramidin GERCEK bounds'una gore hesapla (XZ only varsayilan)
        Vector3 playerPos = playerCache.transform.position;
        float dist;
        if (usePyramidBounds)
        {
            if (useXZOnly)
            {
                // Sadece XZ duzleminde piramidin tabanina mesafe
                float halfX = pyramidSize.x * 0.5f;
                float halfZ = pyramidSize.z * 0.5f;
                float dx = playerPos.x - pyramidCenter.x;
                float dz = playerPos.z - pyramidCenter.z;
                float clX = Mathf.Clamp(dx, -halfX, halfX);
                float clZ = Mathf.Clamp(dz, -halfZ, halfZ);
                Vector2 closestXZ = new Vector2(pyramidCenter.x + clX, pyramidCenter.z + clZ);
                Vector2 playerXZ = new Vector2(playerPos.x, playerPos.z);
                dist = Vector2.Distance(playerXZ, closestXZ);
            }
            else
            {
                Vector3 halfSize = pyramidSize * 0.5f;
                Vector3 d = playerPos - pyramidCenter;
                Vector3 clamped = new Vector3(
                    Mathf.Clamp(d.x, -halfSize.x, halfSize.x),
                    Mathf.Clamp(d.y, -halfSize.y, halfSize.y),
                    Mathf.Clamp(d.z, -halfSize.z, halfSize.z)
                );
                Vector3 closest = pyramidCenter + clamped;
                dist = Vector3.Distance(playerPos, closest);
            }
        }
        else
        {
            var col = GetComponent<Collider>();
            if (col == null) return;
            Vector3 closest = col.bounds.ClosestPoint(playerPos);
            dist = Vector3.Distance(playerPos, closest);
        }

        bool inRange = dist <= promptDistance;
        if (promptUI != null && promptUI.activeSelf != inRange)
        {
            promptUI.SetActive(inRange);
        }

        if (inRange && ReadEscapePressed())
        {
            BeginTransition();
        }
    }

    void CreatePromptUI()
    {
        // Canvas bul ya da olustur
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("PyramidPromptCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Prompt button (Disari Cik ile ayni stilde, kucuk)
        promptUI = new GameObject("PyramidEnterPrompt");
        promptUI.transform.SetParent(canvas.transform, false);
        var rt = promptUI.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(320f, 60f);
        rt.anchoredPosition = new Vector2(20f, -20f);

        var img = promptUI.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.7f, 0.92f);     // mavi - giris

        // Tiklamayla da gecsin
        var btn = promptUI.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => { if (!transitioning) BeginTransition(); });

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(promptUI.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var txt = txtGO.AddComponent<Text>();
        txt.text = "Muzeye Gir (E)";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 25;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;

        promptUI.SetActive(false);
    }

    void BeginTransition()
    {
        if (transitioning) return;
        transitioning = true;
        transitionStart = Time.time;
        if (promptUI != null) promptUI.SetActive(false);
        Debug.Log("[PyramidPortal] Ic sahneye geciliyor.");
    }

    void OnGUI()
    {
        if (!transitioning) return;
        float t = (Time.time - transitionStart) / FADE_DURATION;
        t = Mathf.Clamp01(t);

        GUI.color = new Color(0, 0, 0, t);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (t > 0.4f)
        {
            GUIStyle s = new GUIStyle();
            s.fontSize = 24;
            s.fontStyle = FontStyle.Bold;
            s.alignment = TextAnchor.MiddleCenter;
            s.normal.textColor = new Color(1f, 0.92f, 0.6f, (t - 0.4f) / 0.6f);
            GUI.Label(new Rect(0, Screen.height / 2 - 20, Screen.width, 40), fadeMessage, s);
        }

        if (t >= 1f) EnterInterior();
    }

    public void EnterInterior()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[PyramidPortal] targetSceneName bos!");
            return;
        }
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        Debug.Log($"[PyramidPortal] Sahne degisiyor: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    private bool ReadEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.eKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }
}
