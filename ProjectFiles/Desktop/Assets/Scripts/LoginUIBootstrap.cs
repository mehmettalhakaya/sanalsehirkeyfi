using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoginUIBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu") return;

        var runner = new GameObject("_LoginUIBootstrap");
        runner.AddComponent<LoginUIBootstrapRunner>();
    }
}

public class LoginUIBootstrapRunner : MonoBehaviour
{
    static readonly Color CardDark = new Color(0.16f, 0.11f, 0.07f, 0.98f);
    static readonly Color Gold = new Color(0.95f, 0.85f, 0.65f);
    static readonly Color Border = new Color(0.78f, 0.62f, 0.35f, 1f);
    static readonly Color InputBg = new Color(0.30f, 0.22f, 0.14f, 1f);
    static readonly Color ButtonNormal = new Color(0.30f, 0.20f, 0.12f, 1f);

    System.Collections.IEnumerator Start()
    {
        if (Object.FindObjectsByType<LoginManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0)
        {
            Destroy(gameObject);
            yield break;
        }

        float timeout = 3f;
        Button startBtn = null;
        Button quitBtn = null;

        while (timeout > 0f && (startBtn == null || quitBtn == null))
        {
            yield return null;
            timeout -= Time.deltaTime;
            startBtn = FindButtonByName("KEŞFE BAŞLA");
            quitBtn = FindButtonByName("AYRIL");
        }

        if (startBtn == null)
        {
            Debug.LogWarning("[LoginUI] KEŞFE BAŞLA butonu bulunamadi.");
            Destroy(gameObject);
            yield break;
        }

        startBtn.onClick.RemoveAllListeners();

        Transform canvasT = startBtn.transform.parent;
        while (canvasT != null && canvasT.GetComponent<Canvas>() == null)
            canvasT = canvasT.parent;

        if (canvasT == null)
        {
            Debug.LogWarning("[LoginUI] Canvas bulunamadi.");
            Destroy(gameObject);
            yield break;
        }

        var lmGO = new GameObject("_LoginManager");
        lmGO.transform.SetParent(canvasT, false);

        var lm = lmGO.AddComponent<LoginManager>();
        lm.startButton = startBtn;
        lm.quitButton = quitBtn;
        var loadingPanel = canvasT.Find("LoadingPanel");
        if (loadingPanel != null)
            lm.loadingPanel = loadingPanel.gameObject;
        lm.overrideQuitButtonClick = false;

        BuildAuthChoicePanel(canvasT, lm);
        BuildLoginPanel(canvasT, lm);
        BuildRegisterPanel(canvasT, lm);

        Debug.Log("[LoginUI] Auth panelleri ve LoginManager kuruldu.");
        Destroy(gameObject);
    }

    static Button FindButtonByName(string name)
    {
        var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var button in buttons)
            if (button.gameObject.name == name)
                return button;

        return null;
    }

    static GameObject CreatePanelRoot(Transform parent, string name, Vector2 size, float cardYOffset = 0f)
    {
        var root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;

        var overlay = new GameObject("Overlay");
        overlay.transform.SetParent(root.transform, false);
        var overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.58f);
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;
        overlay.AddComponent<Button>();

        var card = new GameObject("Card");
        card.transform.SetParent(root.transform, false);
        var cardImage = card.AddComponent<Image>();
        cardImage.color = CardDark;
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = size;
        cardRect.anchoredPosition = new Vector2(0f, cardYOffset);

        var border = new GameObject("Border");
        border.transform.SetParent(card.transform, false);
        var borderImage = border.AddComponent<Image>();
        borderImage.color = Border;
        var borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0f, 0f);
        borderRect.anchorMax = new Vector2(1f, 0f);
        borderRect.pivot = new Vector2(0.5f, 0f);
        borderRect.sizeDelta = new Vector2(0f, 4f);
        borderRect.anchoredPosition = Vector2.zero;

        root.SetActive(false);
        return root;
    }

    static TMP_Text CreateLabel(Transform parent, string name, string text, Vector2 anchoredPos,
        Vector2 size, float fontSize, FontStyles style, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = align;
        label.color = color;
        label.enableWordWrapping = true;
        label.raycastTarget = false;

        var rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        return label;
    }

    static TMP_InputField CreateInput(Transform parent, string name, string placeholder,
        Vector2 anchoredPos, Vector2 size, bool isPassword)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var image = go.AddComponent<Image>();
        image.color = InputBg;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        var input = go.AddComponent<TMP_InputField>();
        input.targetGraphic = image;

        var area = new GameObject("Text Area");
        area.transform.SetParent(go.transform, false);
        area.AddComponent<RectMask2D>();
        var areaRect = area.GetComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(14f, 0f);
        areaRect.offsetMax = new Vector2(-14f, 0f);

        var text = CreateLabel(area.transform, "Text", "", Vector2.zero, Vector2.zero,
            22f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, Gold);
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        text.enableWordWrapping = false;

        var ph = CreateLabel(area.transform, "Placeholder", placeholder, Vector2.zero, Vector2.zero,
            22f, FontStyles.Italic, TextAlignmentOptions.MidlineLeft, new Color(0.7f, 0.55f, 0.35f, 0.6f));
        var phRect = ph.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        phRect.anchoredPosition = Vector2.zero;
        ph.enableWordWrapping = false;

        input.textViewport = areaRect;
        input.textComponent = text;
        input.placeholder = ph;
        input.contentType = isPassword ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;

        return input;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var image = go.AddComponent<Image>();
        image.color = ButtonNormal;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = ButtonNormal;
        colors.highlightedColor = new Color(0.42f, 0.30f, 0.18f, 1f);
        colors.pressedColor = new Color(0.22f, 0.16f, 0.10f, 1f);
        button.colors = colors;

        var text = CreateLabel(go.transform, "Label", label, Vector2.zero,
            new Vector2(size.x - 20f, size.y - 10f), 22f, FontStyles.Bold,
            TextAlignmentOptions.Center, Gold);
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        return button;
    }

    static void SetRect(Transform parent, string childName, Vector2 anchoredPos, Vector2 size)
    {
        var child = parent.Find(childName);
        if (child == null) return;

        var rect = child.GetComponent<RectTransform>();
        if (rect == null) return;

        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
    }

    static void SetLabel(Transform parent, string childName, float fontSize, TextAlignmentOptions alignment)
    {
        var child = parent.Find(childName);
        if (child == null) return;

        var label = child.GetComponent<TMP_Text>();
        if (label == null) return;

        label.fontSize = fontSize;
        label.alignment = alignment;
        label.enableWordWrapping = false;
    }

    static void PolishLoginLayout(Transform card)
    {
        SetRect(card, "Title", new Vector2(0f, 155f), new Vector2(560f, 50f));
        SetRect(card, "ULabel", new Vector2(0f, 112f), new Vector2(500f, 26f));
        SetRect(card, "UsernameInput", new Vector2(0f, 70f), new Vector2(500f, 48f));
        SetRect(card, "PLabel", new Vector2(0f, 22f), new Vector2(500f, 26f));
        SetRect(card, "PasswordInput", new Vector2(0f, -20f), new Vector2(500f, 48f));
        SetRect(card, "Message", new Vector2(0f, -82f), new Vector2(540f, 30f));
        SetRect(card, "Submit", new Vector2(-130f, -140f), new Vector2(220f, 55f));
        SetRect(card, "Back", new Vector2(130f, -140f), new Vector2(220f, 55f));

        SetLabel(card, "ULabel", 17f, TextAlignmentOptions.Left);
        SetLabel(card, "PLabel", 17f, TextAlignmentOptions.Left);
    }

    static void PolishRegisterLayout(Transform card)
    {
        SetRect(card, "Title", new Vector2(0f, 230f), new Vector2(560f, 50f));
        SetRect(card, "ULabel", new Vector2(0f, 174f), new Vector2(500f, 26f));
        SetRect(card, "UsernameInput", new Vector2(0f, 135f), new Vector2(500f, 48f));
        SetRect(card, "EmailLabel", new Vector2(0f, 84f), new Vector2(500f, 26f));
        SetRect(card, "EmailInput", new Vector2(0f, 45f), new Vector2(500f, 48f));
        SetRect(card, "P1Label", new Vector2(0f, -6f), new Vector2(500f, 26f));
        SetRect(card, "PasswordInput", new Vector2(0f, -45f), new Vector2(500f, 48f));
        SetRect(card, "P2Label", new Vector2(0f, -96f), new Vector2(500f, 26f));
        SetRect(card, "PasswordAgainInput", new Vector2(0f, -135f), new Vector2(500f, 48f));
        SetRect(card, "Message", new Vector2(0f, -182f), new Vector2(540f, 30f));
        SetRect(card, "Submit", new Vector2(-130f, -235f), new Vector2(220f, 55f));
        SetRect(card, "Back", new Vector2(130f, -235f), new Vector2(220f, 55f));

        SetLabel(card, "ULabel", 17f, TextAlignmentOptions.Left);
        SetLabel(card, "EmailLabel", 17f, TextAlignmentOptions.Left);
        SetLabel(card, "P1Label", 17f, TextAlignmentOptions.Left);
        SetLabel(card, "P2Label", 17f, TextAlignmentOptions.Left);
    }

    static void BuildAuthChoicePanel(Transform canvas, LoginManager lm)
    {
        var panel = CreatePanelRoot(canvas, "AuthChoicePanel", new Vector2(600f, 320f), -130f);
        var card = panel.transform.Find("Card");

        CreateLabel(card, "Title", "GİRİŞ GEREKLİ", new Vector2(0f, 110f),
            new Vector2(560f, 50f), 32f, FontStyles.Bold, TextAlignmentOptions.Center, Gold);

        var warning = CreateLabel(card, "Warning",
            "Keşfe başlamak için önce giriş yapmanız gerekiyor.",
            new Vector2(0f, 40f), new Vector2(540f, 60f), 22f,
            FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.92f, 0.82f, 0.62f));

        var login = CreateButton(card, "GirisYap", "GİRİŞ YAP", new Vector2(-130f, -50f), new Vector2(220f, 60f));
        var register = CreateButton(card, "Kaydol", "KAYDOL", new Vector2(130f, -50f), new Vector2(220f, 60f));
        var close = CreateButton(card, "Kapat", "GERİ", new Vector2(0f, -120f), new Vector2(180f, 45f));

        lm.authChoicePanel = panel;
        lm.authWarningText = warning;
        lm.openLoginButton = login;
        lm.openRegisterButton = register;
        lm.authChoiceCloseButton = close;
    }

    static void BuildLoginPanel(Transform canvas, LoginManager lm)
    {
        var panel = CreatePanelRoot(canvas, "LoginPanel", new Vector2(600f, 410f), -165f);
        var card = panel.transform.Find("Card");

        CreateLabel(card, "Title", "GİRİŞ YAP", new Vector2(0f, 180f),
            new Vector2(560f, 50f), 32f, FontStyles.Bold, TextAlignmentOptions.Center, Gold);
        CreateLabel(card, "ULabel", "Kullanıcı Adı veya E-posta", new Vector2(-100f, 110f),
            new Vector2(320f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Left, Gold);
        var username = CreateInput(card, "UsernameInput", "kullanıcı adı veya e-posta...", new Vector2(0f, 75f), new Vector2(500f, 50f), false);

        CreateLabel(card, "PLabel", "Şifre", new Vector2(-200f, 30f),
            new Vector2(160f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Left, Gold);
        var password = CreateInput(card, "PasswordInput", "şifre...", new Vector2(0f, -5f), new Vector2(500f, 50f), true);

        var message = CreateLabel(card, "Message", "", new Vector2(0f, -55f),
            new Vector2(540f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Center, Gold);
        var submit = CreateButton(card, "Submit", "GİRİŞ YAP", new Vector2(-130f, -120f), new Vector2(220f, 55f));
        var back = CreateButton(card, "Back", "GERİ DÖN", new Vector2(130f, -120f), new Vector2(220f, 55f));

        PolishLoginLayout(card);

        lm.loginPanel = panel;
        lm.loginUsernameInput = username;
        lm.loginPasswordInput = password;
        lm.loginMessageText = message;
        lm.loginSubmitButton = submit;
        lm.loginBackButton = back;
    }

    static void BuildRegisterPanel(Transform canvas, LoginManager lm)
    {
        var panel = CreatePanelRoot(canvas, "RegisterPanel", new Vector2(600f, 560f), -205f);
        var card = panel.transform.Find("Card");

        CreateLabel(card, "Title", "KAYDOL", new Vector2(0f, 260f),
            new Vector2(560f, 50f), 32f, FontStyles.Bold, TextAlignmentOptions.Center, Gold);
        CreateLabel(card, "ULabel", "Kullanıcı Adı", new Vector2(-200f, 190f),
            new Vector2(160f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Left, Gold);
        var username = CreateInput(card, "UsernameInput", "kullanıcı adı...", new Vector2(0f, 155f), new Vector2(500f, 50f), false);

        CreateLabel(card, "EmailLabel", "E-posta", new Vector2(-200f, 105f),
            new Vector2(160f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Left, Gold);
        var email = CreateInput(card, "EmailInput", "ornek@mail.com", new Vector2(0f, 70f), new Vector2(500f, 50f), false);

        CreateLabel(card, "P1Label", "Şifre", new Vector2(-200f, 20f),
            new Vector2(160f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Left, Gold);
        var password = CreateInput(card, "PasswordInput", "en az 8 karakter...", new Vector2(0f, -15f), new Vector2(500f, 50f), true);

        CreateLabel(card, "P2Label", "Şifre Tekrar", new Vector2(-200f, -65f),
            new Vector2(160f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Left, Gold);
        var passwordAgain = CreateInput(card, "PasswordAgainInput", "şifre tekrar...", new Vector2(0f, -100f), new Vector2(500f, 50f), true);

        var message = CreateLabel(card, "Message", "", new Vector2(0f, -150f),
            new Vector2(540f, 30f), 18f, FontStyles.Normal, TextAlignmentOptions.Center, Gold);
        var submit = CreateButton(card, "Submit", "KAYDOL", new Vector2(-130f, -215f), new Vector2(220f, 55f));
        var back = CreateButton(card, "Back", "GERİ DÖN", new Vector2(130f, -215f), new Vector2(220f, 55f));

        PolishRegisterLayout(card);

        lm.registerPanel = panel;
        lm.registerUsernameInput = username;
        lm.registerEmailInput = email;
        lm.registerPasswordInput = password;
        lm.registerPasswordAgainInput = passwordAgain;
        lm.registerMessageText = message;
        lm.registerSubmitButton = submit;
        lm.registerBackButton = back;
    }
}
