using Game.Foundation;

namespace Game.Core
{
    public class Zone
    {
        public string ZoneId;
        public Board Board = new Board();
        public CurrencyWallet Wallet = new CurrencyWallet();
        
        public CurrencyValue GenerationRate = CurrencyValue.Zero;
    }
}