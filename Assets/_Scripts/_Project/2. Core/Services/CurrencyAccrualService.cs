using Game.Foundation;

namespace Game.Core
{
    public class CurrencyAccrualService
    {
        private readonly ZoneService _zoneService;
        private readonly BoostService _boostService;
        private readonly IEventBus _eventBus;

        public CurrencyAccrualService(ZoneService zoneService, BoostService boostService, IEventBus eventBus)
        {
            _zoneService = zoneService;
            _boostService = boostService;
            _eventBus = eventBus;
        }

        public void Tick(float deltaTime)
        {
            var zones = _zoneService.GetZonesToSimulate();
            float multiplier = _boostService.CurrentMultiplier;

            foreach (var zone in zones)
            {
                if (zone.GenerationRate.Mantissa > 0)
                {
                    CurrencyValue tickAmount = zone.GenerationRate * (deltaTime * multiplier);
                    zone.Wallet.Add(tickAmount);
                    
                    //TODO: debounce this event, so it fires ~10 times/second instead of every single frame to save UI updates.
                    _eventBus.Publish(new CurrencyChangedEvent(zone.ZoneId, zone.Wallet.Balance));
                }
            }
        }
    }
}