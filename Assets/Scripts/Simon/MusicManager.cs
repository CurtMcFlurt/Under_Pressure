using FMOD;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Debug = UnityEngine.Debug;

public class MusicManager : MonoBehaviour
{
    public StudioEventEmitter bgm1, bgm2, dwListening, dwHunting, dwFoundYou;

    public void PlayMusic(string eventName,Vector3 pos)
    {
        Debug.Log("Entering playMusic");
        StudioEventEmitter playEvent = ChooseEvent(eventName);
        Debug.Log("Play event is " + playEvent);
        playEvent.transform.position = pos;
        
        if (playEvent.IsActive == false)
        {
            Debug.Log("Play event is active? " + playEvent.IsActive);
            playEvent.Play();
            Debug.Log("Play event is active? " + playEvent.IsActive);
        }
    }

    public void StopMusic(string eventName)
    {
        StudioEventEmitter stopEvent = ChooseEvent(eventName);

        if (stopEvent.IsActive == true)
        {
            stopEvent.Stop();
        }
    }
    
    public void SetParameter(string eventName, string parameterName, float parameterValue)
    {
        StudioEventEmitter paramEvent = ChooseEvent(eventName);
        
        paramEvent.SetParameter(parameterName, parameterValue);
    }

    public void PlayOneShot(string eventName, Vector3 pos)
    {
        StudioEventEmitter oneshotEvent = ChooseEvent(eventName);
        
        oneshotEvent.transform.position = pos;
        
        if (oneshotEvent.IsActive == true)
        {
            RuntimeManager.PlayOneShot(eventName);
        }
    }

    private StudioEventEmitter ChooseEvent(string eventName)
    {
        Debug.Log("Choosing event...");
        StudioEventEmitter emitter;
        
        switch (eventName) 
        { 
            case "bgm1": 
                emitter = bgm1; 
                break;
            case "bgm2": 
                emitter = bgm2; 
                break;
            case "dwListening": 
                Debug.Log("listening");
                emitter = dwListening; 
                break;
            default: 
                Debug.Log("You have typed an invalid eventName"); 
                emitter = bgm1; 
                break;
        }

        Debug.Log("Event chosen is: " + emitter);
        return emitter;
    }
    

     /*public void UnPackData(Component sender, object data)
    {
        // Check if the data is a string before proceeding
        if (data is AudioSettings aS)
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
        else
        {
            Debug.LogWarning("Data is not a valid scene name string.");
        }
    }*/
}
