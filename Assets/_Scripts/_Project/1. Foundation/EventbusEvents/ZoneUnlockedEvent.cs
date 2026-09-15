namespace Game.Foundation
{
    public readonly struct ZoneUnlockedEvent
    {
        public readonly string NewZoneId;

        public ZoneUnlockedEvent(string newZoneId)
        {
            NewZoneId = newZoneId;
        }
    }
}