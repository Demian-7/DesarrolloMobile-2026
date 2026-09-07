using System;
using System.Collections.Generic;
using Game.Foundation;

public struct CellCoord : IEquatable<CellCoord>
{
    public int X;
    public int Y;

    public CellCoord(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(CellCoord other)
    {
        return (X == other.X && Y == other.Y);
    }

    public override bool Equals(object obj)
    {
        return obj is CellCoord other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

public class Cell
{
    public CellCoord Coordinate;
    public bool IsUnlocked = true;
    public CardInstance OccupiedCard;

    public Cell(int x, int y)
    {
        Coordinate = new CellCoord(x, y);
    }
}

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

//Behaviors
public interface ICardBehavior
{ }

public class FlatGenerationBehavior : ICardBehavior
{
    public readonly CurrencyValue BaseAmount;

    public FlatGenerationBehavior(CurrencyValue amount)
    {
        BaseAmount = amount;
    }
}

public class PercentageGenerationBehavior : ICardBehavior
{
    public readonly double Percentage; //1.5 for 150%

    public PercentageGenerationBehavior(double percent)
    {
        Percentage = percent;
    }
}

public class ModifierBehavior : ICardBehavior
{
    public readonly double Multiplier;
    
    public ModifierBehavior(double amount)
    {
        Multiplier = amount;
    }
}

public class CardInstance
{
    public string Id;
    public List<ICardBehavior> Behaviors = new();
    public Cell CurrentCell;
    
    //Runtime state calculated by the Evaluation Service
    public CurrencyValue BaseValue;
    public CurrencyValue EffectiveValue;
    public bool IsResolved;

    public CardInstance(string id, params ICardBehavior[] behaviors)
    {
        Id = id;
        Behaviors.AddRange(behaviors);
    }

    public T GetBehavior<T>() where T : class, ICardBehavior
    {
        foreach (var b in Behaviors)
            if (b is T match)
                return match;

        return null;
    }
}