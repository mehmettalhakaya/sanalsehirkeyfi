using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Esc ile acilan ayarlar menusu.
/// - Ses (master volume slider)
/// - Grafik kalitesi (dropdown)
/// - Mouse hassasiyeti (slider)
/// - Devam et butonu
/// - Cikis butonu
///
/// Bu component menunun root'una eklenir. UI elemanlari Inspector'dan
/// surukleyerek baglanir veya LouvreSetup tarafindan otomatik baglanir.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("UI Elemanlari (Otomatik baglanir)")]
    public Slider volumeSlider;
    public Text volumeLabel;
    public Dropdown qualityDropdown;
    public Slider mouseSensSlider;
    public Text mouseSensLabel;
    public Button resumeButton;
    public Button quitButton;

    [Header("Hedefler")]
    public PlayerController playerController;

    public bool IsOpen { get; private set; }

    private float defaultVolume = 0.8f;
    private float defaultMouseSens = 0.15f;

    void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Hide);
        if (quitButton != null)   quitButton.onClick.AddListener(QuitApp);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = defaultVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            OnVolumeChanged(defaultVolume);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (mouseSensSlider != null)
        {
            mouseSensSlider.minValue = 0.05f;
            mouseSensSlider.maxValue = 1.0f;
            mouseSensSlider.value = defaultMouseSens;
            mouseSensSlider.onValueChanged.AddListener(OnMouseSensChanged);
        }

        gameObject.SetActive(false);
        IsOpen = false;
    }

    void Update()
    {
        if (ReadEscapePressed())
        {
            if (IsOpen) Hide();
            else Show();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        IsOpen = true;
        if (playerController != null)
        {
            playerController.SetInputLocked(true);
            playerController.LockCursor(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        Time.timeScale = 0f;  // oyunu durdur
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        IsOpen = false;
        Time.timeScale = 1f;
        if (playerController != null)
        {
            playerController.SetInputLocked(false);
            playerController.LockCursor(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnVolumeChanged(float v)
    {
        AudioListener.volume = v;
        if (volumeLabel != null) volumeLabel.text = $"Ses: %{Mathf.RoundToInt(v * 100f)}";
    }

    void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    void OnMouseSensChanged(float v)
    {
        if (playerController != null) playerController.mouseSensitivity = v;
        if (mouseSensLabel != null) mouseSensLabel.text = $"Mouse Hassasiyeti: {v:F2}";
    }

    private bool ReadEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.escapeKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.Escape);
    }
}
