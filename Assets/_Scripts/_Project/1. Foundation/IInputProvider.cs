using System;
using UnityEngine;

namespace Game.Foundation
{
    public interface IInputProvider
    {
        event Action<Vector2> OnScreenTapped;
        event Func<Guid, Vector2, bool> OnCardDropped;
        event Action<float> OnPinchZoom;
        event Action<Vector2> OnPanDelta;
    }
}