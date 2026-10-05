using System;
using System.Linq;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class FlatGenerationEffect : GeneratingEffect
    {
        [Tooltip("The fixed base amount of currency this card generates by itself.")]
        public CurrencyValue BaseAmount;

        public override CurrencyValue Evaluate(BoardEvaluationContext ctx, CellCoord ownerCell)
        {
            return BaseAmount;
        }
    }

    [Serializable]
    public class PercentageGenerationEffect : GeneratingEffect
    {
        [Tooltip("Defines which surrounding or board-wide cards this effect targets and filters.")]
        public CardInteraction Interaction;

        [Tooltip(
            "PercentageOfSum: Generates a % based on the Effective Value of all matching targets.\nFlatPerMatch: Generates a fixed amount multiplied by the number of valid targets found.")]
        public GenerationAggregationMode Mode;

        [Tooltip(
            "Used if Mode is PercentageOfSum. E.g., 0.5 means it generates 50% of the targets' combined effective value.")]
        public float Percentage; // e.g. 1.5 = 150%

        [Tooltip("Used if Mode is FlatPerMatch. E.g., adds +10 generation for every matching target found.")]
        public CurrencyValue FlatPerMatch;

        public override CurrencyValue Evaluate(BoardEvaluationContext ctx, CellCoord ownerCell)
        {
            var targetCells = ctx.Board.ResolveInteraction(Interaction, ownerCell);

            var validTargets = targetCells
                .Where(cell => cell.OccupiedCard != null && Interaction.Matches(cell.OccupiedCard.Definition))
                .ToList();

            if (Mode == GenerationAggregationMode.FlatPerMatch)
            {
                return FlatPerMatch * validTargets.Count;
            }

            CurrencyValue sum = CurrencyValue.Zero;
            foreach (var target in validTargets)
            {
                if (ctx.TryGetEffectiveValue(target.Coordinate, out CurrencyValue targetValue))
                {
                    sum += targetValue;
                }
            }

            return sum * Percentage;
        }
    }
}