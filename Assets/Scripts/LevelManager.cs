using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;



public class LevelLoader : MonoBehaviour
{
    public string filePrefix = "level_";
    public int numberOfLevels = 10;
    public Levels allLevels;

    void Awake() // Ensure levels are loaded as soon as the game starts
    {
        allLevels = LoadAllLevels();
        Debug.Log("Loaded " + allLevels.levels.Length + " levels");
    }

    Levels LoadAllLevels()
    {
        Levels levels = new Levels();
        levels.levels = new LevelData[numberOfLevels];

        for (int i = 1; i <= numberOfLevels; i++)
        {
            string fileName = filePrefix + i.ToString("D2");
            TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

            if (jsonFile != null)
            {
                LevelData level = JsonUtility.FromJson<LevelData>(jsonFile.text);
                levels.levels[i - 1] = level;
            }
            else
            {
                Debug.LogError("Cannot find file: " + fileName);
            }
        }

        return levels;
    }

    public LevelData GetLevel(int levelNumber)
    {
        Debug.Log(levelNumber);
        if (levelNumber >= 1 && levelNumber <= numberOfLevels)
        {
            return allLevels.levels[levelNumber - 1];
        }
        Debug.LogError("Level number out of range");
        return null;
    }

    public void RetryLevel()
    {
        Debug.Log("Button Clicked");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

[System.Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;
    public Goal[] goals;
}

[System.Serializable]
public class Goal
{
    public string goalType;
    public int count;

    public void DecrementGoal()
    {
        if (count > 0)
        {
            count--;
        }
    }
}

[System.Serializable]
public class Levels
{
    public LevelData[] levels;
}


