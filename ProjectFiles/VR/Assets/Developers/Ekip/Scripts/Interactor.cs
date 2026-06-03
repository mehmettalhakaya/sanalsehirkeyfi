using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform InteractorSource;
    public float InteractRange = 3f;

    [Header("UI")]
    public GameObject interactPromptUI;
    public InfoPanel infoPanel;          // <<< InfoPanel scripti olan objeyi sürükle

    void Update()
    {
        // Panel açıkken raycast yapma
        if (infoPanel != null && infoPanel.IsOpen())
            return;

        Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
        bool showPrompt = false;

        if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                showPrompt = true;

                // World Space kullanıyorsan pozisyon ata
                if (interactPromptUI != null)
                {
                    Vector3 pos = hitInfo.collider.transform.position + new Vector3(0f, 1.5f, 0f);
                    interactPromptUI.transform.position = pos;
                    interactPromptUI.transform.LookAt(
                        interactPromptUI.transform.position + Camera.main.transform.rotation * Vector3.forward,
                        Camera.main.transform.rotation * Vector3.up
                    );
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactObj.Interact();

                    // Paneli aç
                    if (infoPanel != null)
                        infoPanel.OpenPanel(interactObj);
                }
            }
        }

        if (interactPromptUI != null && interactPromptUI.activeSelf != showPrompt)
            interactPromptUI.SetActive(showPrompt);
    }
}