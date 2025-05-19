using System;
using UnityEngine;
using FMODUnity;

[System.Serializable]

public class AudioTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct AudioSettings
    {
        public TriggerAction Action;
        public BackgroundMusicEvents bgmEvent;
        public string paramName;
        public float paramValue;
        public bool paramIsGlobal;
        public bool IgnoreFadeOut;
        public bool IgnoreSeek;
        public bool exit;
        public Vector3 position;
    }
    
    [Header("General Settings")]
    public string requiredTag = "Player";
    public bool destroyAfterUse = false;
    
    [Header("TriggerSettings")]
    [NonReorderable] public AudioSettings[] audioSettings;

    private AudioManager aM;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aM = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == requiredTag)
        {
            Debug.Log("Triggered!");
            foreach (AudioSettings aS in audioSettings)
            {
                if(aS.exit) continue;
                switch (aS.Action)
                {
                    case TriggerAction.None: 
                        Debug.Log("You haven't entered a valid Trigger Action"); 
                        break;
                    case TriggerAction.Play:
                        Debug.Log("Activating Play"); 
                        aM.PlayMusic(aS.bgmEvent, aS.position); 
                        Debug.Log("Active Event is " + aS.bgmEvent);
                        break;
                    case TriggerAction.Stop: 
                        aM.StopMusic(aS.bgmEvent, aS.IgnoreFadeOut); 
                        break;
                    case TriggerAction.SetParameter: 
                        aM.SetParameter(aS.bgmEvent, aS.paramName, aS.paramValue, aS.IgnoreSeek, aS.paramIsGlobal); 
                        Debug.Log(aS.paramName + " is = " + aS.paramValue);
                        break;
                    
                }
            }
            
            if (destroyAfterUse == true)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == requiredTag)
        {
            Debug.Log("Triggered!");
            foreach (AudioSettings aS in audioSettings)
            {
                if(!aS.exit) continue;
                switch (aS.Action)
                {
                    case TriggerAction.None:
                        Debug.Log("You haven't entered a valid Trigger Action");
                        break;
                    case TriggerAction.Play:
                        Debug.Log("Activating Play");
                        aM.PlayMusic(aS.bgmEvent, aS.position);
                        break;
                    case TriggerAction.Stop:
                        aM.StopMusic(aS.bgmEvent, aS.IgnoreFadeOut);
                        break;
                    case TriggerAction.SetParameter:
                        aM.SetParameter(aS.bgmEvent, aS.paramName, aS.paramValue, aS.IgnoreSeek, aS.paramIsGlobal);
                        Debug.Log(aS.paramName + " is = " + aS.paramValue);
                        break;
                }
            }
        }
    }
}
