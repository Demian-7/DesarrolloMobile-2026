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

        /// <summary>
        /// Used by loader to restore the wallet value
        /// </summary>
        /// <param name="amount"></param>
        public void SetBalance(CurrencyValue amount)
        {
            Balance = amount.Mantissa > 0 ? amount : CurrencyValue.Zero;
        }
    }
}