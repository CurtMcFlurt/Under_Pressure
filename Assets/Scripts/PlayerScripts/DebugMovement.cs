using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugMovement : MonoBehaviour
{
    private float moveSpeed = 5f;

    public float walkingSpeed = 5f;
    public float runningSpeed = 5f;
    public float sneakingSpeed = 5f;
    public float lookSpeed = 2f;
    public float stamina = 1;
    public float exhaust = 1;
    public float regenerate = 1;
    public float crouchHeigtDivision = 2;
    public float extraRunningSound = 4;
    public int extraRunningRange = 3;
    private int regularRange;
    private float regularSound;
    public Transform cameraTransform;
    public Rigidbody rb;
    public GameObject glowStick;
    public GameObject cameraPoint;
    public Transform throwPoint;
    public float throwForce = 10f;
    public LayerMask layerstuff;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction throwAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private float xRotation = 0f;
    private CapsuleCollider myCollider;
    private float originalHeight;
    public WeightChangers walkSound;
    private Vector3 originCamera;
    private PlayerDeath myDeath;
    public float currentAngle;
    public Animator myAnim;
    public void OnEnable()
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

            moveAction.Enable();
            lookAction.Enable();
            throwAction.Enable();
        }
        myDeath = GetComponent<PlayerDeath>();
        myCollider = GetComponent<CapsuleCollider>();
        originalHeight = myCollider.height;
        originCamera = cameraPoint.transform.position - transform.position;

        if (walkSound != null)
        {
            regularSound = walkSound.myHeat.sound;
            regularRange = walkSound.range;
        }
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        throwAction?.Disable();
    }

    private bool forcedCrouch;
    private float increasingTvalue = 0;
    private void FixedUpdate()
    {
        if (myDeath.IsDead) return;
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float crouchValue = crouchAction.ReadValue<float>();
        float sprintValue = sprintAction.ReadValue<float>();
        bool isCrouching = crouchValue > 0.5f;
        bool isSprinting = sprintValue > 0.5f && !runCD;

        bool Hunched = false;
        if (isCrouching || forcedCrouch)
        {
            moveSpeed = sneakingSpeed;
            Hunched = isCrouching;
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
            if (walkSound != null) { walkSound.myHeat.sound = regularSound + extraRunningSound; walkSound.range = regularRange + extraRunningRange; }
        }
        else
        {
            moveSpeed = walkingSpeed;
            RegenerateStamina();
            if (walkSound != null) { walkSound.myHeat.sound = regularSound; walkSound.range = regularRange; }
        }
        myAnim.SetBool("Running", isSprinting);
        if (moveInput.magnitude > 0.1 && !isCrouching)
        {
            walkSound.enabled = true;
        }
        if (stamina >= 1f)
        {
            stamina = 1f;
            runCD = false;
        }
        if (Hunched)
        {
            hunchCharacter(true);
        }
        else hunchCharacter(false);
        myAnim.SetBool("Crouching", Hunched);
        if (Hunched || forcedCrouch)
        {
            if (increasingTvalue < 1)
            {
                increasingTvalue += Time.deltaTime * 2;

            }
            else increasingTvalue = 1;

            MoveCamera(increasingTvalue);
        }
        else
        {
            if (increasingTvalue > 0)
            {
                increasingTvalue -= Time.deltaTime * 2;
            }
            else increasingTvalue = 0;

            MoveCamera(increasingTvalue);

        }
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }
    private bool runCD;
    private void Update()
    {
        if (myDeath.IsDead) return;
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        GetAngle();
        if (throwAction.WasPressedThisFrame())
        {
            ThrowGlowStick();
        }

    }
    private bool wasHunched = false;
    private void hunchCharacter(bool wich)
    {
        if (wich && !wasHunched)
        {
            //start hunching
            myCollider.height = originalHeight / crouchHeigtDivision;
            myCollider.center = new Vector3(myCollider.center.x, -0.5f, myCollider.center.z);
            wasHunched = true;
        }

        if (!wich && wasHunched)
        {
            //return to normal
            var hits = Physics.OverlapCapsule(transform.position, transform.position + myCollider.height * 2 * Vector3.up, myCollider.radius, layerstuff);
            Debug.Log(hits.Length);

            if (hits.Length == 0)
            {
                myCollider.center = Vector3.zero;
                myCollider.height = originalHeight;
                wasHunched = false;
                forcedCrouch = false;
            }
            else forcedCrouch = true;


        }
    }

    void GetAngle()
    {
        Vector3 moveDir = rb.linearVelocity;
        moveDir.y = 0;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Convert movement direction to local-space values relative to camera
        float moveX = Vector3.Dot(moveDir.normalized, forward);
        float moveZ = Vector3.Dot(moveDir.normalized, right);
        
        myAnim.SetFloat("MoveX", moveX);
        myAnim.SetFloat("MoveZ", moveZ);
        myAnim.SetFloat("Speed", rb.linearVelocity.magnitude);
    }
    void RegenerateStamina()
    {
        if (stamina < 1f)
        {
            stamina += Time.deltaTime * regenerate;
        }
    }
    private void MoveCamera(float tValue)
    {
        cameraPoint.transform.position = Vector3.Lerp(originCamera + transform.position, throwPoint.transform.position, tValue);
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


