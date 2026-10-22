using System.Collections.Generic;
using Game.Foundation;

namespace Game.Core
{
    public class Board
    {
        public Dictionary<CellCoord, Cell> Cells = new();

        private static readonly Dictionary<Directions, (int dx, int dy)> DirectionOffsets = new()
        {
            { Directions.Right, (1, 0) },
            { Directions.Left, (-1, 0) },
            { Directions.Top, (0, 1) },
            { Directions.Bottom, (0, -1) },
            { Directions.TopRight, (1, 1) },
            { Directions.TopLeft, (-1, 1) },
            { Directions.BottomRight, (1, -1) },
            { Directions.BottomLeft, (-1, -1) },
        };

        private const Directions AllDirections =
            Directions.Right | Directions.Left | Directions.Top | Directions.Bottom |
            Directions.TopRight | Directions.TopLeft | Directions.BottomRight | Directions.BottomLeft;

        public void AddCell(int x, int y)
        {
            Cells[new CellCoord(x, y)] = new Cell(x, y);
        }

        public void UnlockCell(CellCoord coord)
        {
            if (Cells.TryGetValue(coord, out var cell))
                cell.IsUnlocked = true;
        }

        public bool TryPlaceCard(CardInstance card, CellCoord coord)
        {
            if (!Cells.TryGetValue(coord, out Cell cell) || !cell.IsUnlocked || cell.OccupiedCard != null)
                return false;

            cell.OccupiedCard = card;
            card.CurrentCell = cell;
            return true;
        }

        public List<Cell> GetCellsInDirections(CellCoord origin, Directions directions, int range)
        {
            var results = new List<Cell>();
            foreach (var kvp in DirectionOffsets)
            {
                if (!directions.HasFlag(kvp.Key)) continue;
                var (dx, dy) = kvp.Value;
                for (int step = 1; step <= range; step++)
                {
                    var target = new CellCoord(origin.X + dx * step, origin.Y + dy * step);
                    if (Cells.TryGetValue(target, out var cell))
                        results.Add(cell);
                }
            }
            return results;
        }

        public List<Cell> GetNeighbors(CellCoord coord) => GetCellsInDirections(coord, AllDirections, 1);

        public List<Cell> GetRow(CellCoord coord)
        {
            var row = new List<Cell>();
            foreach (var kvp in Cells)
                if (kvp.Key.Y == coord.Y && !kvp.Key.Equals(coord))
                    row.Add(kvp.Value);
            return row;
        }

        public List<Cell> GetColumn(CellCoord coord)
        {
            var column = new List<Cell>();
            foreach (var kvp in Cells)
                if (kvp.Key.X == coord.X && !kvp.Key.Equals(coord))
                    column.Add(kvp.Value);
            return column;
        }

        public List<Cell> GetAllCells() => new List<Cell>(Cells.Values);

        public List<Cell> ResolveInteraction(CardInteraction interaction, CellCoord origin)
        {
            return interaction.Type switch
            {
                InteractionType.Directional => GetCellsInDirections(origin, interaction.Directions, interaction.Range),
                InteractionType.Adjacent => GetCellsInDirections(origin, AllDirections, interaction.Range),
                InteractionType.Row => GetRow(origin),
                InteractionType.Column => GetColumn(origin),
                InteractionType.AllMap => GetAllCells(),
                _ => new List<Cell>()
            };
        }
    }
}