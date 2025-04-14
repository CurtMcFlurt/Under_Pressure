using UnityEngine;
using FMODUnity;
public class AudioZoneSettings : MonoBehaviour
{
    [System.Serializable]
    public struct AudioSettings
    {
        public TriggerAction Action;
        public string eventEmitterName;
        public string paramName;
        public float paramValue;
    }
    
    [Header("AudioSettings")]
    public AudioSettings[] audioSettings;

    private MusicManager mManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>().musicManager;
        GenerateAudioSettings();
    }

    private void GenerateAudioSettings()
    {
        foreach (AudioSettings aS in audioSettings)
        {
            switch (aS.Action)
            { 
                case TriggerAction.None: 
                    Debug.Log("You haven't entered a valid Trigger Action"); 
                    break;
                case TriggerAction.Play: 
                    Debug.Log("Activating Play"); 
                    mManager.PlayMusic(aS.eventEmitterName); 
                    break;
                case TriggerAction.Stop: 
                    mManager.StopMusic(aS.eventEmitterName); 
                    break;
                case TriggerAction.SetParameter: 
                    mManager.SetParameter(aS.eventEmitterName, aS.paramName, aS.paramValue); 
                    break;
                
            }
        }
    }
}
