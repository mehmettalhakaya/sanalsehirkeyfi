using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Ic sahnede:
///   - Mevcut Main Camera'yi yeni Player altina tasir
///   - HumanoidController ekler (WASD + ziplama + egilme + carpisma)
///   - PlayerInteraction ekler (tablolara tiklayinca bilgi paneli acar)
///   - Sol ust "Disari Cik" butonu
///   - Esc tusu ile cikis
/// </summary>
public class ReturnPortal : MonoBehaviour
{
    [Header("Gecis")]
    public string mainSceneName = "SampleScene";
    public bool readFromPrefs = true;

    [Header("Otomatik Kurulum")]
    public bool autoCreateButton = true;
    public bool autoSetupPlayer = true;

    void Start()
    {
        Time.timeScale = 1f;
        if (autoSetupPlayer) SetupPlayer();
        if (autoCreateButton) CreateReturnButton();
    }

    void Update()
    {
        if (ReadEscapePressed()) ReturnToMain();
    }

    public void ReturnToMain()
    {
        string target = mainSceneName;
        if (readFromPrefs)
        {
            string prev = PlayerPrefs.GetString("PreviousScene", "");
            if (!string.IsNullOrEmpty(prev)) target = prev;
        }
        Debug.Log($"[ReturnPortal] Geri don: {target}");
        SceneManager.LoadScene(target);
    }

    private void SetupPlayer()
    {
        if (GameObject.Find("Player") != null) return;

        int colliderCount = 0;
        var allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (var mr in allRenderers)
        {
            if (mr.GetComponent<Collider>() != null) continue;
            var mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            try
            {
                var mc = mr.gameObject.AddComponent<MeshCollider>();
                mc.convex = false;
                colliderCount++;
            }
            catch { }
        }
        Debug.Log($"[ReturnPortal] {colliderCount} ic mesh'e MeshCollider eklendi");
        Physics.SyncTransforms();

        Camera existingCam = Camera.main;
        Vector3 camPos = existingCam != null ? existingCam.transform.position : new Vector3(0, 5f, 0);
        Quaternion startRot = existingCam != null ? Quaternion.Euler(0, existingCam.transform.eulerAngles.y, 0) : Quaternion.identity;

        float groundY = camPos.y - 1.5f;
        RaycastHit groundHit;
        Vector3 rayStart = new Vector3(camPos.x, camPos.y + 2f, camPos.z);
        if (Physics.Raycast(rayStart, Vector3.down, out groundHit, 1000f))
        {
            groundY = groundHit.point.y;
            Debug.Log($"[ReturnPortal] Zemin bulundu: Y={groundY:F2} ({groundHit.collider.gameObject.name})");
        }

        Vector3 startPos = new Vector3(camPos.x, groundY + 0.05f, camPos.z);

        var playerGO = new GameObject("Player");
        playerGO.transform.position = startPos;
        playerGO.transform.rotation = startRot;

        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.85f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.925f, 0);
        cc.skinWidth = 0.05f;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        playerGO.transform.localScale = new Vector3(1f, 1f, 1f);

        if (existingCam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            existingCam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }

        existingCam.transform.SetParent(playerGO.transform, false);
        existingCam.transform.localPosition = new Vector3(0, 1.25f, 0);
        existingCam.transform.localRotation = Quaternion.identity;

        var fly = existingCam.GetComponent<SimpleFlyCamera>();
        if (fly != null) Destroy(fly);

        var hc = playerGO.AddComponent<HumanoidController>();
        hc.cameraTransform = existingCam.transform;
        hc.jumpHeight = 0.5f;
        hc.standHeight = 1.85f;
        hc.crouchHeight = 0.9f;
        hc.standCameraY = 1.7f;
        hc.crouchCameraY = 0.75f;
        hc.useWorldSpaceCamera = false;

        // PlayerInteraction ekle - tablolara tiklayinca bilgi paneli acar
        var pi = playerGO.AddComponent<PlayerInteraction>();
        pi.maxDistance = 200f;
        pi.viewCamera = existingCam;

        // InfoPanelController'i sahnede bul ve bagla
        var infoPanel = Object.FindFirstObjectByType<InfoPanelController>(FindObjectsInactive.Include);
        if (infoPanel != null)
        {
            pi.infoPanel = infoPanel;
            Debug.Log("[ReturnPortal] InfoPanelController bulundu ve PlayerInteraction'a baglandi.");
        }
        else
        {
            Debug.LogWarning("[ReturnPortal] InfoPanelController bulunamadi! Sahnede Canvas > Panel > InfoPanelController olmali.");
        }

        Debug.Log($"[ReturnPortal] Insansi Player kuruldu (ic sahne) - Pos={startPos}");
    }

    private void CreateReturnButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("ReturnUICanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null) esGO.AddComponent(inputModuleType);
            else esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#else
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        if (GameObject.Find("ReturnButton") != null) return;

        var btnGO = new GameObject("ReturnButton");
        btnGO.transform.SetParent(canvas.transform, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(300f, 60f);
        rt.anchoredPosition = new Vector2(20f, -20f);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.7f, 0.2f, 0.2f, 0.9f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(ReturnToMain);

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = "← Disari Cik (E)";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 25;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;
    }

    private bool ReadEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.eKey.wasPressedThisFrame;
#endif
        return kb.eKey.wasPressedThisFrame;
    }
}