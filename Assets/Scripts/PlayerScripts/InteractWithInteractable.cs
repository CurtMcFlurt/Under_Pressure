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
    //[SerializeField] private float minScale = 0.05f;
    // [SerializeField] private float maxScale = 0.15f;

     [SerializeField] private float fixedScale = 0.005f;

    [SerializeField] public GameObject aimIndicator; // Assign a sphere in the inspector
    [SerializeField] private float rayOffset = 0.5f;
    [SerializeField] private float maxRayDistance = 10f;
    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions.FindAction("Attack");
        }

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
        bool intearct = interactValue > .5f;
        if (intearct && timeSinceInteract < minTimeBetweenInteraction)
        {
            timeSinceInteract = minTimeBetweenInteraction;
            CheckInteractRay();

        } else if (timeSinceInteract > 0) timeSinceInteract -= Time.deltaTime;
        if (aimIndicator != null) { UpdateAimIndicator(); }

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
