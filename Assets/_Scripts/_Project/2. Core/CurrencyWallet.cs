namespace Game.Foundation
{
    public class CurrencyWallet
    {
        public CurrencyValue Balance { get; private set; } = CurrencyValue.Zero;

        public void Add(CurrencyValue amount)
        {
            if (amount.Mantissa <= 0)
                return;
            
            Balance += amount;
        }

        public bool TrySpend(CurrencyValue amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }
    }
}