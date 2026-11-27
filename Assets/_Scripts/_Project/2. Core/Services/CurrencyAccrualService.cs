using Game.Foundation;

namespace Game.Core
{
    public class CurrencyAccrualService
    {
        private const float EVENT_PUBLISH_INTERVAL = 0.1f;

        private float _publishTimer;
        
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

            _publishTimer += deltaTime;
            bool shouldPublish = _publishTimer >= EVENT_PUBLISH_INTERVAL;
            if (shouldPublish)
            {
                _publishTimer -= EVENT_PUBLISH_INTERVAL;
            }

            foreach (var zone in zones)
            {
                if (zone.GenerationRate.Mantissa > 0)
                {
                    CurrencyValue tickAmount = zone.GenerationRate * (deltaTime * multiplier);
                    zone.Wallet.Add(tickAmount);

                    if (shouldPublish)
                    {
                        _eventBus.Publish(new CurrencyChangedEvent(zone.ZoneId, zone.Wallet.Balance));
                    }
                }
            }
        }
    }
}