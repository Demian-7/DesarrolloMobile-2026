namespace Game.Foundation
{
    public readonly struct BoardChangedEvent
    {
        public readonly string ZoneId;

        public BoardChangedEvent(string zoneId)
        {
            ZoneId = zoneId;
        }
    }
}