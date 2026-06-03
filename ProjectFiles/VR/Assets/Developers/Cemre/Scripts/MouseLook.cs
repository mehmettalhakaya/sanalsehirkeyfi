using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Ayarlar")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Bağlantılar")]
    public InfoPanel infoPanel; // <<< Inspector'dan InfoPanel'i sürükle

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Panel açıksa mouse hareketi durdur
        if (infoPanel != null && infoPanel.IsOpen())
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}