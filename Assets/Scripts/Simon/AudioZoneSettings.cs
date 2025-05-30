using UnityEngine;
using FMODUnity;
public class AudioZoneSettings : MonoBehaviour
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
        public Vector3 position;
    }
    
    [Header("AudioSettings")]
    public AudioSettings[] audioSettings;

    private AudioManager aM;
    
    void Start()
    {
        aM = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
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
