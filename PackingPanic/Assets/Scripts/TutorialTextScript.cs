using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialTextScript : MonoBehaviour
{
    [SerializeField]
    private Canvas _tutorialCanvas; // Assign your tutorial Canvas here

    private int _currentStep = 0; 

    // Names of the child GameObjects corresponding to tutorial steps
    [SerializeField]
    private string[] _stepNames;

    private void Start()
    {
        ShowStep(_currentStep);
    }

    private void Update()
    {
        if (_stepNames != null && _tutorialCanvas != null)
        { 
            if (TileBehaviour.GetAmountHolding() > 0 && _currentStep == 0)
            {
                AdvanceStep();
            }

            foreach (TileBehaviour tile in TileBehaviour.AllTiles)
            {
                if( tile.GetIsStored() &&  _currentStep == 1)
                {
                    AdvanceStep();
                }
            }

            if(_currentStep == 1 && TileBehaviour.GetAmountHolding() < 0)
            {
                PreviousStep();
            }
        }
    }

    private void AdvanceStep()
    {
        if (_currentStep < _stepNames.Length - 1)
        {
            _currentStep++;
            ShowStep(_currentStep);
        }
        else
        {
            Debug.Log("Tutorial complete!");
            HideAllSteps(); 
        }
    }

    private void PreviousStep()
    {
        if (_currentStep > 0)
        {
            _currentStep--;
            ShowStep(_currentStep);
        }
    }

    private void ShowStep(int stepIndex)
    {
        HideAllSteps(); 

        Transform stepTransform = _tutorialCanvas.transform.Find(_stepNames[stepIndex]);
        if (stepTransform != null)
        {
            stepTransform.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Step with name '{_stepNames[stepIndex]}' not found!");
        }
    }

    private void HideAllSteps()
    {
        foreach (string stepName in _stepNames)
        {
            Transform stepTransform = _tutorialCanvas.transform.Find(stepName);
            if (stepTransform != null)
            {
                stepTransform.gameObject.SetActive(false);
            }
        }
    }
}
