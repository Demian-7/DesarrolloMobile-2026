namespace Game.Core
{
    public class Cell
    {
        public CellCoord Coordinate;
        public bool IsUnlocked;
        public CardInstance OccupiedCard;

        public Cell(int x, int y)
        {
            Coordinate = new CellCoord(x, y);
        }
    }
}