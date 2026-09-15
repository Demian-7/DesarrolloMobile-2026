namespace Game.Foundation
{
    public readonly struct SlotUnlockedEvent
    {
        public readonly string ZoneId;
        public readonly CellCoord Coord;

        public SlotUnlockedEvent(string zoneId, CellCoord coord)
        {
            ZoneId = zoneId;
            Coord = coord;
        }
    }
}