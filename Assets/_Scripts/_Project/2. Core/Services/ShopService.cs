using Game.Foundation;

namespace Game.Core
{
    public class ShopService
    {
        private readonly IEventBus _eventBus;
        private readonly ISlotPricingStrategy _pricingStrategy;
        private readonly PackOpeningService _packOpeningService;
        private readonly ZoneService _zoneService;

        public ShopService(IEventBus eventBus, ISlotPricingStrategy pricingStrategy,
            PackOpeningService packOpeningService, ZoneService zoneService)
        {
            _eventBus = eventBus;
            _pricingStrategy = pricingStrategy;
            _packOpeningService = packOpeningService;
            _zoneService = zoneService;
        }

        public bool TryPurchaseSlot(Zone zone, CellCoord coord)
        {
            if (!zone.Board.Cells.TryGetValue(coord, out Cell cell) || cell.IsUnlocked)
                return false;
            
            var price = _pricingStrategy.GetPrice(zone.Board, coord);
            if (!zone.Wallet.TrySpend(price))
                return false;

            zone.Board.UnlockCell(coord);
            _pricingStrategy.OnPurchase(coord);
            _eventBus.Publish(new SlotUnlockedEvent(zone.ZoneId, coord));
            return true;
        }

        public bool TryOpenPack(Zone zone, CardPackDefinition pack)
        {
            if (!zone.Wallet.TrySpend(pack.Price))
                return false;

            var drawnCards = _packOpeningService.Draw(pack);
            foreach (var cardDef in drawnCards)
                zone.CardInventory.Add(new CardInstance(cardDef));

            _eventBus.Publish(new PackOpenedEvent(zone.ZoneId, drawnCards));
            return true;
        }

        public bool TryUnlockZone(Zone currentZone, ZoneDefinition nextZoneDefinition)
        {
            if (!currentZone.Wallet.TrySpend(nextZoneDefinition.UnlockPrice))
                return false;

            _zoneService.CreateAndActivateZone(nextZoneDefinition);
            return true;
        }
    }
}