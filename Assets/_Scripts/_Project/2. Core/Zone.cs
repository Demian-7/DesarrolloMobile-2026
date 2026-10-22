using System.Collections.Generic;
using Game.Foundation;

namespace Game.Core
{
    public class Zone
    {
        public string ZoneId;
        public Board Board = new Board();
        public CurrencyWallet Wallet = new CurrencyWallet();

        public List<CardInstance> CardInventory = new();
        
        public CurrencyValue GenerationRate = CurrencyValue.Zero;
    }
}