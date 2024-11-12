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
        PlayerPrefs.SetInt("CurrentLevel", 1);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
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
        // Load the scene corresponding to the selected level
        SceneManager.LoadScene("Level " + levelIndex);
    }

    public void CompleteLevel(int levelIndex)
    {
        Debug.Log("Completed Level " + levelIndex);
        // Disable the current button and enable the next one
        if (levelIndex <= levelButtons.Length)
        {
            levelButtons[levelIndex - 1].gameObject.SetActive(false);
        }
        if (levelIndex < levelButtons.Length)
        {
            levelButtons[levelIndex].gameObject.SetActive(true);
        }
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        UpdateLevelButtons();
    }
}
