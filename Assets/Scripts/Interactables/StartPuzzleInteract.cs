using Unity.Netcode;
using UnityEngine;

public class StartPuzzleInteract : Interactable
{
    public TakeTheCamera myCameraNow;
    public GameObject CameraPoint;
    public Collider interactCollider;
    public GameObject lidObject;
    public CogPuzzleManager manager;
    public float lidRotation = 90f;
    public float lidLerpSpeed = 2f;
    public bool lockMe;
    public NetworkObject puzzleObject;
    public GameObject missingCog;
    public GameObject cogTomiss;

    public void OnEnable()
    {
        interactCollider = GetComponent<Collider>();
    }

    public override void StartInteraction(GameObject sender)
    {
        if (lockMe) return;

        bool myCog = false;

        if (missingCog != null)
        {
            var holder = sender.GetComponent<HoldAThing>();
            if (holder != null && holder.handHoldItem == missingCog)
            {
                missingCog = null;
                cogTomiss.SetActive(true);
                myCog = true;
                holder.DropHeldItem();
            }
        }

        var cameraUser = sender.GetComponent<TakeTheCamera>();
        myCameraNow = cameraUser;
        cameraUser.activeStealer = this;
        cameraUser.StealThecamera(CameraPoint);

        if (sender.TryGetComponent(out NetworkObject playerNetworkObj))
        {
            manager.RequestPuzzleStartRpc(playerNetworkObj.OwnerClientId, myCog);
        }

        taken = true;
    }

    public void Update()
    {
        if (manager.foundLost.Value) cogTomiss.SetActive(true);

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
        if (data is string keyName)
        {
            Debug.Log($"Scene change requested: {keyName}");

            if (keyName == manager.sendString)
            {
                taken = false;
                lockMe = true;
            }
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
