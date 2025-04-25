using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneChangeManager : NetworkBehaviour
{
    [Tooltip("Optional: Automatically switch to this scene on call")]
    public string sceneToLoad;

 

    /// <summary>
    /// Called by a UI Button or event system to request a scene load.
    /// Can be passed a specific scene name OR uses the default one.
    /// </summary>
    
    public void RequestSceneChange(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = sceneToLoad;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name not set in SceneChangeManager.");
            return;
        }
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

   

    public void UnPackData(Component sender, object data)
    {
        // Check if the data is a string before proceeding
        if (data is string sceneName)
        {
            Debug.Log($"Scene change requested: {sceneName}");

            // Trigger the scene change with the provided scene name
            RequestSceneChange(sceneName);
        }
        else
        {
            Debug.LogWarning("Data is not a valid scene name string.");
        }
    }
}
