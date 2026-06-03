using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// INSANSI YURUYUS KONTROLCUSU
/// Kamera Player'a parent OLABILIR (klasik) ya da AYRI obje olabilir (scale Y=0 ise).
/// useWorldSpaceCamera = true ise kamera world space'te yonetilir (LateUpdate ile).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class HumanoidController : MonoBehaviour
{
    [Header("Hareket Hizlari")]
    public float walkSpeed = 4.5f;
    public float sprintSpeed = 7.5f;
    public float crouchSpeed = 2.0f;

    [Header("Ziplama ve Yer Cekimi")]
    public float jumpHeight = 0.5f;        // 50cm - hem dis hem ic
    public float gravity = -9.81f;

    [Header("Boy Ayarlari")]
    public float standHeight = 1.85f;
    public float crouchHeight = 0.9f;       // derin egilme (eski ayar)
    public float standCameraY = 0f;         // KULLANICI ISTEGI: dis sahne default 0
    public float crouchCameraY = 0f;        // crouch da 0 (dis sahne)
    public float crouchTransitionSpeed = 8f;

    [Header("Mouse")]
    public float mouseSensitivity = 0.15f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;

    [Header("Hareket Pürüzsüzlüğü")]
    public float accelTime = 0.1f;
    public float decelTime = 0.08f;

    [Header("Bas Salinim (opsiyonel)")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.6f;
    public float bobAmplitude = 0.04f;

    [Header("Referans")]
    public Transform cameraTransform;

    [Header("Kamera Modu")]
    [Tooltip("True ise kamera Player'a parent olmasa bile world space'te kontrol edilir. Player scale Y=0 olunca AKTIF olmali.")]
    public bool useWorldSpaceCamera = false;

    private CharacterController cc;
    private Vector3 velocity;
    private Vector3 currentHorizontalVelocity;
    private float yaw, pitch;
    private bool isCrouching;
    private float bobTimer;

    // World space camera state
    private float currentCameraYOffset;  // current Y offset above player feet
    private Vector3 currentBobOffset;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.height = standHeight;
        cc.center = new Vector3(0, standHeight * 0.5f, 0);
    }

    void Start()
    {
        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        // Player scale Y=0 ise otomatik world space mode
        if (Mathf.Approximately(transform.localScale.y, 0f))
        {
            useWorldSpaceCamera = true;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = 0f;
        currentCameraYOffset = standCameraY;

        if (cameraTransform != null)
        {
            if (useWorldSpaceCamera)
            {
                // World space - kamera Player'a parent OLMAMALI, biz konumlariz
                cameraTransform.position = transform.position + Vector3.up * standCameraY;
                cameraTransform.rotation = Quaternion.Euler(0, yaw, 0);
            }
            else
            {
                // Klasik - parent altinda localPosition
                cameraTransform.localPosition = new Vector3(0, standCameraY, 0);
                cameraTransform.localRotation = Quaternion.identity;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleCrouch();
        HandleMovement();
        if (enableHeadBob) HandleHeadBob();

        if (ReadEscapePressed())
        {
            bool wasLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = wasLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = wasLocked;
        }
        if (Cursor.lockState != CursorLockMode.Locked && ReadLeftMouseDown())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        // World space mode'da kamera pozisyon/rotasyonunu burada uygula
        if (useWorldSpaceCamera && cameraTransform != null)
        {
            Vector3 basePos = transform.position + Vector3.up * currentCameraYOffset;
            Vector3 bobWorld = transform.right * currentBobOffset.x + Vector3.up * currentBobOffset.y;
            cameraTransform.position = basePos + bobWorld;
            cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
    }

    private int framesSinceStart = 0;

    void HandleLook()
    {
        framesSinceStart++;
        if (Cursor.lockState != CursorLockMode.Locked) return;
        Vector2 m = ReadMouseDelta();
        if (framesSinceStart < 5) m = Vector2.zero;
        m.x = Mathf.Clamp(m.x, -200f, 200f);
        m.y = Mathf.Clamp(m.y, -200f, 200f);

        yaw += m.x * mouseSensitivity;
        pitch -= m.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Player yaw'i her zaman uygulanir
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        // Classic mode'da camera localRotation pitch
        if (!useWorldSpaceCamera && cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
        // World space mode'da LateUpdate halleder
    }

    void HandleCrouch()
    {
        bool wantCrouch = ReadCrouchHeld();
        isCrouching = wantCrouch;

        float targetHeight = wantCrouch ? crouchHeight : standHeight;
        float targetCamY = wantCrouch ? crouchCameraY : standCameraY;

        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        cc.center = new Vector3(0, cc.height * 0.5f, 0);

        // Smooth camera Y offset
        currentCameraYOffset = Mathf.Lerp(currentCameraYOffset, targetCamY, Time.deltaTime * crouchTransitionSpeed);

        if (!useWorldSpaceCamera && cameraTransform != null)
        {
            Vector3 p = cameraTransform.localPosition;
            p.y = currentCameraYOffset;
            cameraTransform.localPosition = p;
        }
    }

    void HandleMovement()
    {
        Vector2 input = ReadMoveInput();
        bool sprint = ReadSprintInput();

        float speed;
        if (isCrouching) speed = crouchSpeed;
        else if (sprint) speed = sprintSpeed;
        else speed = walkSpeed;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 desired = (forward * input.y + right * input.x);
        if (desired.sqrMagnitude > 1f) desired.Normalize();
        desired *= speed;

        float t = (desired.sqrMagnitude > 0.01f) ? accelTime : decelTime;
        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            desired,
            (speed / Mathf.Max(t, 0.01f)) * Time.deltaTime
        );

        if (cc.isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;
            if (ReadJumpPressed() && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 total = currentHorizontalVelocity + Vector3.up * velocity.y;
        cc.Move(total * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        Vector3 horizVel = new Vector3(currentHorizontalVelocity.x, 0, currentHorizontalVelocity.z);
        float speed = horizVel.magnitude;
        Vector3 targetBob = Vector3.zero;
        if (cc.isGrounded && speed > 0.2f)
        {
            float speedFactor = speed / walkSpeed;
            bobTimer += Time.deltaTime * bobFrequency * Mathf.PI * 2f * speedFactor;
            float yOff = Mathf.Sin(bobTimer) * bobAmplitude * speedFactor;
            float xOff = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f * speedFactor;
            targetBob = new Vector3(xOff, yOff, 0);
        }
        else
        {
            bobTimer = 0;
        }
        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBob, Time.deltaTime * 10f);

        // Classic mode: localPosition'a bob uygula
        if (!useWorldSpaceCamera && cameraTransform != null)
        {
            Vector3 p = cameraTransform.localPosition;
            p.x = currentBobOffset.x;
            p.z = currentBobOffset.z;
            // y already set by crouch
            p.y = currentCameraYOffset + currentBobOffset.y;
            cameraTransform.localPosition = p;
        }
    }

    public void TeleportTo(Vector3 worldPos)
    {
        cc.enabled = false;
        transform.position = worldPos;
        cc.enabled = true;
    }

    // ===== Input ======================================================
    private Vector2 ReadMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.delta.ReadValue();
#endif
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
    }

    private Vector2 ReadMoveInput()
    {
        float h = 0, v = 0;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1;
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

    private bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.spaceKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool ReadCrouchHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.leftCtrlKey.isPressed || kb.cKey.isPressed;
#endif
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
    }

    private bool ReadEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.escapeKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private bool ReadLeftMouseDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
#endif
        return Input.GetMouseButtonDown(0);
    }
}
