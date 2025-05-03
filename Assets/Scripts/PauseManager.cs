using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public bool IsPaused;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IsPaused)
                ClosePauseMenu();
            else
                OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;  // Pause all time-based activity (animations, physics, etc.)
        IsPaused = true;
    }
    public void ClosePauseMenu()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;  // Resume normal gameplay
        IsPaused = false;
                              
      
    }  
}
