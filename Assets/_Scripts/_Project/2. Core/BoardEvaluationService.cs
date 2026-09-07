using System.Collections.Generic;
using System.Linq;
using Game.Foundation;

namespace Game.Core
{
    public class BoardEvaluationService
    {
        private readonly IEventBus _eventBus;

        public bool IsMultiplicativeStackingUnlocked { get; set; } = false;

        public BoardEvaluationService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<BoardChangedEvent>(OnBoardChanged);
        }

        private void OnBoardChanged(BoardChangedEvent evt)
        {
            CurrencyValue totalRate = EvaluateBoard(evt.Board);
            _eventBus.Publish(new GenerationRateChangedEvent { NewRate = totalRate });
        }

        public CurrencyValue EvaluateBoard(Board board)
        {
            var allCards = board.Cells.Values
                .Where(c => c.OccupiedCard != null)
                .Select(c => c.OccupiedCard)
                .ToList();

            //Reset State
            foreach (var card in allCards)
            {
                card.BaseValue = CurrencyValue.Zero;
                card.EffectiveValue = CurrencyValue.Zero;
                card.IsResolved = false;
            }

            var resolvedCards = new HashSet<CardInstance>();

            //Flat Phase
            foreach (var card in allCards)
            {
                var flat = card.GetBehavior<FlatGenerationBehavior>();
                if (flat != null)
                {
                    card.BaseValue = flat.BaseAmount;
                    double multiplier = CalculateModifierMultiplier(board, card.CurrentCell);
                    card.EffectiveValue = card.BaseValue * multiplier;

                    card.IsResolved = true;
                    resolvedCards.Add(card);
                }
            }

            //Percentage Phase (Iterative Wavefront)
            bool changed = true;
            while (changed)
            {
                changed = false;
                var snapshot = new HashSet<CardInstance>(resolvedCards);

                foreach (var card in allCards.Where(c => !snapshot.Contains(c)))
                {
                    var percent = card.GetBehavior<PercentageGenerationBehavior>();
                    if (percent != null)
                    {
                        var neighbors = board.GetNeighbors(card.CurrentCell.Coordinate);
                        var resolvedNeighbors = neighbors
                            .Where(n => n.OccupiedCard != null && snapshot.Contains(n.OccupiedCard))
                            .ToList();

                        if (resolvedNeighbors.Count > 0)
                        {
                            CurrencyValue neighborSum = CurrencyValue.Zero;

                            foreach (var n in resolvedNeighbors)
                            {
                                neighborSum += n.OccupiedCard.EffectiveValue;
                            }

                            card.BaseValue = neighborSum * percent.Percentage;
                            double multiplier = CalculateModifierMultiplier(board, card.CurrentCell);
                            card.EffectiveValue = card.BaseValue * multiplier;

                            card.IsResolved = true;
                            resolvedCards.Add(card);
                            changed = true;
                        }
                    }
                }
            }

            CurrencyValue total = CurrencyValue.Zero;
            foreach (var card in allCards)
            {
                total += card.EffectiveValue;
            }

            return total;
        }

        private double CalculateModifierMultiplier(Board board, Cell cell)
        {
            var neighbors = board.GetNeighbors(cell.Coordinate);
            var modifiers = neighbors
                .Where(n => n.OccupiedCard != null)
                .Select(n => n.OccupiedCard.GetBehavior<ModifierBehavior>())
                .Where(m => m != null)
                .ToList();

            if (modifiers.Count == 0)
                return 1.0;

            if (IsMultiplicativeStackingUnlocked)
            {
                //Exponential: 2x and 3x = 6.0x
                double total = 1.0;
                foreach (var mod in modifiers)
                {
                    total *= mod.Multiplier;
                }

                return total;
            }

            else
            {
                //Additive: 2x (+100%) and 3x (+200%) = 4.0x (+300%)
                double bonusSum = 0;
                foreach (var mod in modifiers)
                {
                    bonusSum += (mod.Multiplier - 1.0);
                }
                
                return 1.0 + bonusSum;
            }
        }
    }
}