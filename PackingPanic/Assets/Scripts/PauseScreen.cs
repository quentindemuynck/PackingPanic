using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseScreen : MonoBehaviour
{

    public delegate void OnMenuOpened();
    public event OnMenuOpened OnMenuOpenedEvent;

    [SerializeField]
    private InputActionAsset _inputAsset;

    private InputAction _backAction;

    private bool isPaused = false;

    private void Awake()
    {
        _inputAsset.FindActionMap("StartScreen").Enable();

        if (_inputAsset != null)
        {
            _backAction = _inputAsset.FindActionMap("StartScreen").FindAction("Back");

        }
    }
    void Update()
    {
        if (_backAction.WasReleasedThisFrame()) 
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        OnMenuOpenedEvent?.Invoke();

        _inputAsset.FindActionMap("Chest").Disable();
        _inputAsset.FindActionMap("Gameplay").Disable();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(true);
        }

        Time.timeScale = 0f; // Freezes the game
    }

    public void ResumeGame()
    {


        _inputAsset.FindActionMap("Chest").Enable();
        _inputAsset.FindActionMap("Gameplay").Enable();

        isPaused = false;
        Time.timeScale = 1f; // Resumes the game

        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false; // Stop playing in the Editor
        #else
                    Application.Quit(); // Quit the application
        #endif
        
    }

    public void RestartLevel()
    {
        _inputAsset.FindActionMap("Chest").Enable();
        _inputAsset.FindActionMap("Gameplay").Enable();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenMainMenu()
    {
        _inputAsset.FindActionMap("Chest").Enable();
        _inputAsset.FindActionMap("Gameplay").Enable();
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
    }

    public void GoToNextLevel()
    {
        _inputAsset.FindActionMap("Chest").Enable();
        _inputAsset.FindActionMap("Gameplay").Enable();
        Time.timeScale = 1f; 

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No more levels to load. Ensure the next level is added to the Build Settings.");
            SceneManager.LoadScene(0);
        }
    }
}
