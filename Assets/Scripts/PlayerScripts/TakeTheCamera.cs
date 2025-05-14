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
    private PlayerMovement myPlayerMovement;
    public GameObject DebugObject;
    public bool Stolen,Debuging;
    public Interactable activeStealer;
    void OnEnable()
    {
        // Get components
        inting = GetComponent<InteractWithInteractable>();
        myMovement = GetComponent<DebugMovement>();
        if (myMovement == null)myPlayerMovement= GetComponent<PlayerMovement>();
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
    private float cd = .25f;
    private float acd = 0;
    public void Update()
    {
        if(DebugObject != null && !Stolen && Debuging)StealThecamera(DebugObject);
        if(DebugObject == null &&  Stolen && Debuging)ResetPoint();
        if (activeStealer!=null && !activeStealer.taken) { ResetPoint(); }
        if (myMovement != null) { myMovement.StopMoving = Stolen; } else if (myPlayerMovement != null)
        {
            myPlayerMovement.StopMoving = Stolen;
        }
        var esc = escapeAction.ReadValue<float>();

        if (!Stolen && esc>0.5 &&acd<=0)
        {
            var r = FindAnyObjectByType<RelayConnectionManager>();
            r.uiPanel.SetActive(!r.uiPanel.activeInHierarchy);
            myPlayerMovement.StopMoving = !r.uiPanel.activeInHierarchy;

            if (!r.uiPanel.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else Cursor.lockState = CursorLockMode.None;
            Cursor.visible = !r.uiPanel.activeInHierarchy;
            acd = cd;
        }
        acd -= Time.deltaTime;
    }

    public void StealThecamera(GameObject Point)
    {
        
        cameraTransform.position=new Vector3(Point.transform.position.x,cameraTransform.position.y,Point.transform.position.z);
        Stolen = true;
        Debug.Log("Stolen");
    }

    public void ResetPoint()
    {
        Stolen = false;
        if (activeStealer != null)
        {
            activeStealer.taken = false;
            activeStealer = null;
            Debug.Log("Back");
        }
    }

}
