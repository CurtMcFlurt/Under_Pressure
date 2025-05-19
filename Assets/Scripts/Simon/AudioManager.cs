using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;

public enum TriggerAction
{
    None,
    Play,
    Stop,
    SetParameter,
    OneShot
}

public enum BackgroundMusicEvents
{
    None,
    AmbienceMusic,
    AmbienceSound,
    BoidsSound
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")] 
    [SerializeField] private EventReference[] bgmReferences = new EventReference[3];
    private EventInstance[] bgmInstances = new EventInstance[3];

    [Header("GameOver")] [SerializeField] private EventReference gameOverStinger;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private BoidFollowTarget bFT;
    private void Start()
    {
        bFT = GameObject.FindGameObjectWithTag("Player").GetComponent<BoidFollowTarget>();
    }

    public void PlayMusic(BackgroundMusicEvents bgmEvent, Vector3 pos)
    {
        int num = Convert.ToInt32(bgmEvent) - 1;

        if (num < 0)
        {
            Debug.Log("INVALID EVENT CHOSEN!");
            return;
        }

        bool isActive = CheckActiveState(bgmInstances[num]);

        if (!isActive)
        { 
            Debug.Log("Event Not Active before, Activating");
            bgmInstances[num] = RuntimeManager.CreateInstance(bgmReferences[num]); 
            bgmInstances[num].start(); 
        }

        
    }
    public void StopMusic(BackgroundMusicEvents bgmEvent, bool ignoreFadeOut)
    {
        int num = Convert.ToInt32(bgmEvent) - 1;

        if (num < 0)
        {
            Debug.Log("INVALID EVENT CHOSEN!");
            return;
        }
        
        if (ignoreFadeOut)
        {
            bgmInstances[num].stop(STOP_MODE.IMMEDIATE);
        }
        else
        {
            bgmInstances[num].stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        bgmInstances[num].release();
    }

    public void SetParameter(BackgroundMusicEvents bgmEvent, string paramName, float paramValue, bool ignoreSeek, bool globalParam)
    {
        if (globalParam)
        { 
            RuntimeManager.StudioSystem.setParameterByName(paramName, paramValue, ignoreSeek); 
            return;
        }
        
        int num = Convert.ToInt32(bgmEvent) - 1;

        if (num < 0)
        {
            Debug.Log("INVALID EVENT CHOSEN!");
            return;
        }

        bgmInstances[num].setParameterByName(paramName, paramValue, ignoreSeek);
    }

    private bool CheckActiveState(EventInstance eInstance)
    {
        bool isActive = true;

        eInstance.getPlaybackState(out PLAYBACK_STATE state);

        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
        {
            isActive = false;
        }
        return isActive;
    }

    public void PlayGameOver()
    {
        RuntimeManager.PlayOneShot(gameOverStinger);
    }
    
    void Update()
    {
        if (bFT.boidCounter > 0) 
        {
           SetParameter(BackgroundMusicEvents.BoidsSound, "Boids", bFT.boidCounter, false, false); 
           Debug.Log("Boid parameter is " + bFT.boidCounter);
        }
    }
    
    private int currentAmmount;
    public List<String> keys = new List<String>();
    

    public void UnPackData(Component sender, object data)
    {
        // Check if the data is a string before proceeding
        if (data is string keyName)
        {
            Debug.Log($"Scene change requested: {keyName}");

            // Trigger the scene change with the provided scene name
            if (!keys.Contains(keyName))
            {
                currentAmmount++;
                keys.Add(keyName);
                SetParameter(BackgroundMusicEvents.AmbienceMusic,"Progression",currentAmmount,true, false);
                Debug.Log( + currentAmmount);
            }
        }
        else
        {
            Debug.LogWarning("Data is not a valid scene name string.");
        }
    }
}
