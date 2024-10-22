using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChestBehaviour : InteractableObject
{
    [SerializeField]
    private GameObject _player;
    private GameObject inventoryPanel;

    [SerializeField]
    private InputActionAsset _inputAsset;

    private InputAction _interactAction;

    private bool isInventoryOpen = false;

    private void Awake()
    {
        if (_inputAsset == null) return;
        _interactAction = _inputAsset.FindActionMap("Gameplay").FindAction("Interact");

        // Subscribe to the action's performed event
        _interactAction.performed += OnInteract;
    }

    void Start()
    {
        inventoryPanel = transform.Find("Inventory").gameObject;
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
    }

    // Unsubscribe from the event when the object is destroyed
    private void OnDestroy()
    {
        _interactAction.performed -= OnInteract;
    }

    private void Update()
    {
        if (inventoryPanel == null) return;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // Check which control triggered the action
        if ((_player.transform.position - this.transform.position).magnitude < 2)
        {
            // If it's the E key on the keyboard
            if (context.control.device is Keyboard && context.control.name == "e")
            {
                if (isInventoryOpen)
                {
                    CloseChest();
                }
                else if (isHovered)
                {
                    OpenChest();
                }
            }
            // If it's a mouse button, we can ignore or do something else
            else if (context.control.device is Mouse)
            {
                if (!isInventoryOpen && isHovered)
                {
                    OpenChest();
                }
            }
        }
    }

    private void OpenChest()
    {
        if (!isInventoryOpen)
        {
            inventoryPanel?.SetActive(true);
            isInventoryOpen = true;
        }

    }

    private void CloseChest()
    {
        if (isInventoryOpen)
        {
            inventoryPanel?.SetActive(false);
            isInventoryOpen = false;
        }
    }
}