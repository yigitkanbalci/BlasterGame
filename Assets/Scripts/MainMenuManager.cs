using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public Button[] levelButtons;   // Array of level buttons
    public Button finishedButton;   // Button to show when all levels are completed
    private int currentLevel;       // Tracks the current level

    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.SetInt("CurrentLevel", 1);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        Debug.Log("CurrentLevel in MainMenu: " + currentLevel);
        if (currentLevel > 10)
        {
            ResetProgress();
        }
        UpdateLevelButtons();
    }

    void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            var buttonText = levelButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText == null)
            {
                Debug.LogError("No TMP_Text component found in Level Button " + i);
                continue;
            }
            if (i < currentLevel)
            {
                levelButtons[i].gameObject.SetActive(true);
                buttonText.text = "Level " + (i + 1);
                int levelIndex = i + 1; // Capture the correct level index for the listener
                levelButtons[i].onClick.RemoveAllListeners(); // Remove any existing listeners to avoid duplication
                levelButtons[i].onClick.AddListener(() => StartLevel(levelIndex));
            }
            else
            {
                levelButtons[i].gameObject.SetActive(false);
            }
        }
        finishedButton.gameObject.SetActive(currentLevel > levelButtons.Length);
    }

    public void StartLevel(int levelIndex)
    {
        Debug.Log("Starting Level " + levelIndex);

        // Check if the scene exists
        string sceneName = "Level 1";
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName); // Load the selected level scene
        }
        else
        {
            Debug.LogError("Scene " + sceneName + " does not exist or is not added to Build Settings.");
        }
    }

    public void CompleteLevel(int levelIndex)
    {
        Debug.Log("Completed Level " + levelIndex);

        if (levelIndex < levelButtons.Length)
        {
            levelButtons[levelIndex].gameObject.SetActive(false);
            currentLevel = Mathf.Max(currentLevel, levelIndex + 1); // Increment level only if higher
            PlayerPrefs.SetInt("CurrentLevel", currentLevel);
            UpdateLevelButtons();
        }
        else
        {
            Debug.Log("All levels completed!");
            finishedButton.gameObject.SetActive(true);
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        Debug.Log("Progress reset to Level 1");
        UpdateLevelButtons();
    }
}
