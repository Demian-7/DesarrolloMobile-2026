using System.Linq;
using Game.Foundation;

namespace Game.Core
{
    public interface ISlotPricingStrategy
    {
        CurrencyValue GetPrice(Board board, CellCoord coord);
    }

    public class SequentialPricingStrategy : ISlotPricingStrategy
    {
        private readonly CurrencyValue _basePrice;
        private readonly double _growthFactor;

        public SequentialPricingStrategy(CurrencyValue basePrice, double growthFactor = 1.5)
        {
            _basePrice = basePrice;
            _growthFactor = growthFactor;
        }

        public CurrencyValue GetPrice(Board board, CellCoord coord)
        {
            //Price scales based on how many cells are already unlocked
            int unlockedCount = board.Cells.Values.Count(c => c.IsUnlocked);
            
            //Base * (1.5 ^ unlocked_count)
            double multiplier = System.Math.Pow(_growthFactor, unlockedCount);
            return _basePrice * multiplier;
        }
    }
}