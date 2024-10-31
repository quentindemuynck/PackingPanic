using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PackingPanick.TileData;

public class ChestBehaviour : InteractableObject
{
    [SerializeField]
    private GameObject tilePreviewImage;

    [SerializeField]
    private GameObject _camera;

    [SerializeField]
    private GameObject _player;
    private GameObject inventoryPanel;

    [SerializeField]
    private InputActionAsset _inputAsset;

    private InputAction _interactAction;
    private InputAction _rotateAction; // New input action for rotation
    private bool isInventoryOpen = false;

    private GameObject[] gridSlots;
    private int slotCount;

    private Dictionary<GameObject, Color> originalSlotColors = new Dictionary<GameObject, Color>();

    private float rotationAngle = 0f; // To keep track of the rotation

    private void Awake()
    {
        if (_inputAsset == null) return;
        _interactAction = _inputAsset.FindActionMap("Gameplay").FindAction("Interact");
        _interactAction.performed += OnInteract;

        // Initialize the rotation action
        _rotateAction = _inputAsset.FindActionMap("Gameplay").FindAction("Rotate");
        _rotateAction.performed += OnRotate; // Attach the rotation method
    }

    void Start()
    {
        inventoryPanel = transform.Find("Inventory").gameObject;
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);

        slotCount = this.transform.GetChild(0).GetChild(0).childCount;
        gridSlots = new GameObject[slotCount];

        for (int index = 0; index < slotCount; index++)
        {
            gridSlots[index] = this.transform.GetChild(0).GetChild(0).GetChild(index).gameObject;

            Image slotImage = gridSlots[index].GetComponent<Image>();
            if (slotImage != null)
            {
                originalSlotColors[gridSlots[index]] = slotImage.color;
            }

            AddEventTriggers(gridSlots[index]);
        }
    }

    private void AddEventTriggers(GameObject slot)
    {
        EventTrigger eventTrigger = slot.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = slot.AddComponent<EventTrigger>();
        }

        // Pointer Enter event (mouse over)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => { OnMouseOverSlot(slot); });
        eventTrigger.triggers.Add(pointerEnter);

        // Pointer Exit event (mouse exit)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => { OnMouseExitSlot(slot); });
        eventTrigger.triggers.Add(pointerExit);
    }

    private void OnDestroy()
    {
        _interactAction.performed -= OnInteract;
        _rotateAction.performed -= OnRotate; // Detach the rotation method
    }

    private void Update()
    {
        if (GetIsOpened())
        {
            DisplayHoldingTile();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if ((_player.transform.position - this.transform.position).magnitude < 2)
        {
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
            else if (context.control.device is Mouse)
            {
                if (!isInventoryOpen && isHovered)
                {
                    OpenChest();
                }
            }
        }
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        // Increment rotation angle by 90 degrees on each rotation
        rotationAngle += 90f; // or -90f for counter-clockwise
        rotationAngle %= 360f; // Keep the angle within 0-359 degrees
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

            // Reset slot colors
            for (int index = 0; index < slotCount; index++)
            {
                ResetSlotImage(gridSlots[index]);
            }
        }
    }

    // Hover effect: Add a semi-transparent overlay to the slot's color
    public void OnMouseOverSlot(GameObject slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            Color overlayColor = new Color(0f, 0.5f, 0f, 0.3f);
            slotImage.color = originalSlotColors[slot] + overlayColor;
        }
    }

    public void OnMouseExitSlot(GameObject slot)
    {
        ResetSlotImage(slot);
    }

    private void ResetSlotImage(GameObject slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null && originalSlotColors.ContainsKey(slot))
        {
            slotImage.color = originalSlotColors[slot];
        }
    }

    public bool GetIsOpened()
    {
        return isInventoryOpen;
    }

    private TileBehaviour GetHoldingTile()
    {
        List<TileBehaviour> tiles = TileBehaviour.AllTiles;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].GetIsHolding()) return tiles[i];
        }

        return null;
    }

    private void DisplayHoldingTile()
    {
        TileBehaviour tile = GetHoldingTile();

        if (tile == null)
        {
            tilePreviewImage.SetActive(false);
            return;
        }

        TileData tileData = tile.GetTileData();
        UpdateTilePreviewImage(tileData);

        // Get mouse position in screen space
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        PositionTilePreview(mousePosition);

        tilePreviewImage.SetActive(true);
    }

    private void UpdateTilePreviewImage(TileData tileData)
    {
        Image previewImage = tilePreviewImage.GetComponent<Image>();
        if (previewImage != null)
        {
            previewImage.color = tileData.color;
            TileShape shape = TileShapeLibrary.GetShape(tileData.shape.name);
            previewImage.sprite = TileSpriteGenerator.CreateTileSprite(shape, tileData.color);
            previewImage.rectTransform.localScale = new Vector3(3, 3, 1);
        }

        // Rotate the preview image based on the rotation angle
        previewImage.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }

    private void PositionTilePreview(Vector2 mousePosition)
    {
        // Position the preview image at the mouse position in screen space
        RectTransform rectTransform = tilePreviewImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = mousePosition; // Set position directly to mouse position
        }
    }

    private Sprite GetTileSprite(TileShape shape, Color color)
    {
        return TileSpriteGenerator.CreateTileSprite(shape, color);
    }
}
