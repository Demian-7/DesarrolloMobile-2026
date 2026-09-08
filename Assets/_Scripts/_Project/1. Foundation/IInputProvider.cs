using System;
using UnityEngine;

namespace Game.Foundation
{
    public interface IInputProvider
    {
        event Action<Vector2> OnScreenTapped;
        event Action<Guid, Vector2> OnCardDropped;
        event Action<float> OnPinchZoom;
        event Action<Vector2> OnPanDelta;
    }
}