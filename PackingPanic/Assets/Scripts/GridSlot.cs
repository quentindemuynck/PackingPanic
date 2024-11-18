using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSlot : MonoBehaviour
{
    private TileBehaviour _holdingTile = null;
    private int _middleTileSlot = -1;
    private int slotIndex = 0;

    private Color originalColor;
    private Color originalTileColor;

    void Awake()
    {
        // Initialize original color of the grid slot
        Image image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
        }

        
        Transform tileRepresentation = transform.Find("TileRepresentation");
        if (tileRepresentation != null)
        {
            Image tileImage = tileRepresentation.GetComponent<Image>();
            if (tileImage != null)
            {
                originalTileColor = tileImage.color; 
            }
        }
    }

    void Update()
    {
        HideTakenSlots();
        Debug.Log(slotIndex);
    }

    public TileBehaviour GetHoldingTile()
    {
        return _holdingTile;
    }

    public void SetHoldingTile(TileBehaviour holdingTile, int middleSlotIndex)
    {
        _holdingTile = holdingTile;
        _middleTileSlot = middleSlotIndex;
        originalTileColor = holdingTile.GetTileData().color;
        Debug.Log("stored");
    }

    public void ResetHoldingTile()
    {
        _holdingTile = null;
        _middleTileSlot = -1;
        originalTileColor = Color.black;

        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        HighlightSlot(false);

    }

    private void HideTakenSlots()
    {
        Image image = this.gameObject.GetComponent<Image>();

        if (_holdingTile != null)
        {
            if (image != null)
            {
                image.enabled = false;
            }
        }
        else if(image != null)
        {
            image.enabled=true;
        }
    }

    public int GetMiddleSlot()
    {
        return _middleTileSlot;
    }

    public void HighlightSlot(bool highlight)
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            if (highlight)
            {
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            }
            else
            {
                image.color = originalColor;
            }
        }

        // Highlight the child "TileRepresentation" if it exists
        Transform tileRepresentation = transform.Find("TileRepresentation");
        if (tileRepresentation != null)
        {
            Image tileImage = tileRepresentation.GetComponent<Image>();
            if (tileImage != null)
            {
                if (highlight)
                {
                    tileImage.color = new Color(tileImage.color.r, tileImage.color.g, tileImage.color.b, 0.5f);
                }
                else
                {
                    tileImage.color = originalTileColor;
                }
            }
        }
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public int GetIndex()
    {
        return slotIndex;
    }

}
