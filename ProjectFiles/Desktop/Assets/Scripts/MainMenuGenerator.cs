using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

[ExecuteAlways]  // Editor'da da calisir, oyun baslatmadan menu gorunur
public class MainMenuGenerator : MonoBehaviour
{
    public Sprite arkaplanFotografi;

    // Runtime referanslar
    private Canvas mainCanvas;
    private GameObject onayPaneliRoot;
    private GameObject vedaEkraniRoot;

    void OnEnable()
    {
        // UI yoksa otomatik olustur (hem editor'da hem runtime'da)
        if (GameObject.Find("MainMenuCanvas") == null)
        {
#if UNITY_EDITOR
            // Editor'da delayCall ile guvenli olustur (OnEnable icinde direkt scene mutation tehlikeli)
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this == null) return;
                    if (GameObject.Find("MainMenuCanvas") == null) MenuyuInsaEt();
                    ButonGorunurluguGuncelle();
                };
                return;
            }
#endif
            MenuyuInsaEt();
        }

        ButonGorunurluguGuncelle();
    }

    void Update()
    {
        // Butonlar SADECE Play modunda gorunsun
        ButonGorunurluguGuncelle();
    }

    void ButonGorunurluguGuncelle()
    {
        bool authPanelOpen = false;
        if (Application.isPlaying)
        {
            var loginManager = Object.FindFirstObjectByType<LoginManager>();
            authPanelOpen = loginManager != null && loginManager.IsAuthPanelOpen();
        }

        bool show = Application.isPlaying && !authPanelOpen;
        // GameObject.Find inactive objeleri BULMAZ.
        // Transform.Find (Canvas alti) inactive child'lari da bulur.
        var canvas = GameObject.Find("MainMenuCanvas");
        if (canvas == null) return;
        var t1 = canvas.transform.Find("KEŞFE BAŞLA");
        var t2 = canvas.transform.Find("AYRIL");
        if (t1 != null && t1.gameObject.activeSelf != show) t1.gameObject.SetActive(show);
        if (t2 != null && t2.gameObject.activeSelf != show) t2.gameObject.SetActive(show);
    }

    void Start()
    {
        // Runtime'da: mevcut UI varsa sadece referans yakala + butonlari bagla
        if (!Application.isPlaying) return;

        if (GameObject.Find("MainMenuCanvas") == null)
        {
            MenuyuInsaEt();
        }
        else
        {
            var existingCanvas = GameObject.Find("MainMenuCanvas");
            if (existingCanvas != null) mainCanvas = existingCanvas.GetComponent<Canvas>();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
        }

        // Play modda butonlar gorunur (Transform.Find inactive'leri de bulur)
        var canvasGO = GameObject.Find("MainMenuCanvas");
        if (canvasGO != null)
        {
            var tA = canvasGO.transform.Find("KEŞFE BAŞLA");
            if (tA != null)
            {
                tA.gameObject.SetActive(true);
                var btnA = tA.GetComponent<Button>();
                if (btnA != null)
                {
                    btnA.onClick.RemoveAllListeners();
                    // Auth akisini LoginManager yonetiyor; direkt sahne acma burada baglanmiyor.
                }
            }
            var tB = canvasGO.transform.Find("AYRIL");
            if (tB != null)
            {
                tB.gameObject.SetActive(true);
                var btnB = tB.GetComponent<Button>();
                if (btnB != null)
                {
                    btnB.onClick.RemoveAllListeners();
                    btnB.onClick.AddListener(AyrilTiklandi);
                }
            }
        }
    }

    // Sahne icinde saved button'larin onClick handler'larini yeniden bagla
    void ButonOnClickBagla(string buttonName, UnityEngine.Events.UnityAction action)
    {
        var go = GameObject.Find(buttonName);
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    // Public yapildi - editor bake icin
    public void MenuyuInsaEt()
    {
        // Cursor goster (HumanoidController'dan gelirken kilitliyse)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // 1. Event System (Unity 6 - FindFirstObjectByType)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null) es.AddComponent(inputModuleType);
            else es.AddComponent<StandaloneInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        // 2. Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Arka Plan
        GameObject bgObj = new GameObject("Arkaplan");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.sprite = arkaplanFotografi;
        bgImage.color = Color.white;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 4. Karartma Katmanı
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(bgObj.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0.05f, 0.05f, 0.05f, 0.65f);
        overlay.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        overlay.GetComponent<RectTransform>().anchorMax = Vector2.one;
        overlay.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // 5. Başlık
        GameObject titleObj = new GameObject("Baslik");
        titleObj.transform.SetParent(canvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "LOUVRE MÜZESİ KEŞFİ";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 115;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.9f);
        titleShadow.effectDistance = new Vector2(4, -4);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 250);
        titleRect.sizeDelta = new Vector2(1400, 400);

        // 6. Premium Butonlar
        ButonYarat(canvasObj.transform, "KEŞFE BAŞLA", new Vector2(0, -30), null);
        ButonYarat(canvasObj.transform, "AYRIL", new Vector2(0, -150), AyrilTiklandi);
    }

    void ButonYarat(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(text);
        btnObj.transform.SetParent(parent, false);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        Button btn = btnObj.AddComponent<Button>();
        if (action != null)
            btn.onClick.AddListener(action);

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        cb.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        btn.colors = cb;

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = new Vector2(400, 85);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text btnText = txtObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 30;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(0.95f, 0.85f, 0.65f);
        RectTransform txtRT = btnText.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        txtRT.anchoredPosition = Vector2.zero;
    }

    void OyunaBasla()
    {
        SceneManager.LoadScene("Outside_Museum");
    }

    // ============================================
    // AYRIL AKIŞI: Onay Penceresi → Veda Ekranı → Quit
    // ============================================

    void AyrilTiklandi()
    {
        OnayPenceresiAc();
    }

    void OnayPenceresiAc()
    {
        if (onayPaneliRoot != null) return;

        // Yari saydam karartma overlay (tum ekrani kapla)
        onayPaneliRoot = new GameObject("OnayPaneliRoot");
        onayPaneliRoot.transform.SetParent(mainCanvas.transform, false);
        var rootImg = onayPaneliRoot.AddComponent<Image>();
        rootImg.color = new Color(0, 0, 0, 0.7f);
        var rootRT = onayPaneliRoot.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;

        // Tiklama dis tarafa gelmesin diye Button (action yok, sadece raycast block)
        onayPaneliRoot.AddComponent<Button>();

        // Onay karti (orta dikdortgen)
        var card = new GameObject("OnayKart");
        card.transform.SetParent(onayPaneliRoot.transform, false);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.10f, 0.08f, 0.06f, 0.98f);  // koyu fume
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(700, 350);
        cardRT.anchoredPosition = Vector2.zero;

        // Altin serit (sol kenar)
        var border = new GameObject("AltinSerit");
        border.transform.SetParent(card.transform, false);
        var borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.78f, 0.62f, 0.35f, 1f);
        var borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0f, 0f);
        borderRT.anchorMax = new Vector2(0f, 1f);
        borderRT.pivot = new Vector2(0f, 0.5f);
        borderRT.sizeDelta = new Vector2(5f, 0f);
        borderRT.anchoredPosition = Vector2.zero;

        // Baslik
        var titleGO = new GameObject("Baslik");
        titleGO.transform.SetParent(card.transform, false);
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "AYRILMAK İSTEDİĞİNİZE EMİN MİSİNİZ?";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.95f, 0.85f, 0.65f);  // sampanya altini
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(-60, 80);
        titleRT.anchoredPosition = new Vector2(0, -40);

        // Alt yazi
        var subGO = new GameObject("AltYazi");
        subGO.transform.SetParent(card.transform, false);
        var subText = subGO.AddComponent<Text>();
        subText.text = "Müze gezintiniz sonlandırılacak.";
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.fontSize = 17;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.85f, 0.80f, 0.72f);
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 0.5f);
        subRT.anchorMax = new Vector2(1f, 0.5f);
        subRT.pivot = new Vector2(0.5f, 0.5f);
        subRT.sizeDelta = new Vector2(-60, 40);
        subRT.anchoredPosition = new Vector2(0, 20);

        // EVET, AYRIL butonu (kirmizi-burgundi)
        OnayButon(card.transform, "EVET, AYRIL", new Vector2(-110, -120),
            new Color(0.45f, 0.10f, 0.10f, 1f), VedaEkraniAc);
        // IPTAL butonu (notr fume)
        OnayButon(card.transform, "İPTAL", new Vector2(110, -120),
            new Color(0.15f, 0.13f, 0.11f, 1f), OnayPenceresiKapat);
    }

    void OnayButon(Transform parent, string label, Vector2 pos, Color bgColor, UnityEngine.Events.UnityAction action)
    {
        var btnGO = new GameObject(label);
        btnGO.transform.SetParent(parent, false);
        var img = btnGO.AddComponent<Image>();
        img.color = bgColor;
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(action);

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f, 1f);
        cb.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
        btn.colors = cb;

        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(200, 55);
        rt.anchoredPosition = pos;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 18;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.95f, 0.85f, 0.65f);
        var txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        txtRT.anchoredPosition = Vector2.zero;
    }

    void OnayPenceresiKapat()
    {
        if (onayPaneliRoot != null)
        {
            Destroy(onayPaneliRoot);
            onayPaneliRoot = null;
        }
    }

    // ============================================
    // VEDA EKRANI (Louvre arkaplan + tesekkur + fade)
    // ============================================

    void VedaEkraniAc()
    {
        if (onayPaneliRoot != null)
        {
            Destroy(onayPaneliRoot);
            onayPaneliRoot = null;
        }

        // Tam ekran kart - Louvre fotografi arka plan
        vedaEkraniRoot = new GameObject("VedaEkraniRoot");
        vedaEkraniRoot.transform.SetParent(mainCanvas.transform, false);
        var bgImg = vedaEkraniRoot.AddComponent<Image>();
        bgImg.sprite = arkaplanFotografi;
        bgImg.color = new Color(1, 1, 1, 0);  // fade in icin alpha 0 baslar
        var bgRT = vedaEkraniRoot.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        // Karartma overlay
        var darkGO = new GameObject("Karartma");
        darkGO.transform.SetParent(vedaEkraniRoot.transform, false);
        var darkImg = darkGO.AddComponent<Image>();
        darkImg.color = new Color(0.02f, 0.02f, 0.04f, 0);  // fade in icin 0
        var darkRT = darkGO.GetComponent<RectTransform>();
        darkRT.anchorMin = Vector2.zero;
        darkRT.anchorMax = Vector2.one;
        darkRT.sizeDelta = Vector2.zero;

        // Tesekkur yazisi
        var thanksGO = new GameObject("TesekkurYazisi");
        thanksGO.transform.SetParent(vedaEkraniRoot.transform, false);
        var thanksText = thanksGO.AddComponent<Text>();
        thanksText.text = "Louvre Müzesi'ni ziyaret ettiğiniz\niçin teşekkür ederiz.";
        thanksText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        thanksText.fontSize = 60;
        thanksText.fontStyle = FontStyle.Bold;
        thanksText.alignment = TextAnchor.MiddleCenter;
        thanksText.color = new Color(0.95f, 0.85f, 0.65f, 0);  // alpha 0 baslar
        thanksText.lineSpacing = 1.3f;

        var thanksShadow = thanksGO.AddComponent<Shadow>();
        thanksShadow.effectColor = new Color(0, 0, 0, 0.9f);
        thanksShadow.effectDistance = new Vector2(3, -3);

        var thanksRT = thanksGO.GetComponent<RectTransform>();
        thanksRT.anchorMin = new Vector2(0f, 0.4f);
        thanksRT.anchorMax = new Vector2(1f, 0.6f);
        thanksRT.sizeDelta = new Vector2(0, 0);
        thanksRT.anchoredPosition = new Vector2(0, 50);

        // Alt yazi - kucuk Au revoir
        var auGO = new GameObject("AuRevoir");
        auGO.transform.SetParent(vedaEkraniRoot.transform, false);
        var auText = auGO.AddComponent<Text>();
        auText.text = "Au revoir";
        auText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        auText.fontSize = 32;
        auText.fontStyle = FontStyle.Italic;
        auText.alignment = TextAnchor.MiddleCenter;
        auText.color = new Color(0.78f, 0.62f, 0.35f, 0);  // alpha 0 baslar
        var auRT = auGO.GetComponent<RectTransform>();
        auRT.anchorMin = new Vector2(0f, 0.3f);
        auRT.anchorMax = new Vector2(1f, 0.4f);
        auRT.sizeDelta = Vector2.zero;
        auRT.anchoredPosition = Vector2.zero;

        StartCoroutine(VedaAnimasyonu(bgImg, darkImg, thanksText, auText));
    }

    IEnumerator VedaAnimasyonu(Image bg, Image dark, Text thanks, Text au)
    {
        // 1) Fade in (1.2 saniye)
        float fadeInTime = 1.2f;
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeInTime);
            SetAlpha(bg, a);
            SetAlpha(dark, a * 0.55f);
            SetAlpha(thanks, a);
            SetAlpha(au, a * 0.9f);
            yield return null;
        }
        SetAlpha(bg, 1f);
        SetAlpha(dark, 0.55f);
        SetAlpha(thanks, 1f);
        SetAlpha(au, 0.9f);

        // 2) Bekle (2.5 saniye yazi gorunsun)
        yield return new WaitForSeconds(2.5f);

        // 3) Fade out to black (1.5 saniye - karartma artar, yazilar solar)
        float fadeOutTime = 1.5f;
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeOutTime);
            SetAlpha(thanks, 1f - p);
            SetAlpha(au, 0.9f - p * 0.9f);
            SetAlpha(dark, 0.55f + p * 0.45f);  // tam siyaha gider
            yield return null;
        }

        // 4) Cik
        Debug.Log("[MainMenu] Application.Quit cagriliyor.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetAlpha(Graphic g, float a)
    {
        var c = g.color;
        c.a = a;
        g.color = c;
    }
}
