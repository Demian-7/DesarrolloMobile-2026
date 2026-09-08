using Game.Foundation;

namespace Game.Core
{
    public class CurrencyAccrualService
    {
        private readonly ZoneService _zoneService;
        private readonly IEventBus _eventBus;

        public CurrencyAccrualService(ZoneService zoneService, IEventBus eventBus)
        {
            _zoneService = zoneService;
            _eventBus = eventBus;
        }

        //Should be called by Bootstrap
        public void Tick(float deltaTime)
        {
            var zones = _zoneService.GetZonesToSimulate();
            bool changed = false;

            foreach (var zone in zones)
            {
                if (zone.GenerationRate.Mantissa > 0)
                {
                    CurrencyValue tickAmount = zone.GenerationRate * deltaTime;
                    zone.Wallet.Add(tickAmount);
                    changed = true;
                    
                    //TODO: debounce this event, so it fires ~10 times/second instead of every single frame to save UI updates.
                    _eventBus.Publish(new CurrencyChangedEvent {ZoneId = zone.ZoneId, NewBalance = zone.Wallet.Balance});
                }
            }
        }
    }
}