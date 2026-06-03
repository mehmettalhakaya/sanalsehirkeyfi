using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInteraction : MonoBehaviour
{
    [Header("Etkilesim")]
    public float maxDistance = 200f;
    public InfoPanelController infoPanel;
    public LayerMask interactionLayers = ~0;

    [Header("Referanslar")]
    public Camera viewCamera;
    public PlayerController playerController;

    private InteractableObject currentHover;

    void Awake()
    {
        if (viewCamera == null)
            viewCamera = GetComponentInChildren<Camera>();
        if (viewCamera == null)
            viewCamera = Camera.main;
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (infoPanel != null && infoPanel.IsOpen)
        {
            ClearHover();
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.X))
            {
                infoPanel.Hide();
                if (playerController != null)
                {
                    playerController.SetInputLocked(false);
                    playerController.LockCursor(true);
                }
            }
            return;
        }

        if (viewCamera == null) return;

        Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayers))
        {
            InteractableObject io = hit.collider.GetComponentInParent<InteractableObject>();
            if (io != null)
            {
                if (io != currentHover)
                {
                    ClearHover();
                    currentHover = io;
                    currentHover.SetHover(true);
                }
                if (ReadClickPressed())
                {
                    if (infoPanel != null)
                    {
                        infoPanel.Show(io.title, io.description, io.imageOptional);
                        if (playerController != null)
                        {
                            playerController.SetInputLocked(true);
                            playerController.LockCursor(false);
                        }
                    }
                }
                return;
            }
        }
        ClearHover();
    }

    void ClearHover()
    {
        if (currentHover != null)
        {
            currentHover.SetHover(false);
            currentHover = null;
        }
    }

    private bool ReadClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
#endif
        return Input.GetMouseButtonDown(0);
    }
}