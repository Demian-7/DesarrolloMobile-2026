using Game.Foundation;

namespace Game.Core
{
    public class FlatGenerationBehavior : ICardBehavior
    {
        public readonly CurrencyValue BaseAmount;

        public FlatGenerationBehavior(CurrencyValue amount)
        {
            BaseAmount = amount;
        }
    }
}