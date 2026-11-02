using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    public class ShopService
    {
        private readonly IEventBus _eventBus;
        private readonly PackOpeningService _packOpeningService;
        private readonly ZoneService _zoneService;

        public ShopService(IEventBus eventBus, PackOpeningService packOpeningService,
            ZoneService zoneService)
        {
            _eventBus = eventBus;
            _packOpeningService = packOpeningService;
            _zoneService = zoneService;
        }

        public bool TryPurchaseSlot(Zone zone, CellCoord coord)
        {
            if (!zone.Board.Cells.TryGetValue(coord, out Cell cell) || cell.IsUnlocked)
                return false;
            
            var price = zone.PricingStrategy.GetPrice(zone.Board, coord);
            if (!zone.Wallet.TrySpend(price))
                return false;

            zone.Board.UnlockCell(coord);
            zone.PricingStrategy.OnPurchase(coord);
            
            
            _eventBus.Publish(new SlotUnlockedEvent(zone.ZoneId, coord));
            _eventBus.Publish(new CurrencyChangedEvent(zone.ZoneId, zone.Wallet.Balance));
            _eventBus.Publish(new BoardChangedEvent(zone.ZoneId));
            
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
            _eventBus.Publish(new CurrencyChangedEvent(zone.ZoneId, zone.Wallet.Balance));
    
            return true;
        }

        public bool TryUnlockZone(Zone currentZone, ZoneDefinition nextZoneDefinition)
        {
            Debug.Assert(currentZone != null);
            Debug.Assert(nextZoneDefinition != null);

            if (_zoneService.IsZoneUnlocked(nextZoneDefinition.Id))
                return false;

            if (!currentZone.Wallet.TrySpend(nextZoneDefinition.UnlockPrice))
                return false;

            _eventBus.Publish(new CurrencyChangedEvent(currentZone.ZoneId, currentZone.Wallet.Balance));
            _zoneService.CreateAndActivateZone(nextZoneDefinition);
            return true;
        }
        
        public CurrencyValue GetSlotPrice(Zone zone, CellCoord coord)
        {
            return zone.PricingStrategy.GetPrice(zone.Board, coord);
        }
    }
}