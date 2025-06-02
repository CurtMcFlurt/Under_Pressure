using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionListUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject sessionButtonPrefab; // Prefab with Button + TMP_Text
    public Transform sessionListParent;    // Must have VerticalLayoutGroup
    public TMP_InputField joinCodeInputField;

    private List<GameObject> currentSessionButtons = new();

    /// <summary>
    /// Clears existing buttons and creates new ones based on available sessions.
    /// </summary>
    /// <param name="sessionNames">List of available session names.</param>
    public void PopulateSessionList(List<string> sessionNames)
    {
        ClearSessionList();

        foreach (string sessionName in sessionNames)
        {
            GameObject buttonObj = Instantiate(sessionButtonPrefab, sessionListParent);
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            Button button = buttonObj.GetComponent<Button>();

            if (buttonText != null)
                buttonText.text = sessionName;

            if (button != null)
                button.onClick.AddListener(() => OnSessionButtonClicked(sessionName));

            currentSessionButtons.Add(buttonObj);
        }
    }

    /// <summary>
    /// Clears all dynamically created session buttons.
    /// </summary>
    private void ClearSessionList()
    {
        foreach (GameObject button in currentSessionButtons)
        {
            Destroy(button);
        }

        currentSessionButtons.Clear();
    }

    /// <summary>
    /// Called when a session button is clicked.
    /// </summary>
    /// <param name="sessionName">Session name to populate in the input field.</param>
    private void OnSessionButtonClicked(string sessionName)
    {
        if (joinCodeInputField != null)
        {
            joinCodeInputField.text = sessionName;
        }
    }
}
