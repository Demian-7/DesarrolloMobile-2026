using Game.Foundation;

namespace Game.Core
{
    public class ShopService
    {
        private readonly IEventBus _eventBus;
        
        public ShopService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public bool TryPurchaseSlot(Zone zone, CellCoord coord, ISlotPricingStrategy pricingStrategy)
        {
            if (!zone.Board.Cells.TryGetValue(coord, out Cell cell) || cell.IsUnlocked)
                return false;

            CurrencyValue price = pricingStrategy.GetPrice(zone.Board, coord);

            if (zone.Wallet.TrySpend(price))
            {
                cell.IsUnlocked = true;
                
                //UI needs to know wallet changed and the board changed
                _eventBus.Publish(new CurrencyChangedEvent {ZoneId = zone.ZoneId, NewBalance = zone.Wallet.Balance});
                _eventBus.Publish(new BoardChangedEvent {ZoneId = zone.ZoneId});
                
                return true;
            }

            return false;
        }
        
        //TryOpenPack and TryUnlockZone are stubbed.
        //they require the ScriptableObject definitions to be made.
        public void TryOpenPack() { }
        public void TryUnlockZone() { }
    }
}