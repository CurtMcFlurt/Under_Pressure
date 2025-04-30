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
    void OnEnable()
    {
        // Get components
        inting = GetComponent<InteractWithInteractable>();
        myMovement = GetComponent<DebugMovement>();

        // Get actions from PlayerInput component
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            backAction = playerInput.actions.FindAction("Back");
            escapeAction = playerInput.actions.FindAction("Escape");

            backAction?.Enable();
            escapeAction?.Enable();

        
        }

    }

    void OnDisable()
    {
        escapeAction?.Disable();
        backAction?.Disable();
    }

    public void Update()
    {
        if(DebugObject != null && !Stolen)StealThecamera(DebugObject);
        if(DebugObject == null &&  Stolen)ResetPoint();
        if (activeStealer!=null && !activeStealer.taken) { ResetPoint(); }
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
