using System.Collections.Generic;
using Game.Foundation;

namespace Game.Core
{
    public class Zone
    {
        public string ZoneId;
        public ZoneDefinition Definition;
        public ISlotPricingStrategy PricingStrategy;
        public Board Board = new();
        public CurrencyWallet Wallet = new();

        public List<CardInstance> CardInventory = new();
        
        public CurrencyValue GenerationRate = CurrencyValue.Zero;
    }
}