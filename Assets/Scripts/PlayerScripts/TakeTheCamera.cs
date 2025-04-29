using UnityEngine;
using UnityEngine.InputSystem;

public class TakeTheCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public GameObject OriginObject;
    private InteractWithInteractable inting;
    private InputAction escapeAction;
    private InputAction backAction;
    private DebugMovement myMovement;
    public GameObject DebugObject;
    public bool Stolen;
    public Interactable activeStealer;
    public void OnEnable()
    {
        // Get components
        inting = GetComponent<InteractWithInteractable>();
        myMovement = GetComponent<DebugMovement>();

        // Initialize and enable input actions
        escapeAction = new InputAction("Escape");
        backAction = new InputAction("Back");

        escapeAction.Enable();
        backAction.Enable();

        // Optional: Add callbacks
        escapeAction.performed += OnEscapePressed;
        backAction.performed += OnBackPressed;
    }

    // Optional input handlers
    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        // Your escape logic
        Debug.Log("Escape");
    }

    private void OnBackPressed(InputAction.CallbackContext context)
    {
        // Your back button logic

        Debug.Log("Back");
    }

    public void OnDisable()
    {
        escapeAction?.Disable();
        backAction?.Disable();
    }

    public void Update()
    {
        if(DebugObject != null && !Stolen)StealThecamera(DebugObject);
        if(DebugObject == null &&  Stolen)ResetPoint();
        if (!activeStealer.taken) { ResetPoint(); }
        myMovement.StopMoving = Stolen;
    }

    public void StealThecamera(GameObject Point)
    {
        cameraTransform.parent = Point.transform;
        cameraTransform.position=Point.transform.position;
        Stolen = true;
        Debug.Log("Stolen");
    }

    public void ResetPoint()
    {
        cameraTransform.parent = null;
        Stolen = false;
        Debug.Log("Back");
    }

}
