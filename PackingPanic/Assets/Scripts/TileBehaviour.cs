using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileBehaviour : InteractableObject
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private InputActionAsset _inputAsset;

    private InputAction _interactAction;
    private InputAction _dropAction;

    [SerializeField]
    private GameObject _player;

    private bool _isHolding = false;
    private bool _isStored = false;

    private void Awake()
    {
        if (_inputAsset == null) return;

        _interactAction = _inputAsset.FindActionMap("Gameplay").FindAction("Interact");
        _dropAction = _inputAsset.FindActionMap("Gameplay").FindAction("Drop");
    }

    private void OnEnable()
    {
        if (_inputAsset == null) return;

        _inputAsset.Enable();
        _interactAction.performed += OnInteract; 
    }

    private void OnDisable()
    {
        if (_inputAsset == null) return;

        _interactAction.performed -= OnInteract; 
        _inputAsset.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if ((_player.transform.position - this.transform.position).magnitude < 2 && !_isStored && isHovered && !_isHolding)
        {
            _isHolding = true;
        }
        else if(isHovered && _isHolding)
        {
            _isHolding = false;
        }
    }

    private void Update()
    {
        Hold();
        Drop();
    }

    private void Hold()
    {
        if (rb == null) return;

        
        if (_isHolding)
        {
            if (this.transform.parent == null)
            {
                this.transform.SetParent(_player.transform);
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }

            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, Vector3.forward + (Vector3.up * 0.30f), Time.deltaTime * 5f);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
        else if (!_isHolding && this.transform.parent == _player.transform)
        {
            this.transform.SetParent(null);

            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
        }
    }

    private void Drop()
    {
        if(_dropAction.IsPressed())
        {
            _isHolding = false;
        }
    }

}
