using System;

namespace Game.Foundation
{
    public struct CardPlacedEvent
    {
        public Guid CardInstanceId;
        public CellCoord TargetCell;
    }
}