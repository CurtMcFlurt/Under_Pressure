using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class StartPuzzleInteract : Interactable
{
    public TakeTheCamera myCameraNow;
    public GameObject CameraPoint;
    public Collider interactCollider;

    public void OnEnable()
    {
        interactCollider = GetComponent<Collider>();
    }
    public override void StartInteraction(GameObject sender)
    {
        var take = sender.GetComponent<TakeTheCamera>();
        myCameraNow = take;
        take.activeStealer = this;
        take.StealThecamera(CameraPoint);

        taken= true;
    }

    public void Update()
    {
        if (taken)
        {
            interactCollider.enabled = false;
        }
        else { 
            interactCollider.enabled = true;
            myCameraNow = null;
        }
    }
}
