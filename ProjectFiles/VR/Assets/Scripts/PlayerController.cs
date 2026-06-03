using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Birinci sahis kamera + karakter kontrolu.
/// CharacterController ile fizik+carpisma, mouse look, yuruyus/kosma.
///
/// Unity 6 yeni Input System ile uyumludur. Eski Input class'i da destekler
/// ("Both" modunda).
///
/// Kontroller:
///   WASD            - Yuru
///   Shift           - Kos (3x hiz)
///   Space           - Atla / yukari uc (ucus modunda)
///   Ctrl            - Asagi alcal (ucus modunda)
///   F               - Ucus modu ac/kapa
///   Mouse           - Bak
///   Esc             - Settings menusunu ac
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Hizi")]
    public float walkSpeed = 8f;
    public float sprintMultiplier = 3f;
    public float verticalSpeed = 8f;

    [Header("Mouse")]
    public float mouseSensitivity = 0.15f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;

    [Header("Fizik")]
    public float gravity = -20f;
    public float jumpHeight = 2.5f;

    [Header("Mod")]
    [Tooltip("Acikken yer cekimi devre disi - ucabilirsin")]
    public bool flyMode = false;

    [Header("Referanslar")]
    [Tooltip("Bu kamera mouse rotasyonu icin kullanilir. Bos birakirsa MainCamera kullanilir.")]
    public Transform cameraTransform;

    // Internal
    private CharacterController controller;
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector3 velocity = Vector3.zero;
    private bool inputLocked = false;  // SettingsMenu/InfoPanel acikken hareket disable

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
        {
            cameraTransform = transform;
        }
    }

    void Start()
    {
        Vector3 angles = cameraTransform.eulerAngles;
        yaw = angles.y;
        pitch = NormalizePitch(angles.x);
        LockCursor(true);
    }

    void Update()
    {
        if (!inputLocked)
        {
            HandleMouseLook();
            HandleMovement();
            HandleModeToggle();
        }
        else
        {
            // Bile menu acikken yer cekimi cikari uygulanir
            ApplyGravityOnly();
        }
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = ReadMouseDelta();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void HandleMovement()
    {
        Vector2 move = ReadMoveInput();   // h, v
        bool sprint = ReadSprintInput();
        bool jumpPressed = ReadJumpPressed();
        bool jumpHeld = ReadJumpHeld();
        bool downHeld = ReadDownHeld();

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        if (!flyMode)
        {
            forward.y = 0; forward.Normalize();
            right.y = 0; right.Normalize();
        }

        Vector3 dir = (forward * move.y + right * move.x);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float speed = walkSpeed * (sprint ? sprintMultiplier : 1f);

        if (flyMode)
        {
            // Ucus modu: yer cekimi yok, Space yukari, Ctrl asagi
            if (jumpHeld) dir += Vector3.up;
            if (downHeld) dir += Vector3.down;
            controller.Move(dir * speed * Time.deltaTime);
            velocity = Vector3.zero;
        }
        else
        {
            // Yer cekimi modu
            if (controller.isGrounded)
            {
                if (velocity.y < 0) velocity.y = -2f;  // hafif yere yapis
                if (jumpPressed)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }

            Vector3 horizontal = dir * speed;
            controller.Move((horizontal + Vector3.up * velocity.y) * Time.deltaTime);
        }
    }

    void HandleModeToggle()
    {
        if (ReadFlyToggle())
        {
            flyMode = !flyMode;
            velocity = Vector3.zero;
        }
    }

    void ApplyGravityOnly()
    {
        if (flyMode) return;
        if (controller.isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    // Public API ----------------------------------------------------------

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    public void TeleportTo(Vector3 worldPos)
    {
        // CharacterController'i devre disi birakmadan Move kullanmak gerekir
        controller.enabled = false;
        transform.position = worldPos;
        controller.enabled = true;
    }

    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // Input okuma yardimcilari (yeni + eski uyumlu) -----------------------

    private static float NormalizePitch(float angle)
    {
        // Unity euler 0..360 -> -180..180
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private Vector2 ReadMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.delta.ReadValue();
#endif
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
    }

    private Vector2 ReadMoveInput()
    {
        float h = 0f, v = 0f;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v -= 1f;
            return new Vector2(h, v);
        }
#endif
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        return new Vector2(h, v);
    }

    private bool ReadSprintInput()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.leftShiftKey.isPressed;
#endif
        return Input.GetKey(KeyCode.LeftShift);
    }

    private bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.spaceKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool ReadJumpHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.spaceKey.isPressed;
#endif
        return Input.GetKey(KeyCode.Space);
    }

    private bool ReadDownHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.leftCtrlKey.isPressed;
#endif
        return Input.GetKey(KeyCode.LeftControl);
    }

    private bool ReadFlyToggle()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.fKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.F);
    }
}
