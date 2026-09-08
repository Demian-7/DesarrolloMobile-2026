using System;
using UnityEngine;
using Game.Foundation;

namespace Game.Input.Desktop
{
    public class DesktopInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<Vector2> OnScreenTapped;
        public event Action<Guid, Vector2> OnCardDropped;
        public event Action<float> OnPinchZoom;
        public event Action<Vector2> OnPanDelta;

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnScreenTapped?.Invoke(UnityEngine.Input.mousePosition);
            }

            //Fake a card drop for testing.
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                OnCardDropped?.Invoke(Guid.NewGuid(), UnityEngine.Input.mousePosition);
            }
        }
    }
}