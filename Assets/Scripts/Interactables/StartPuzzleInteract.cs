using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class StartPuzzleInteract : Interactable
{
    public TakeTheCamera myCameraNow;
    public GameObject CameraPoint;
    public Collider interactCollider;
    public GameObject lidObject;
    public CogPuzzleManager manager;
    public float lidRotation = 90f; // Desired rotation in degrees
    public float lidLerpSpeed = 2f; // How fast the lid opens/closes
    public bool lockMe;
    public NetworkObject puzzleObject;
    

    public void OnEnable()
    {
        interactCollider = GetComponent<Collider>();
    }

    public override void StartInteraction(GameObject sender)
    {
        if (lockMe) { return; }
        var take = sender.GetComponent<TakeTheCamera>();
        myCameraNow = take;
        take.activeStealer = this;
        take.StealThecamera(CameraPoint);
        NetworkObject t;
        sender.TryGetComponent<NetworkObject>(out t);
        if(t!=null)puzzleObject.ChangeOwnership(t.OwnerClientId);
        Debug.Log(t.gameObject.name);
        taken = true;
    }

    public void Update()
    {
        if (taken)
        {
            interactCollider.enabled = false;

            if (lidObject != null)
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, -lidRotation);
                lidObject.transform.localRotation = Quaternion.Lerp(
                    lidObject.transform.localRotation,
                    targetRotation,
                    Time.deltaTime * lidLerpSpeed
                );
            }
        }
        else
        {
            interactCollider.enabled = true;
            myCameraNow = null;

            if (lidObject != null)
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
                lidObject.transform.localRotation = Quaternion.Lerp(
                    lidObject.transform.localRotation,
                    targetRotation,
                    Time.deltaTime * lidLerpSpeed
                );
            }
        }
    }
    public void UnPackData(Component sender, object data)
    {
        // Check if the data is a string before proceeding
        if (data is string keyName)
        {
            Debug.Log($"Scene change requested: {keyName}");

            if (keyName == manager.sendString) { taken = false; lockMe = true; }
        }
        else
        {
            Debug.LogWarning("Data is not a valid scene name string.");
        }

        
    }
    public void ReleasePuzzle()
    {
        taken = false;
    }
}
