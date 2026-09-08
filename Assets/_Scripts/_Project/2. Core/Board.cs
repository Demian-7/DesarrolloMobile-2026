using System.Collections.Generic;

namespace Game.Core
{
    public class Board
    {
        public Dictionary<CellCoord, Cell> Cells = new();

        public void AddCell(int x, int y)
        {
            Cells[new CellCoord(x, y)] = new Cell(x, y);
        }

        public bool TryPlaceCard(CardInstance card, CellCoord coord)
        {
            if (!Cells.TryGetValue(coord, out Cell cell) || !cell.IsUnlocked || cell.OccupiedCard != null)
                return false;

            cell.OccupiedCard = card;
            card.CurrentCell = cell;
            return true;
        }

        public List<Cell> GetNeighbors(CellCoord coord)
        {
            var neighbors = new List<Cell>();
            var dirs = new[]
            {
                new CellCoord(0, 1),
                new CellCoord(0, -1),
                new CellCoord(1, 0),
                new CellCoord(-1, 0)
            };

            foreach (var dir in dirs)
            {
                var neighborCoord = new CellCoord(coord.X + dir.X, coord.Y + dir.Y);
                if (Cells.TryGetValue(neighborCoord, out Cell neighborCell))
                {
                    neighbors.Add(neighborCell);
                }
            }
        
            return neighbors;
        }
    }
}