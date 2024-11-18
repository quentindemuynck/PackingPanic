using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    [SerializeField]
    private Canvas _mainScreen;
    [SerializeField]
    private Canvas _levelSelect;
    [SerializeField]
    private Canvas _shop;
    [SerializeField]
    private Canvas _controls;

    [SerializeField]
    private TextMeshProUGUI _controlsText;

    [SerializeField]
    private InputActionAsset _inputAsset;

    [SerializeField]
    private TextMeshProUGUI _coinsDisplayText;

    [SerializeField]
    private GameObject[] _starPanels;

    private InputAction _backAction;

    // Stack to keep track of canvas navigation history
    private Stack<Canvas> _canvasStack = new Stack<Canvas>();

    private PlayerProgress _playerProgress;

    [SerializeField]
    private TextMeshProUGUI _stockDisplayText;

    private void Awake()
    {
        _inputAsset.FindActionMap("StartScreen").Enable();
        _canvasStack.Push(_mainScreen);

        if (_inputAsset != null)
        {
            
            _backAction = _inputAsset.FindActionMap("StartScreen").FindAction("Back");
            _backAction.performed += HandleBackAction;
        }
    }

    private void Start()
    {
        if(_controlsText != null)
            UpdateControlsText();

        _playerProgress = SaveLoadManager.LoadProgress();

        UpdateCoinsDisplay();
        UpdateStarPanels();
        UpdateStockDisplay();
    }

    private void OnDestroy()
    {
        
        if (_backAction != null)
        {
            _backAction.performed -= HandleBackAction;
        }
    }

    private void HandleBackAction(InputAction.CallbackContext context)
    {
        BackAction();
    }

    public void BackAction()
    {
        // Check if there’s more than one canvas in the stack to go back to
        if (_canvasStack.Count > 1)
        {
            Canvas currentCanvas = _canvasStack.Pop();
            currentCanvas.gameObject.SetActive(false);
            _canvasStack.Peek().gameObject.SetActive(true);
        }
    }

    // General method to show a specific canvas and update the navigation stack
    private void ShowCanvas(Canvas canvasToShow)
    {
        if (canvasToShow == null) return;

        _canvasStack.Peek().gameObject.SetActive(false);

        canvasToShow.gameObject.SetActive(true);
        _canvasStack.Push(canvasToShow);
    }

    public void OpenLevelSelect()
    {
        ShowCanvas(_levelSelect);
    }

    public void OpenShop()
    {
        ShowCanvas(_shop);
    }

    public void OpenControls()
    {
        ShowCanvas(_controls);
    }

    public void ExitGame() 
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Stop playing in the Editor
        #else
            Application.Quit(); // Quit the application
        #endif
    }

    public void SelectLevel(int levelIndex)
    {
        _inputAsset.FindActionMap("Gameplay").Enable();
        _inputAsset.FindActionMap("Chest").Enable();
        SceneManager.LoadSceneAsync(levelIndex + 1);
    }

    private void UpdateControlsText()
    {
        string controlsList = "Controls:\n";

        foreach (var actionMap in _inputAsset.actionMaps)
        {
            controlsList += $"\n{actionMap.name}:\n"; // Action Map Name (e.g., Player, UI)

            foreach (var action in actionMap.actions)
            {
                controlsList += $"{action.name}: "; // Action Name (e.g., Jump, Move)

                // Collect all non-composite bindings for this action
                List<string> bindingsList = new List<string>();

                foreach (var binding in action.bindings)
                {
                    if (binding.isComposite) continue; // Skip composite bindings

                    // Clean up the binding path (e.g., remove "<Keyboard>/")
                    string bindingName = binding.path;
                    if (bindingName.Contains("/"))
                    {
                        bindingName = bindingName.Substring(bindingName.LastIndexOf("/") + 1);
                    }

                    bindingsList.Add(bindingName);
                }

                // Combine all bindings for the action into a single line
                controlsList += string.Join(", ", bindingsList) + "\n";
            }
        }

        _controlsText.text = controlsList;
    }

    private void UpdateCoinsDisplay()
    {
        if (_coinsDisplayText != null && _playerProgress != null)
        {
            _coinsDisplayText.text = $"Coins: {_playerProgress.totalCoins}";
        }
    }

    private void UpdateStarPanels()
    {
        if (_starPanels == null || _playerProgress == null) return;

        for (int i = 0; i < _starPanels.Length; i++)
        {
            GameObject panel = _starPanels[i];
            if (panel == null) continue;

            int levelIndex = i; // Match panel index to level index
            int starsEarned = _playerProgress.GetStarsForLevel(levelIndex);

            Transform[] starImages = new Transform[panel.transform.childCount];

            // Collect all the star image transforms from the panel
            for (int j = 0; j < panel.transform.childCount; j++)
            {
                starImages[j] = panel.transform.GetChild(j);
            }

            // Update the visibility of the stars for this panel
            for (int j = 0; j < starImages.Length; j++)
            {
                Image starImage = starImages[j].GetComponent<Image>();
                if (starImage != null)
                {
                    // Invert the index: the last child (index 0) should be the first star
                    int invertedIndex = starImages.Length - 1 - j;

                    bool shouldShowStar = invertedIndex < starsEarned;
                    if (shouldShowStar)
                    {
                        // Show the star as white and fully opaque
                        starImage.color = new Color(1, 1, 1, 1);
                    }
                }
            }
        }
    }

    public void BuySpeedBoost(int cost)
    {
        if (_playerProgress.totalCoins >= cost)
        {
            _playerProgress.totalCoins -= cost;
            _playerProgress.amountOfSpeedBoosts++;
            SaveLoadManager.SaveProgress(_playerProgress);
            UpdateCoinsDisplay();
            UpdateStockDisplay();
            Debug.Log("Speed boost purchased!");
        }
        else
        {
            Debug.Log("Not enough coins to buy a speed boost.");
        }
    }

    public void BuyTimeslow(int cost)
    {
        if (_playerProgress.totalCoins >= cost)
        {
            _playerProgress.totalCoins -= cost;
            _playerProgress.amountOfTimeSlows++;
            SaveLoadManager.SaveProgress(_playerProgress);
            UpdateCoinsDisplay();
            UpdateStockDisplay();
            Debug.Log("Time slow purchased!");
        }
        else
        {
            Debug.Log("Not enough coins to buy a time slow.");
        }
    }

    private void UpdateStockDisplay()
    {
        if (_stockDisplayText != null && _playerProgress != null)
        {
            _stockDisplayText.text = $"Speedboosts: {_playerProgress.amountOfSpeedBoosts}\n" +
                                     $"Timeslows: {_playerProgress.amountOfTimeSlows}";
        }
    }

}
