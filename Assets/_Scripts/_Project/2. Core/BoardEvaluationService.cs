using System.Collections.Generic;
using System.Linq;
using Game.Foundation;

namespace Game.Core
{
    public class BoardEvaluationService
    {
        private readonly IEventBus _eventBus;
        private readonly ZoneService _zoneService;
        private readonly GlobalUpgradeService _globalUpgrades;

        public BoardEvaluationService(IEventBus eventBus, ZoneService zoneService, GlobalUpgradeService globalUpgrades)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _globalUpgrades = globalUpgrades;
            
            _eventBus.Subscribe<BoardChangedEvent>(OnBoardChanged);
        }

        private void OnBoardChanged(BoardChangedEvent evt)
        {
            Zone targetZone = _zoneService.GetZone(evt.ZoneId);
            if (targetZone == null)
                return;
            
            BoardGenerationResult result = EvaluateBoard(targetZone.Board);
            
            _eventBus.Publish(new GenerationRateChangedEvent(evt.ZoneId, result.TotalRate));
        }

        public BoardGenerationResult EvaluateBoard(Board board)
        {
            var allCards = board.Cells.Values
                .Where(c => c.OccupiedCard != null)
                .Select(c => c.OccupiedCard)
                .ToList();

            foreach (var card in allCards)
            {
                card.BaseValue = CurrencyValue.Zero;
                card.EffectiveValue = CurrencyValue.Zero;
                card.IsResolved = false;
                card.ResolvedPass = -1;
            }

            var context = new BoardEvaluationContext(board, _globalUpgrades);
            var resolvedCards = new HashSet<CardInstance>();

            foreach (var card in allCards)
            {
                var flatEffects = card.GetEffects<FlatGenerationEffect>().ToList();
                if (flatEffects.Count > 0)
                {
                    CurrencyValue totalBase = CurrencyValue.Zero;
                    foreach (var flat in flatEffects)
                        totalBase += flat.Evaluate(context, card.CurrentCell.Coordinate);

                    card.BaseValue = totalBase;
                    card.EffectiveValue = context.FinalizeEffectiveValue(card.CurrentCell.Coordinate, card.BaseValue);

                    card.IsResolved = true;
                    card.ResolvedPass = 0;
                    resolvedCards.Add(card);
                }
            }

            int pass = 0;
            bool changed = true;
            while (changed)
            {
                changed = false;
                pass++;
                
                context.SnapshotForNextPass(); 

                foreach (var card in allCards.Where(c => !c.IsResolved))
                {
                    var percentageEffects = card.GetEffects<PercentageGenerationEffect>().ToList();
                    if (percentageEffects.Count > 0)
                    {
                        bool canResolve = false;
                        CurrencyValue totalBase = CurrencyValue.Zero;

                        foreach (var percent in percentageEffects)
                        {
                            var targets = board.ResolveInteraction(percent.Interaction, card.CurrentCell.Coordinate);
                            
                            bool hasResolvedTarget = targets.Any(t => context.TryGetEffectiveValue(t.Coordinate, out _));

                            if (hasResolvedTarget)
                            {
                                canResolve = true;
                                totalBase += percent.Evaluate(context, card.CurrentCell.Coordinate);
                            }
                        }

                        if (canResolve)
                        {
                            card.BaseValue = totalBase;
                            card.EffectiveValue = context.FinalizeEffectiveValue(card.CurrentCell.Coordinate, card.BaseValue);

                            card.IsResolved = true;
                            card.ResolvedPass = pass;
                            resolvedCards.Add(card);
                            changed = true;
                        }
                    }
                }
            }
            
            foreach (var card in allCards.Where(c => !c.IsResolved))
            {
                card.EffectiveValue = context.FinalizeEffectiveValue(card.CurrentCell.Coordinate, card.BaseValue);
            }

            CurrencyValue totalRate = CurrencyValue.Zero;
            var perCellValues = new Dictionary<CellCoord, CurrencyValue>();
                    
            foreach (var card in allCards)
            {
                totalRate += card.EffectiveValue;
                perCellValues[card.CurrentCell.Coordinate] = card.EffectiveValue;
            }

            return new BoardGenerationResult 
            { 
                TotalRate = totalRate,
                PerCellEffectiveValues = perCellValues
            };
        }
    }
}