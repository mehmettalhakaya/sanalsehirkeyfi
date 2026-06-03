using UnityEngine;
using UnityEngine.UI;

public class InfoPanelController : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Image image;
    public GameObject imageContainer;
    public Button closeButton;

    public bool IsOpen { get; private set; }

    void Start()
    {
        BuildPanel();
        gameObject.SetActive(false);
        IsOpen = false;
    }

    void BuildPanel()
    {
        // Onceden kurulduysa tekrar kurma
        if (transform.Find("Header") != null)
        {
            titleText = FindInChildren<Text>("TitleText");
            descriptionText = FindInChildren<Text>("DescriptionText");
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Panel arka plan
        var panelImg = GetComponent<Image>();
        if (panelImg == null) panelImg = gameObject.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.08f, 0.05f, 0.96f);

        // Panel pozisyon - sag taraf, tam boy (BUYUTULDU: 400 -> 600)
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(600f, 0f);
        rt.anchoredPosition = Vector2.zero;

        // Sol altin serit (daha kalin: 4 -> 6)
        CreateImage("Border", transform, new Color(0.78f, 0.62f, 0.35f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            new Vector2(6f, 0f), Vector2.zero);

        // Ust baslik bolumu (yukseklik 140 -> 200, uzun basliklara yer)
        var header = CreateImage("Header", transform, new Color(0.20f, 0.13f, 0.08f, 1f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, 200f), Vector2.zero);

        // ESER BILGISI etiketi - ust kenardan biraz daha asagida (header'in %62-%85)
        // Anchor: (0, 0.62) - (1, 0.85) - ust kenardan ~15% asagida + altinda title icin %62 alan
        CreateText("Label", header.transform, font, "ESER BİLGİSİ", 20,
            new Color(0.78f, 0.62f, 0.35f, 0.9f), FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 0.85f),
            new Vector2(-40f, 0f), new Vector2(15f, 0f));

        // Baslik - header'in alt %58'inde (label'a daha az tasma riski)
        titleText = CreateText("TitleText", header.transform, font, "Tablo Adı", 32,
            new Color(0.92f, 0.82f, 0.62f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.58f),
            new Vector2(-40f, 0f), new Vector2(15f, 0f));
        titleText.resizeTextForBestFit = true;
        titleText.resizeTextMinSize = 18;
        titleText.resizeTextMaxSize = 34;

        // Ayirici
        CreateImage("Divider", transform, new Color(0.78f, 0.62f, 0.35f, 0.4f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-20f, 1.5f), new Vector2(0f, -200f));

        // Aciklama (15 soldan, daha sola yakin)
        descriptionText = CreateText("DescriptionText", transform, font, "Açıklama buraya gelecek.", 24,
            new Color(0.92f, 0.87f, 0.78f, 1f), FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-50f, -280f), new Vector2(15f, -215f));
        descriptionText.lineSpacing = 1.5f;
        descriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descriptionText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    // Yardimci: Image olustur
    GameObject CreateImage(string name, Transform parent, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    // Yardimci: Text olustur
    Text CreateText(string name, Transform parent, Font font, string text, int size,
        Color color, FontStyle style, TextAnchor anchor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var txt = go.AddComponent<Text>();
        txt.text = text;
        txt.font = font;
        txt.fontSize = size;
        txt.color = color;
        txt.fontStyle = style;
        txt.alignment = anchor;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return txt;
    }

    T FindInChildren<T>(string objName) where T : Component
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == objName) return t.GetComponent<T>();
        return null;
    }

    public void Show(string title, string description, Sprite img = null)
    {
        gameObject.SetActive(true);
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (image != null)
        {
            if (img != null) { image.sprite = img; image.gameObject.SetActive(true); }
            else image.gameObject.SetActive(false);
            if (imageContainer != null) imageContainer.SetActive(img != null);
        }
        IsOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}