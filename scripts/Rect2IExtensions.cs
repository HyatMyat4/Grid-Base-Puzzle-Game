using System.Collections.Generic;
using Godot;

namespace Game;

public static class Rect2IExtensions
{
    public static List<Vector2I> ToTiles(this Rect2I react)
    {
        var tiles = new List<Vector2I>();
        for (int x = react.Position.X; x < react.End.X; x++)
        {
            for (int y = react.Position.Y; y < react.End.Y; y++)
            {
                tiles.Add(new Vector2I(x, y));
            }
        }

        return tiles;
    }

    public static Rect2 ToRect2F(this Rect2I rect)
    {
        return new Rect2(rect.Position, rect.Size);
    }
}