using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour
{
    public void QuitLevel()
    {
        Debug.Log("Clicked quit button.");
        SceneManager.LoadScene("MainMenu");
    }
}
