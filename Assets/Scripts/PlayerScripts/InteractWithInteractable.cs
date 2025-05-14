using UnityEngine;
using UnityEngine.InputSystem;

public class InteractWithInteractable : MonoBehaviour
{
    public Transform lookatPoint;
    public LayerMask interactLayer;
    public LayerMask locatorLayer;
    public float minTimeBetweenInteraction = .25f;
    public float raySize=3;
    private InputAction interactAction;
    private float timeSinceInteract;
    private InputAction escapeAction;
    private InputAction backAction;
    private TakeTheCamera cameraTaker;
    //[SerializeField] private float minScale = 0.05f;
    // [SerializeField] private float maxScale = 0.15f;

    [SerializeField] private float fixedScale = 0.005f;

    [SerializeField] public GameObject aimIndicator; // Assign a sphere in the inspector
    [SerializeField] private float rayOffset = 0.5f;
    [SerializeField] private float maxRayDistance = 10f;
    private float waitTostop = 1.5f;
    private float currentWait=0;
    public bool intearct;
    public bool retract;
    public bool IsHolding;
    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions.FindAction("Attack");
            backAction = playerInput.actions.FindAction("Back");
            escapeAction = playerInput.actions.FindAction("Escape");
        }
        cameraTaker = GetComponent<TakeTheCamera>();
            if (lookatPoint == null) lookatPoint = transform;
       

    }
    private void OnDisable()
    {
        interactAction?.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        float interactValue = interactAction.ReadValue<float>();
        intearct = interactValue > .5f;
        if (intearct && timeSinceInteract < minTimeBetweenInteraction)
        {
            timeSinceInteract = minTimeBetweenInteraction;
            CheckInteractRay();

        } else if (timeSinceInteract > 0) timeSinceInteract -= Time.deltaTime;
        if (aimIndicator != null) { UpdateAimIndicator(); }
        float retractValue = backAction.ReadValue<float>();
        retract = retractValue > .5f;
        float escapeValue = escapeAction.ReadValue<float>();
        var esc = escapeValue > .5f;
        if (IsHolding) { currentWait = 0; } else if(currentWait<waitTostop) currentWait += Time.deltaTime;
        if(retract && currentWait > waitTostop ||esc)
        {
            Debug.Log("escape puzzle");
            cameraTaker.ResetPoint();

            currentWait = 0;
        }


    }

    private void CheckInteractRay()
    {
        // Visual debug line - shows up in Scene view
        Debug.DrawRay(lookatPoint.position, lookatPoint.forward * raySize, Color.red, 1.0f);

        RaycastHit hit;
        if (Physics.Raycast(lookatPoint.position, lookatPoint.forward, out hit, raySize, interactLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.StartInteraction(gameObject);
            }
        }
    }
   
    private void UpdateAimIndicator()
    {
        Vector3 origin = lookatPoint.position + lookatPoint.forward * rayOffset;

        RaycastHit hit;
        if (Physics.Raycast(origin, lookatPoint.forward, out hit, maxRayDistance, locatorLayer))
        {
            aimIndicator.SetActive(true);

            // Calculate scale based on camera distance and FOV
            float distanceToCamera = (Camera.main.transform.position - hit.point).magnitude;
            float scale = distanceToCamera * fixedScale/100 * Camera.main.fieldOfView;
            aimIndicator.transform.localScale = Vector3.one * scale;

            // Offset back along the ray by half the scaled size
            Vector3 offsetPosition = hit.point - lookatPoint.forward * (scale * 0.5f);
            aimIndicator.transform.position = offsetPosition;

            // Optionally orient it to face the camera
            aimIndicator.transform.forward = aimIndicator.transform.position - Camera.main.transform.position;
        }
        else
        {
            aimIndicator.SetActive(false);
        }

        Debug.DrawRay(origin, lookatPoint.forward * maxRayDistance, Color.cyan);
    }

}
