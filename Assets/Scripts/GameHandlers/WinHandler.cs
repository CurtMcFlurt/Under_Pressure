using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WinHandler : MonoBehaviour
{
    public int ammountToWin = 4;
    private int currentAmmount;
    public List<String> keys = new List<String>();
    public string sceneChangeName;
    public GameEvent sceneChange;

   

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
            }
        }
        else
        {
            Debug.LogWarning("Data is not a valid scene name string.");
        }

        if (currentAmmount >= ammountToWin)
        {
            currentAmmount = 0;
            keys.Clear();
            sceneChange.Raise(this, sceneChangeName);
        }
    }

}
