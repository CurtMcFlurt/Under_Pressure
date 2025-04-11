using System;
using UnityEngine;
using FMODUnity;

public enum TriggerAction
{
    None,
    Play,
    Stop,
    SetParameter,
    LoadBank
}
public class AudioTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct AudioSettings
    {
        public TriggerAction tAction;
        public string eventEmitterName;
        public string paramName;
        public float paramValue;
    }
    
    [Header("AudioSettings")]
    public AudioSettings aSettings;

    private MusicManager mManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>().musicManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered!");
        
        switch (aSettings.tAction)
        {
            case TriggerAction.None: Debug.Log("You haven't entered a valid Trigger Action");
                break;
            case TriggerAction.Play:
                Debug.Log("Activating Play");
                mManager.PlayMusic(aSettings.eventEmitterName); 
                break;
            case TriggerAction.Stop:
                mManager.StopMusic(aSettings.eventEmitterName);
                break;
            case TriggerAction.SetParameter: 
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
