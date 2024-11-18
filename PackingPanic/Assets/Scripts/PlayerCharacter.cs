using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : BasicCharacter
{
    [SerializeField]
    private InputActionAsset _inputAsset;


    private InputAction _upAction;
    private InputAction _downAction;
    private InputAction _leftAction;
    private InputAction _rightAction;

    private float _movementMultiplier = 1f;
    // Speed variable to control the movement speed of the player

    
    protected override void Awake()
    {
        base.Awake();

        if (_inputAsset == null) return;

        _upAction = _inputAsset.FindActionMap("Gameplay").FindAction("Up");
        _downAction = _inputAsset.FindActionMap("Gameplay").FindAction("Down");
        _leftAction = _inputAsset.FindActionMap("Gameplay").FindAction("Left");
        _rightAction = _inputAsset.FindActionMap("Gameplay").FindAction("Right");
    }

    private void OnEnable()
    {
        if (_inputAsset == null) return;

        _inputAsset.Enable();
    }

    private void OnDisable()
    {
        if (_inputAsset == null) return;

        _inputAsset.Disable();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {

        Vector3 movementInput = Vector3.zero;

        // Check each action and accumulate the movement input
        if (_upAction.IsPressed())
        {
            movementInput += Vector3.forward; // Move forward
        }
        if (_downAction.IsPressed())
        {
            movementInput += Vector3.back; // Move backward
        }
        if (_leftAction.IsPressed())
        {
            movementInput += Vector3.left; // Move left
        }
        if (_rightAction.IsPressed())
        {
            movementInput += Vector3.right; // Move right
        }

        if (movementInput != Vector3.zero)
        {
            movementInput.Normalize();
        }

        _movementBehaviour.SetMovementMultiplier(_movementMultiplier);
        _movementBehaviour.Move(movementInput);
    }

    public void SetMultiplier(float multiplier)
    {
        _movementMultiplier = multiplier;
    }
}


