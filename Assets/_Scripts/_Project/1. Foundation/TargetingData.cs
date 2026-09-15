using System;

namespace Game.Foundation
{
    public enum InteractionType
    {
        Directional,
        Adjacent,
        Row,
        Column,
        AllMap
    }

    [Flags]
    public enum Directions
    {
        None = 0,
        Right = 1 << 0, Left = 1 << 1, Top = 1 << 2, Bottom = 1 << 3,
        TopRight = 1 << 4, TopLeft = 1 << 5, BottomRight = 1 << 6, BottomLeft = 1 << 7,
    }

    [Serializable]
    public class CardInteraction
    {
        public InteractionType Type;
        public Directions Directions; //only read when Type == Directional
        public int Range = 1;
    }
}