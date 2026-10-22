using System;

namespace Game.Foundation
{
    [Flags]
    public enum Directions
    {
        None = 0,
        Right = 1 << 0, Left = 1 << 1, Top = 1 << 2, Bottom = 1 << 3,
        TopRight = 1 << 4, TopLeft = 1 << 5, BottomRight = 1 << 6, BottomLeft = 1 << 7,
    }
}