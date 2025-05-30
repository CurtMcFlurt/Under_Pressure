using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;

// Enum Trigger Action skapar en lista som fungerar som en meny med flera alternativ som går att välja mellan. Dessa används för att bestämma vad som ska hända i ett FMOD event.
public enum TriggerAction
{
    None,
    Play,
    Stop,
    SetParameter,
    OneShot
}

// Enum Background Music Events skapar en lista som används för att bestämma vilket FMOD event som ska påverkas.
public enum BackgroundMusicEvents
{
    None,
    AmbienceMusic,
    AmbienceSound,
    BoidsSound
}
public class AudioManager : MonoBehaviour
{
    // Här skapar vi en enda global instans av AudioManager så att andra skript kan komma åt den
    public static AudioManager Instance;

    // Vi skapar referenser för att komma åt och lägga till FMOD event i inspektorn, och instanser som används för att spela ljud. 
    [Header("Background Music")] 
    [SerializeField] private EventReference[] bgmReferences = new EventReference[3];
    private EventInstance[] bgmInstances = new EventInstance[3];

    [Header("GameOver")] [SerializeField] private EventReference gameOverStinger;
    
    // I Awake ser vi till att det bara finns en Audiomanager i scenen och att den inte förstörs mellan scenbyten.
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
    
    // Vi hittar ett skript som heter BoidFollowTarget så att vi kan komma åt data från den längre ner i detta skript under "Update".
    private BoidFollowTarget bFT;
    private void Start()
    {
        bFT = GameObject.FindGameObjectWithTag("Player").GetComponent<BoidFollowTarget>();
    }

    // Play Music är en funktion som startar ett FMOD event så länge det inte redan är aktivt.
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
    
    // Stop Music stoppar ett FMOD event. Här finns möjligheten att låta musiken "fade out" eller ej.
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

    // Set Parameter ändrar valfri parameter i FMOD.
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

    // Check Active State används för att se om ett FMOD event är aktivt eller ej.
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
    
    // I Update tar vi information från ett annat skript som håller koll på hur många boids omringar spelaren. Antalet boids ändrar en parameter i FMOD.
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
    
    // UnPackData kallas när data skickas till AudioManager. I detta fall skickas information när spelare har löst ett pussel och "Progression" parametern i FMOD ökar beroende på "currentAmmount" (medveten om felstavning).
    public void UnPackData(Component sender, object data)
    {
        // Kollar om datan är en string innan det går vidare.
        if (data is string keyName)
        {
            Debug.Log($"Scene change requested: {keyName}");
            
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
