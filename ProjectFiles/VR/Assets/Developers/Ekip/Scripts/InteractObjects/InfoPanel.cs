using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    [Header("Panel Elemanları")]
    public GameObject panelRoot;         // Ana panel objesi
    public TMP_Text titleText;           // Başlık
    public TMP_Text descriptionField; // Düzenlenebilir açıklama
    public Button closeButton;           // Kapat butonu

    // Panel açıkken oyuncu hareketi kilitlensin mi?
    [Header("Ayarlar")]
    public bool lockCursorOnOpen = true;

    private IInteractable currentInteractable;

    void Start()
    {
        panelRoot.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    // Dışarıdan çağrılır
    public void OpenPanel(IInteractable interactable)
    {
        currentInteractable = interactable;

        titleText.text = interactable.GetTitle();
        descriptionField.text = interactable.GetDescription();

        panelRoot.SetActive(true);

        if (lockCursorOnOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClosePanel()
    {
        // Değişikliği kaydet
        if (currentInteractable is InteractableObject obj)
        {
            obj.SetDescription(descriptionField.text);
        }

        panelRoot.SetActive(false);
        currentInteractable = null;

        if (lockCursorOnOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsOpen() => panelRoot.activeSelf;

    void Update()
    {
        // ESC ile de kapat
        if (IsOpen() && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }
}