using PackingPanick.TileData;
using UnityEngine;

public static class TileSpriteGenerator
{
    public static Sprite CreateTileSprite(TileShape shape, Color color)
    {
        const int tileSize = 102;
        int textureSize = tileSize * 3;

        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        
        int centerX = tileSize; 
        int centerY = tileSize;

        
        foreach (var cell in shape.occupiedCells)
        {
           
            int x = centerX + cell.x * tileSize - tileSize; 
            int y = centerY + cell.y * tileSize - tileSize; 

            
            for (int xOffset = 0; xOffset < tileSize; xOffset++)
            {
                for (int yOffset = 0; yOffset < tileSize; yOffset++)
                {
                    pixels[(y + yOffset) * textureSize + (x + xOffset)] = color;
                }
            }
        }

        
        FlipTextureVertically(pixels, textureSize, tileSize);

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static void FlipTextureVertically(Color[] pixels, int textureSize, int tileSize)
    {
        for (int y = 0; y < textureSize / 2; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                
                Color temp = pixels[y * textureSize + x];
                pixels[y * textureSize + x] = pixels[(textureSize - 1 - y) * textureSize + x];
                pixels[(textureSize - 1 - y) * textureSize + x] = temp;
            }
        }
    }
}
