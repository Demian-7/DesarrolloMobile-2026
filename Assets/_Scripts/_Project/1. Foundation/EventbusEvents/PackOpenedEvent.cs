using System.Collections.Generic;
using Game.Core;

namespace Game.Foundation
{
    public readonly struct PackOpenedEvent
    {
        public readonly string ZoneId;
        public readonly List<CardDefinition> DrawnCards;

        public PackOpenedEvent(string zoneId, List<CardDefinition> drawnCards)
        {
            ZoneId = zoneId;
            DrawnCards = drawnCards;
        }
    }
}