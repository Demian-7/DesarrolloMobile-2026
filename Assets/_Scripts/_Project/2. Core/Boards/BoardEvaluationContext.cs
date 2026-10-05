using System.Collections.Generic;
using System.Linq;
using Game.Foundation;

namespace Game.Core
{
    public class BoardEvaluationContext
    {
        public Board Board;

        private readonly Dictionary<CellCoord, CurrencyValue> _effectiveValues = new();
        private Dictionary<CellCoord, CurrencyValue> _previousPassSnapshot = new();
        private readonly Dictionary<CellCoord, List<ModifyingEffect>> _modifiersByTarget = new();
        private readonly GlobalUpgradeService _globalUpgrades;

        public BoardEvaluationContext(Board board, GlobalUpgradeService globalUpgrades)
        {
            Board = board;
            _globalUpgrades = globalUpgrades;
            _modifiersByTarget = BuildModifierTargetMap(board); // one scan over placed cards, up front
        }
        
        public bool TryGetEffectiveValue(CellCoord coord, out CurrencyValue value)
        => _previousPassSnapshot.TryGetValue(coord, out value);
        
        public CurrencyValue FinalizeEffectiveValue(CellCoord coord, CurrencyValue baseValue)
        {
            var modifiers = _modifiersByTarget.TryGetValue(coord, out var list) ? list : new();
            CurrencyValue effective;

            if (_globalUpgrades.HasEffect<MultiplicativeStackingEffect>())
            {
                effective = baseValue;
                foreach (var modifier in modifiers)
                    effective *= (1 + modifier.SignedMagnitude);
            }
            else //Default: Additive
            {
                float totalMagnitude = modifiers.Sum(m => m.SignedMagnitude);
                effective = baseValue * (1 + totalMagnitude);
            }

            _effectiveValues[coord] = CurrencyValue.Max(effective, CurrencyValue.Zero);
    
            return _effectiveValues[coord];
        }
        
        public void SnapshotForNextPass()
        => _previousPassSnapshot = new Dictionary<CellCoord, CurrencyValue>(_effectiveValues);
        
        private Dictionary<CellCoord, List<ModifyingEffect>> BuildModifierTargetMap(Board board)
        {
            var map = new Dictionary<CellCoord, List<ModifyingEffect>>();

            foreach (var cell in board.GetAllCells())
            {
                if (cell.OccupiedCard == null) continue;

                foreach (var behavior in cell.OccupiedCard.Definition.Effects)
                {
                    if (behavior is ModifyingEffect modifier)
                    {
                        var targetCells = board.ResolveInteraction(modifier.Interaction, cell.Coordinate);

                        foreach (var targetCell in targetCells)
                        {
                            if (!map.TryGetValue(targetCell.Coordinate, out var modifierList))
                            {
                                modifierList = new List<ModifyingEffect>();
                                map[targetCell.Coordinate] = modifierList;
                            }
                    
                            modifierList.Add(modifier);
                        }
                    }
                }
            }

            return map;
        }
    }

    public class BoardGenerationResult
    {
        public CurrencyValue TotalRate;
        public IReadOnlyDictionary<CellCoord, CurrencyValue> PerCellEffectiveValues;
    }
}