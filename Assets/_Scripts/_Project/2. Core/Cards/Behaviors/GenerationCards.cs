using System;
using Game.Core;
using Game.Foundation;
using UnityEngine;

[Serializable]
public class FlatGenerationEffect : IGeneratingEffect
{
    public CurrencyValue BaseAmount;

    public CurrencyValue Evaluate(BoardEvaluationContext ctx, CellCoord ownerCell)
    {
        return BaseAmount;
    }
}

[Serializable]
public class PercentageGenerationEffect : IGeneratingEffect
{
    [field: SerializeField] public CardInteraction Interaction { get; set; }
    public float Percentage; // e.g. 1.5 = 150%

    public CurrencyValue Evaluate(BoardEvaluationContext ctx, CellCoord ownerCell)
    {
        var targetCells = ctx.Board.ResolveInteraction(Interaction, ownerCell);
        CurrencyValue sum = CurrencyValue.Zero;

        foreach (var target in targetCells)
        {
            if (ctx.TryGetEffectiveValue(target.Coordinate, out CurrencyValue targetValue))
            {
                sum += targetValue;
            }
        }

        return sum * Percentage;
    }
}