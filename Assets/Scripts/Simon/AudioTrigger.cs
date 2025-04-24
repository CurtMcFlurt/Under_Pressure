using System;
using UnityEngine;
using FMODUnity;

[System.Serializable]
public struct AudioSettings
{
    public TriggerAction Action;
    public string eventEmitterName;
    public string paramName;
    public float paramValue;
    public Vector3 position;
}
public class AudioTrigger : MonoBehaviour
{
    
    [Header("General Settings")]
    public string colliderTag;
    public bool destroyAfterUse = false;
    
    [Header("TriggerSettings")]
    public AudioSettings[] audioSettings;

    private MusicManager mManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>().musicManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == colliderTag)
        {
            Debug.Log("Triggered!");
            foreach (AudioSettings aS in audioSettings)
            {
                switch (aS.Action)
                {
                    case TriggerAction.None: 
                        Debug.Log("You haven't entered a valid Trigger Action"); 
                        break;
                    case TriggerAction.Play:
                        Debug.Log("Activating Play"); 
                        mManager.PlayMusic(aS.eventEmitterName,aS.position); 
                        break;
                    case TriggerAction.Stop: 
                        mManager.StopMusic(aS.eventEmitterName); 
                        break;
                    case TriggerAction.SetParameter: 
                        mManager.SetParameter(aS.eventEmitterName, aS.paramName, aS.paramValue); 
                        Debug.Log(aS.paramName + " is = " + aS.paramValue);
                        break;
                    case TriggerAction.OneShot:
                        mManager.PlayOneShot(aS.eventEmitterName,aS.position);
                        break;
                }
            }
            
            if (destroyAfterUse == true)
            {
                Destroy(gameObject);
            }
        }
    }
}
