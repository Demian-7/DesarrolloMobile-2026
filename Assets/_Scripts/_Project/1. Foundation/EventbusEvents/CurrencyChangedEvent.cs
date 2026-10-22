namespace Game.Foundation
{
    public readonly struct CurrencyChangedEvent
    {
        public readonly string ZoneId;
        public readonly CurrencyValue NewBalance;

        public CurrencyChangedEvent(string zoneId, CurrencyValue newBalance)
        {
            ZoneId = zoneId;
            NewBalance = newBalance;
        }
    }
}