using System;
using UnityEngine;

namespace Game.Foundation
{
    public interface IInputProvider
    {
        event Action<Vector2> OnInteractBegan;
        event Action<Vector2> OnInteractMoved;
        event Action<Vector2> OnInteractEnded;

        event Action<Guid, Vector2> OnUICardDragBegan;
        event Action<Guid, Vector2> OnUICardDragMoved;
        event Action<Guid> OnUICardDragEnded;
        event Action<Guid> OnUICardTapped;
        event Func<Guid, Vector2, bool> OnCardDropped; 
        
        event Action<float> OnPinchZoom;
        event Action<Vector2> OnPanDelta;

        bool NotifyCardDroppedUI(Guid cardId, Vector2 screenPos);
        void NotifyUICardDragBegan(Guid cardId, Vector2 screenPos);
        void NotifyUICardDragMoved(Guid cardId, Vector2 screenPos);
        void NotifyUICardDragEnded(Guid cardId);
        void NotifyUICardTapped(Guid cardId);
    }
}