using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioOneShotSpawner : MonoBehaviour
{

    private StudioEventEmitter emitter;
    
    void Start()
    {
        emitter = GetComponent<StudioEventEmitter>();
        if (emitter != null)
        {
            emitter.Play();
            StartCoroutine(WaitToDespawn());
        }
        else
        {
            Debug.LogWarning("No FMOD StudioEventEmitter attached to the object");
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator WaitToDespawn()
    {
        EventInstance instance = emitter.EventInstance;

        bool isPlaying = true;
        while (isPlaying)
        {
            instance.getPlaybackState(out PLAYBACK_STATE state);
            isPlaying = state != PLAYBACK_STATE.STOPPED;
            yield return null;
        }

        Destroy(gameObject);
    }
}
