using System;

namespace Game.Foundation
{
    public readonly struct CardRemovedFromBoardEvent
    {
        public readonly string ZoneId;
        public readonly Guid CardInstanceId;
        public readonly CellCoord Coord;

        public CardRemovedFromBoardEvent(string zoneId, Guid cardInstanceId, CellCoord coord)
        {
            ZoneId = zoneId;
            CardInstanceId = cardInstanceId;
            Coord = coord;
        }
    }
}