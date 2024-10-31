using PackingPanick.TileData;
using UnityEngine;

public static class TileSpriteGenerator
{
    public static Sprite CreateTileSprite(TileShape shape, Color color)
    {
        // Define the size of the tile texture
        const int tileSize = 102; 
        int textureSize = tileSize * 3; 

        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        foreach (var cell in shape.occupiedCells)
        {
            int x = cell.x * tileSize;
            int y = cell.y * tileSize;

            for (int xOffset = 0; xOffset < tileSize; xOffset++)
            {
                for (int yOffset = 0; yOffset < tileSize; yOffset++)
                {
                    pixels[(y + yOffset) * textureSize + (x + xOffset)] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
