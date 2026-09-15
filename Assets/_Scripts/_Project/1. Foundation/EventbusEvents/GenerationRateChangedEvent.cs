namespace Game.Foundation
{
    public readonly struct GenerationRateChangedEvent
    {
        public readonly string ZoneId;
        public readonly CurrencyValue NewRate;

        public GenerationRateChangedEvent(string zoneId, CurrencyValue newRate)
        {
            ZoneId = zoneId;
            NewRate = newRate;
        }
    }
}