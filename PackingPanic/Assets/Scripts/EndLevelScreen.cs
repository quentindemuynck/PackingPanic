using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndLevelScreen : MonoBehaviour
{
    [SerializeField]
    private Image[] _starImages;

    [SerializeField]
    private LevelManager _levelManager;

    [SerializeField]
    private TextMeshProUGUI _levelCompletedText;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private TextMeshProUGUI _coinsText;

    [SerializeField]
    private Button NextLevel;

    [SerializeField]
    private PauseScreen _pauseScreen;

    [SerializeField]
    private InputActionAsset _inputAsset;

    private float _scoreAmount = 0;
    private int _starsEarned = 0;
    private bool _levelCompleted = false;
    private int _coinsEarned = 0; // Coins earned (100 per star)

    private bool _updatedProgress = false;

    private void OpenScreen()
    {
        Time.timeScale = 0f;
        _inputAsset.FindActionMap("Gameplay").Disable();
        _inputAsset.FindActionMap("Chest").Disable();
        this.transform.GetChild(0).gameObject.SetActive(true);

        if(!_updatedProgress)
        {
            SavePlayerProgress();
            _updatedProgress = true;
        }

        UpdateStarImages();
        UpdateUI();
    }

    private void Update()
    {
        if (_levelManager != null && _levelManager.GetLevelEnded())
        {
            _scoreAmount = LevelManager.GetScore();
            _starsEarned = GetAmountOfStarsEarned();
            _coinsEarned = _starsEarned * 100; 
            CheckLevelCompleted();
            OpenScreen();
        }
    }

    private int GetAmountOfStarsEarned()
    {
        if (_levelManager == null) return 0;

        float maxScore = _levelManager.GetMaxScore();
        float scorePercentage = (_scoreAmount / maxScore) * 100;
        Debug.Log(maxScore + " " + scorePercentage + " " + _scoreAmount);
        int starsEarned = 0;

        if (scorePercentage >= _levelManager.GetStarRequirement(0))
            starsEarned = 1;
        if (scorePercentage >= _levelManager.GetStarRequirement(1))
            starsEarned = 2;
        if (scorePercentage >= _levelManager.GetStarRequirement(2))
            starsEarned = 3;

        return starsEarned;
    }

    private void CheckLevelCompleted()
    {
        _levelCompleted = _starsEarned > 0; 
    }

    private void UpdateStarImages()
    {
        for (int i = 0; i < _starImages.Length; i++)
        {
            if (i < _starsEarned && _starImages[i] != null)
            {
                
                _starImages[i].color = new Color(1, 1, 1, 1); 
            }
        }
    }

    private void UpdateUI()
    {
        if (_scoreText == null || _coinsText == null) return;
        _scoreText.text = $"Score: {(int)_scoreAmount}";
        _coinsText.text = $"Coins: {_coinsEarned}";
        if (_levelCompletedText != null)
        {
            if (_levelCompleted)
            {
                _levelCompletedText.text = "Level Completed!";
                NextLevel.GetComponentInChildren<TextMeshProUGUI>().text = "Next Level";
                NextLevel.onClick.RemoveAllListeners();
                NextLevel.onClick.AddListener(() => _pauseScreen.GoToNextLevel());
            }
            else
            {
                _levelCompletedText.text = "Level Failed!";
                NextLevel.GetComponentInChildren<TextMeshProUGUI>().text = "Try Again";
                NextLevel.onClick.RemoveAllListeners();
                NextLevel.onClick.AddListener(() => _pauseScreen.RestartLevel());
            }
        }
    }

    private void SavePlayerProgress()
    {
        int levelIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex - 1;

        // Load current progress
        PlayerProgress progress = SaveLoadManager.LoadProgress();

        
        Debug.Log("Before Save - Total Coins: " + progress.totalCoins);
        Debug.Log("Before Save - Stars for Level " + levelIndex + ": " + progress.GetStarsForLevel(levelIndex));

        // Update the level stars if needed
        if (progress.GetStarsForLevel(levelIndex) < _starsEarned)
        {
            progress.SetStarsForLevel(levelIndex, _starsEarned);
            Debug.Log("Adding stars");
        }

        progress.totalCoins += _coinsEarned;

        
        Debug.Log("After Save - Total Coins: " + progress.totalCoins);
        Debug.Log("After Save - Stars for Level " + levelIndex + ": " + progress.GetStarsForLevel(levelIndex));

        // Save progress back to the file
        SaveLoadManager.SaveProgress(progress);
    }



}
