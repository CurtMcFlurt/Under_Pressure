using UnityEngine;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    public StudioEventEmitter bgm1, bgm2, bgmChase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayMusic(string eventName)
    {
        Debug.Log("Entering playMusic");
        StudioEventEmitter playEvent = ChooseEvent(eventName);
        Debug.Log("Play event is " + playEvent);

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
            case "bgmCase": 
                emitter = bgmChase; 
                break;
            default: 
                Debug.Log("You have typed an invalid eventName"); 
                emitter = bgm1; 
                break;
        }

        Debug.Log("Event chosen is: " + emitter);
        return emitter;
    }
}
