namespace Game.Foundation
{
    public readonly struct ActiveZoneChangedEvent
    {
        public readonly string ActiveZoneId;
        
        public ActiveZoneChangedEvent(string activeZoneId)
        {
            ActiveZoneId = activeZoneId;
        }
    }
}