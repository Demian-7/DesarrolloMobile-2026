using Game.Foundation;

namespace Game.Core
{
    public interface ISlotPricingStrategy
    {
        CurrencyValue GetPrice(Board board, CellCoord coord);
        void OnPurchase(CellCoord coord);
    }

    public class SequentialPricingStrategy : ISlotPricingStrategy
    {
        private readonly BoardLayoutDefinition _layout;

        public SequentialPricingStrategy(BoardLayoutDefinition layout) => _layout = layout;

        public CurrencyValue GetPrice(Board board, CellCoord coord)
        {
            var entry = _layout.UnlockSequence.Find(e => e.Coordinate.Equals(coord));
            return entry != null ? entry.Price : CurrencyValue.Zero;
        }

        public void OnPurchase(CellCoord coord) { } // fixed price table, nothing to update
    }

    public class DoublingPricingStrategy : ISlotPricingStrategy
    {
        private CurrencyValue _currentPrice;

        public DoublingPricingStrategy(CurrencyValue basePrice) => _currentPrice = basePrice;

        public CurrencyValue GetPrice(Board board, CellCoord coord) => _currentPrice;

        public void OnPurchase(CellCoord coord) => _currentPrice *= 2; // "doubled when the player buys any card slot"
    }
}