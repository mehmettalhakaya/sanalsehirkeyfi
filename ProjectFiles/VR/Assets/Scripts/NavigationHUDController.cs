using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Ekranda kontrolleri gosteren yardim overlay'i.
/// H tusu ile ac/kapa.
///
/// Setup tarafindan otomatik olarak Canvas'a eklenir.
/// </summary>
public class NavigationHUDController : MonoBehaviour
{
    [Header("UI")]
    public Text helpText;
    public GameObject panel;

    [Header("Ayarlar")]
    public bool startVisible = true;

    private static readonly string DEFAULT_HELP =
        "<b>Kontroller</b>\n" +
        "WASD - Yuru\n" +
        "Shift - Kos\n" +
        "Space - Atla / Yukari (ucusta)\n" +
        "Ctrl - Asagi (ucusta)\n" +
        "F - Ucus modunu ac/kapa\n" +
        "Mouse - Bak\n" +
        "Sol Tik - Yapi bilgisini ac\n" +
        "Sag Tik (basili tut) - Teleport\n" +
        "H - Bu yardimi ac/kapa\n" +
        "Esc - Ayarlar menusu";

    void Awake()
    {
        if (helpText != null) helpText.text = DEFAULT_HELP;
        if (panel != null) panel.SetActive(startVisible);
    }

    void Update()
    {
        if (ReadHelpToggle())
        {
            if (panel != null) panel.SetActive(!panel.activeSelf);
        }
    }

    private bool ReadHelpToggle()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.hKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.H);
    }
}
