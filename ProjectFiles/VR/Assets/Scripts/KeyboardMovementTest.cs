// Bu script yalnızca VR gözlük olmadan editör içinde test amacıyla eklenmiştir. Final PCVR testinde devre dışı bırakılabilir.
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class KeyboardMovementTest : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float turnSpeed = 90f;
    [SerializeField] float mouseTurnSpeed = 120f;
    [SerializeField] bool enableRightMouseYaw = true;
    [SerializeField] Transform viewReference;

    void Reset()
    {
        AssignDefaultViewReference();
    }

    void Awake()
    {
        AssignDefaultViewReference();
    }

    void Update()
    {
        ReadInput(out float horizontal, out float vertical, out float turn, out float mouseYaw);

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        Transform basis = viewReference != null ? viewReference : transform;
        Vector3 forward = Vector3.ProjectOnPlane(basis.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(basis.right, Vector3.up).normalized;

        transform.position += (right * input.x + forward * input.z) * moveSpeed * Time.deltaTime;

        float yaw = (turn * turnSpeed + mouseYaw * mouseTurnSpeed) * Time.deltaTime;
        if (!Mathf.Approximately(yaw, 0f))
        {
            transform.Rotate(Vector3.up, yaw, Space.World);
        }
    }

    void AssignDefaultViewReference()
    {
        if (viewReference == null && Camera.main != null)
        {
            viewReference = Camera.main.transform;
        }
    }

    void ReadInput(out float horizontal, out float vertical, out float turn, out float mouseYaw)
    {
        horizontal = 0f;
        vertical = 0f;
        turn = 0f;
        mouseYaw = 0f;
        bool usedNewInput = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            usedNewInput = true;
            if (keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed) horizontal += 1f;
            if (keyboard.wKey.isPressed) vertical += 1f;
            if (keyboard.sKey.isPressed) vertical -= 1f;
            if (keyboard.qKey.isPressed) turn -= 1f;
            if (keyboard.eKey.isPressed) turn += 1f;
        }

        Mouse mouse = Mouse.current;
        if (enableRightMouseYaw && mouse != null && mouse.rightButton.isPressed)
        {
            usedNewInput = true;
            mouseYaw = mouse.delta.ReadValue().x;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (!usedNewInput)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.Q)) turn -= 1f;
            if (Input.GetKey(KeyCode.E)) turn += 1f;
            if (enableRightMouseYaw && Input.GetMouseButton(1))
            {
                mouseYaw = Input.GetAxis("Mouse X");
            }
        }
#endif
    }
}
