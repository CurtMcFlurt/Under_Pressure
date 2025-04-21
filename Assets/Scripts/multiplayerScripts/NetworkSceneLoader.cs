using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkSceneLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load")]
    public string sceneToLoad;

    [Tooltip("Optional callback after scene load starts")]
    public UnityEvent onSceneLoadTriggered;

    // Can be called by a button
    public void TriggerSceneChange()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
                onSceneLoadTriggered?.Invoke();
            }
            else
            {
                Debug.LogWarning("Scene name not set in NetworkSceneLoader.");
            }
        }
        else
        {
            Debug.LogWarning("Only the host/server can trigger a scene change.");
        }
    }

    // Optional: Allow calling this with a custom scene name
    public void TriggerSceneChange(string customSceneName)
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(customSceneName, LoadSceneMode.Single);
            onSceneLoadTriggered?.Invoke();
        }
    }
}
