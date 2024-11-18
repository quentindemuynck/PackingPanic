using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PackingPanick.TileData;
using UnityEditor;

public class ChestBehaviour : InteractableObject
{
    [SerializeField] 
    private GameObject _menu;

    private PauseScreen _screen;

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
    private InputAction _rotateAction;
    private InputAction _closeAction;

    private bool isInventoryOpen = false;

    private GameObject[] gridSlots;
    private int slotCount;

    private Dictionary<GameObject, Color> originalSlotColors = new Dictionary<GameObject, Color>();

    private int middleSlotIndex = -2;

    private const int gridWidth = 8;
    private const int gridHeight = 8;


    private void Awake()
    {
        if (_menu != null)
        _screen = _menu.GetComponent<PauseScreen>();

        if (_screen != null)
            _screen.OnMenuOpenedEvent += CloseChest;

        if (_inputAsset == null) return;


        _interactAction = _inputAsset.FindActionMap("Chest").FindAction("Interact");
        _closeAction = _inputAsset.FindActionMap("Chest").FindAction("Close");

        _interactAction.performed += OnInteract;
        _closeAction.performed += OnClose;


        _rotateAction = _inputAsset.FindActionMap("Chest").FindAction("Rotate");
        _rotateAction.canceled += OnRotateReleased;
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
            GridSlot gridSlot = gridSlots[index].GetComponent<GridSlot>();
            gridSlot.SetSlotIndex(index);
            AddEventTriggers(gridSlots[index]);
            Debug.Log("added slot");
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
        _closeAction.performed -= OnClose;   // Detach the close method
    }

    private void Update()
    {
        if (GetIsOpened())
        {
            DisplayHoldingTile();
        }

        if (isInventoryOpen)
        {
            _inputAsset.FindActionMap("Gameplay", true).Disable();
        }
    }


    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!GetIsOpened() && (this.transform.position - _player.transform.position).magnitude < 3 && isHovered)
        {
            OpenChest();
        }
        else if (GetIsOpened())
        {
            if (middleSlotIndex == -2)
            {
                CloseChest();
            }

            TileBehaviour displayHoldingTile = GetHoldingTile();
            if (displayHoldingTile == null && middleSlotIndex != -2)
            {
                GridSlot gridSlot = gridSlots[middleSlotIndex].GetComponent<GridSlot>();
                if (gridSlot != null)
                {
                    if( gridSlot.GetHoldingTile() != null)
                    {
                        PickStoredTile(gridSlot);
                    }
                }
            }
            else
            {
                if (CheckValidStoring(displayHoldingTile, middleSlotIndex))
                {
                    StoreTile(displayHoldingTile, middleSlotIndex);
                    displayHoldingTile.Store(true);
                }
            }
        }
    }

    private void OnClose(InputAction.CallbackContext context)
    {
        if (GetIsOpened())
        {
            CloseChest();
        }
    }

    private void OnRotateReleased(InputAction.CallbackContext context)
    {
        if (GetIsOpened() && GetHoldingTile() != null)
        {
            Rotate();

        }
    }


    void OpenChest()
    {
        middleSlotIndex = -2;
        if (!isInventoryOpen)
        {
            inventoryPanel?.SetActive(true);
            isInventoryOpen = true;

            // Disables movement while in a chest
            _inputAsset.FindActionMap("Gameplay", true).Disable();
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

            tilePreviewImage?.SetActive(false);

            // Re-enable gameplay actions
            _inputAsset.FindActionMap("Gameplay", true).Enable();
        }
    }

    // Hover effect: Add a semi-transparent overlay to the slot's color
    public void OnMouseOverSlot(GameObject slot)
    {
        Debug.Log("Mouse over slot");

        GridSlot gridSlot = slot.GetComponent<GridSlot>();

        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            Color overlayColor = new Color(0f, 0.5f, 0f, 0.3f);
            slotImage.color = originalSlotColors[slot] + overlayColor;

            if (gridSlot != null)
            {
                HighlightTilesInMiddleSlot(gridSlot.GetIndex());

                middleSlotIndex = gridSlot.GetIndex();
            }
        }
    }

    public void OnMouseExitSlot(GameObject slot)
    {
        GridSlot hoveredSlot = slot.GetComponent<GridSlot>();
        if (hoveredSlot != null)
        {

             HighlightTilesInMiddleSlot(hoveredSlot.GetIndex(), false); 
             ResetSlotImage(slot);
            middleSlotIndex = -2;
        }
    }


    private void HighlightTilesInMiddleSlot(int slotIndex, bool highlight = true)
    {
        GridSlot hoveredSlot = gridSlots[slotIndex].GetComponent<GridSlot>();
        if (highlight)
        {
            hoveredSlot.HighlightSlot(true);
        }
        else
        {
            hoveredSlot.HighlightSlot(false);
        }

        
        for (int index = 0; index < gridSlots.Length; index++)
        {
            GridSlot gridSlot = gridSlots[index].GetComponent<GridSlot>();
            if (gridSlot != null && gridSlot.GetMiddleSlot() == hoveredSlot.GetMiddleSlot() && hoveredSlot.GetMiddleSlot() != -1)
            {
                if (highlight)
                {
                    gridSlot.HighlightSlot(true);
                }
                else
                {
                    gridSlot.HighlightSlot(false);
                }
            }
        }
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
            TileShape shape = TileShapeLibrary.GetShape(tileData.shape.name);
            previewImage.sprite = TileSpriteGenerator.CreateTileSprite(shape, tileData.color);
            previewImage.rectTransform.localScale = new Vector3(3, 3, 1);
        }
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

    private void Rotate()
    {
        TileBehaviour currentTile = GetHoldingTile();

        // Rotates the tile in te data
        Debug.Log("Occupied Cells after Rotation:");
        foreach (var cell in currentTile.GetTileData().shape.occupiedCells)
        {
            Debug.Log($"Cell Position: ({cell.x}, {cell.y})");
        }

        if (currentTile != null)
        {
            currentTile.GetTileData().shape.Rotate();
            UpdateTilePreviewImage(currentTile.GetTileData());

        }
    }

    private void StoreTile(TileBehaviour tile, int middleSlotIndex)
    {
        var occupiedCells = tile.GetTileData().shape.occupiedCells;

        int middleX = middleSlotIndex % gridWidth;
        int middleY = middleSlotIndex / gridWidth;

        foreach (var cell in occupiedCells)
        {
            int gridX = middleX + cell.x - 1;
            int gridY = middleY + cell.y - 1;

            if (IsWithinGridBounds(gridX, gridY))
            {
                int slotIndex = gridX + gridY * gridWidth;
                GridSlot gridSlot = gridSlots[slotIndex].GetComponent<GridSlot>();

                // Only store tile if the slot is empty
                if (gridSlot != null && gridSlot.GetHoldingTile() == null)
                {
                    gridSlot.SetHoldingTile(tile, middleSlotIndex);

                    GameObject tileRepresentation = new GameObject("TileRepresentation");
                    tileRepresentation.transform.SetParent(gridSlots[slotIndex].transform, false);
                    tileRepresentation.transform.localPosition = Vector3.zero; // Center it in the slot


                    Image tileImage = tileRepresentation.AddComponent<Image>();
                    tileImage.color = tile.GetTileData().color;


                    RectTransform rectTransform = tileImage.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(102, 102);

                    // Add a BoxCollider2D for raycasting
                    BoxCollider2D boxCollider = tileRepresentation.AddComponent<BoxCollider2D>();
                    boxCollider.size = rectTransform.sizeDelta;

                    Debug.Log($"Tile stored at slot ({gridX}, {gridY}) with index {slotIndex}");
                }
                else
                {
                    Debug.LogWarning($"Slot at index {slotIndex} is already occupied or null.");
                }
            }
            else
            {
                CloseChest();
                Debug.LogWarning($"Position ({gridX}, {gridY}) is out of bounds.");
            }
        }
    }



    private void PickStoredTile(GridSlot slot)
    {
        TileBehaviour tile = slot.GetHoldingTile();

        tile.Store(false);

        List<GridSlot> slotsToReset = new List<GridSlot>();

        for (int index = 0; index < gridSlots.Length; index++)
        {
            GridSlot gridSlot = gridSlots[index].GetComponent<GridSlot>();
            if (gridSlot != null && gridSlot.GetMiddleSlot() == slot.GetMiddleSlot() && gridSlot.GetMiddleSlot() != -1)
            {
                slotsToReset.Add(gridSlot);
            }
        }

        // Reset later because the reset changes the middle slot aswell
        foreach (var gridSlot in slotsToReset)
        {
            gridSlot.ResetHoldingTile(); 
        }
    }

    private bool CheckValidStoring(TileBehaviour tile, int middleSlotIndex)
    {
        if (middleSlotIndex == -2)
        {
            CloseChest();
            return false;
        }
        var occupiedCells = tile.GetTileData().shape.occupiedCells;

        int middleX = middleSlotIndex % gridWidth;
        int middleY = middleSlotIndex / gridWidth;

        foreach (var cell in occupiedCells)
        {
            int gridX = middleX + cell.x - 1;
            int gridY = middleY + cell.y - 1;


            if (!IsWithinGridBounds(gridX, gridY))
            {
                CloseChest();
                Debug.Log($"Tile is out of bounds at ({gridX}, {gridY}).");
                return false;
            }


            int slotIndex = gridX + gridY * gridWidth;


            GridSlot gridSlot = gridSlots[slotIndex].GetComponent<GridSlot>();
            if (gridSlot != null && gridSlot.GetHoldingTile() != null)
            {
                Debug.Log($"Slot at ({gridX}, {gridY}) is already occupied.");
                return false;
            }
        }
        Debug.Log("Valid stroing location");
        return true;
    }


    private bool IsWithinGridBounds(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

}

