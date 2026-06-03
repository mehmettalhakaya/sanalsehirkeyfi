using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// BASIT UCAN KAMERA - Fiziksiz, carpisma yok, CharacterController yok.
/// Sadece WASD + mouse ile serbest uc.
/// Static batching ve karmasik optimizasyon icin kotu davranan
/// CharacterController yerine bunu kullan.
///
/// Kontroller:
///   WASD        - Yatay hareket
///   Space       - Yukari
///   Ctrl/Shift  - Asagi (Shift) veya hizli (LeftShift)
///   Q/E         - Yukari/Asagi alternatif
///   Mouse       - Bak (sag tik basili tutarken)
///   Mouse Wheel - Hiz ayarla
/// </summary>
public class SimpleFlyCamera : MonoBehaviour
{
    [Header("Hiz")]
    public float baseSpeed = 20f;
    public float sprintMultiplier = 4f;
    public float currentSpeed = 20f;

    [Header("Mouse")]
    public float mouseSensitivity = 0.2f;
    public float pitchMin = -89f;
    public float pitchMax = 89f;
    public bool requireRightClickToLook = false;  // false ise surekli bakar

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x > 180 ? angles.x - 360 : angles.x;
        currentSpeed = baseSpeed;

        if (!requireRightClickToLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleSpeedAdjust();
        HandleLook();
        HandleMove();
    }

    void HandleSpeedAdjust()
    {
        float scroll = ReadMouseScroll();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + scroll * 5f, 1f, 200f);
        }
    }

    void HandleLook()
    {
        bool canLook = !requireRightClickToLook || ReadRightMouseHeld();
        if (!canLook) return;

        Vector2 delta = ReadMouseDelta();
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void HandleMove()
    {
        Vector2 move = ReadMoveInput();
        bool sprint = ReadSprintInput();
        bool up = ReadUpHeld();
        bool down = ReadDownHeld();

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 dir = forward * move.y + right * move.x;
        if (up) dir += Vector3.up;
        if (down) dir += Vector3.down;

        float speed = currentSpeed * (sprint ? sprintMultiplier : 1f);
        transform.position += dir * speed * Time.deltaTime;
    }

    // ===== Input =====
    private Vector2 ReadMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.delta.ReadValue();
#endif
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
    }

    private float ReadMouseScroll()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.scroll.ReadValue().y / 120f;
#endif
        return Input.mouseScrollDelta.y;
    }

    private bool ReadRightMouseHeld()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.rightButton.isPressed;
#endif
        return Input.GetMouseButton(1);
    }

    private Vector2 ReadMoveInput()
    {
        float h = 0, v = 0;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed) h -= 1;
            if (kb.dKey.isPressed) h += 1;
            if (kb.wKey.isPressed) v += 1;
            if (kb.sKey.isPressed) v -= 1;
            return new Vector2(h, v);
        }
#endif
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private bool ReadSprintInput()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.leftShiftKey.isPressed;
#endif
        return Input.GetKey(KeyCode.LeftShift);
    }

    private bool ReadUpHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.spaceKey.isPressed || kb.eKey.isPressed;
#endif
        return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E);
    }

    private bool ReadDownHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.leftCtrlKey.isPressed || kb.qKey.isPressed;
#endif
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q);
    }
}
