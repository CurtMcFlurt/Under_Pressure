using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public enum TriggerAction
{
    None,
    Play,
    Stop,
    SetParameter
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    public MusicManager musicManager;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void PlayGameOver()
    {
        RuntimeManager.PlayOneShot(gameOverStinger);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
