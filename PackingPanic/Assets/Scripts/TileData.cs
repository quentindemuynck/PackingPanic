using System.Collections.Generic;
using UnityEngine;

namespace PackingPanick.TileData
{
    public struct TileShape
    {
        public string name;
        public List<Vector2Int> occupiedCells;
        public int id;

        public TileShape(int id, string name, List<Vector2Int> cells)
        {
            this.id = id;
            this.name = name;
            this.occupiedCells = cells;
        }

        public void Rotate()
        {
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                int x = occupiedCells[i].x;
                int y = occupiedCells[i].y;
                occupiedCells[i] = new Vector2Int(y, -x);
            }
        }
    }

    public static class TileShapeLibrary
    {
        // Dictionary for lookup by name or ID
        public static readonly Dictionary<string, TileShape> ShapesByName = new Dictionary<string, TileShape>
        {
            // Stole the names from tetris ugh I mean borrowed 
            { "T", new TileShape(0, "T", new List<Vector2Int>
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                    new Vector2Int(1, 2)
                })
            },
            { "L", new TileShape(1, "L", new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2)
                })
            },
            { "J", new TileShape(2, "J", new List<Vector2Int>
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2),
                    new Vector2Int(0, 2)
                })
            },
            { "Z", new TileShape(3, "Z", new List<Vector2Int>
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2),
                    new Vector2Int(2, 2)
                })
            },
            { "S", new TileShape(4, "S", new List<Vector2Int>
                {
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2)
                })
            },
            { "I", new TileShape(5, "I", new List<Vector2Int>
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2)
                })
            },
            { "O", new TileShape(6, "O", new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1)
                })
            },
        };

        
        public static readonly Dictionary<int, TileShape> ShapesById = new Dictionary<int, TileShape>();

        
        static TileShapeLibrary()
        {
            foreach (var shape in ShapesByName.Values)
            {
                ShapesById[shape.id] = shape;
            }
        }

        public static TileShape GetShape(string shapeName)
        {
            if (ShapesByName.TryGetValue(shapeName, out var shape))
            {
                return shape;
            }
            else
            {
                Debug.LogError($"Shape '{shapeName}' not found in library.");
                return default;
            }
        }

        public static TileShape GetShape(int id)
        {
            if (ShapesById.TryGetValue(id, out var shape))
            {
                return shape;
            }
            else
            {
                Debug.LogError($"Shape with ID '{id}' not found in library.");
                return default;
            }
        }
    }
}
