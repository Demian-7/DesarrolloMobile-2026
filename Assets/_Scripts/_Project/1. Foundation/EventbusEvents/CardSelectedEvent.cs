using System;

namespace Game.Foundation
{
    public readonly struct CardSelectedEvent
    {
        public readonly Guid CardInstanceId;
        
        public CardSelectedEvent(Guid cardInstanceId)
        {
            CardInstanceId = cardInstanceId;
        }
    }
}