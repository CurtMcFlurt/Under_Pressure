using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 5f;

    public float walkingSpeed = 5f;
    public float runningSpeed = 5f;
    public float sneakingSpeed = 5f;
    public float lookSpeed = 2f;
    public float stamina=1;
    public float exhaust = 200;
    public float regenerate = 200;
    public Transform cameraTransform;
    public Rigidbody rb;
    public GameObject glowStick;
    public Transform throwPoint;
    public float throwForce = 10f;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction throwAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private float xRotation = 0f;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions.FindAction("Move");
            lookAction = playerInput.actions.FindAction("Look");
            throwAction = playerInput.actions.FindAction("Interact");
            sprintAction = playerInput.actions.FindAction("Sprint");
            crouchAction = playerInput.actions.FindAction("Crouch");

        }

        moveAction.Enable();
        lookAction.Enable();
        throwAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        throwAction.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float crouchValue = crouchAction.ReadValue<float>();
        float sprintValue = sprintAction.ReadValue<float>();
        bool isCrouching = crouchValue > 0.5f;
        bool isSprinting = sprintValue > 0.5f && !runCD;
        if (isCrouching)
        {
            moveSpeed = sneakingSpeed;
            RegenerateStamina();
        }
        else if (isSprinting)
        {
            moveSpeed = runningSpeed;
            stamina -= Time.deltaTime * exhaust;

            if (stamina <= 0f)
            {
                stamina = 0f;
                runCD = true;
            }
        }
        else
        {
            moveSpeed = walkingSpeed;
            RegenerateStamina();
        }

        if (stamina >= 1f)
        {
            stamina = 1f;
            runCD = false;
        }
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }
    private bool runCD;
    private void Update()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (throwAction.WasPressedThisFrame())
        {
            ThrowGlowStick();
        }
       

    }
    void RegenerateStamina()
    {
        if (stamina < 1f)
        {
            stamina += Time.deltaTime * regenerate;
        }
    }

    private void ThrowGlowStick()
    {
        GameObject thrownGlowStick = Instantiate(glowStick, throwPoint.position, throwPoint.rotation);
        Rigidbody glowStickRb = thrownGlowStick.GetComponent<Rigidbody>();
        if (glowStickRb != null)
        {
            Vector3 playerVelocity = rb.linearVelocity;
            glowStickRb.linearVelocity = playerVelocity; // Inherit player's velocity
            glowStickRb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
        }
    }
}
