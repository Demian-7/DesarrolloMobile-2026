using System;
using Game.Foundation;
using UnityEngine;

namespace Game.Input.Desktop
{
    public class DesktopInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<Vector2> OnScreenTapped;
        public event Func<Guid, Vector2, bool> OnCardDropped; 
        public event Action<float> OnPinchZoom;
        public event Action<Vector2> OnPanDelta;

        public bool NotifyCardDroppedUI(Guid cardId, Vector2 screenPos)
        {
            return OnCardDropped?.Invoke(cardId, screenPos) ?? false;
        }

        private void Update()
        {
            // Left Click - Tapping cells/board
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnScreenTapped?.Invoke(UnityEngine.Input.mousePosition);
            }

            // Right Click Hold & Drag - Camera Panning
            if (UnityEngine.Input.GetMouseButton(1)) 
            {
                float moveX = UnityEngine.Input.GetAxis("Mouse X");
                float moveY = UnityEngine.Input.GetAxis("Mouse Y");
                if (moveX != 0 || moveY != 0)
                {
                    OnPanDelta?.Invoke(new Vector2(moveX, moveY));
                }
            }

            // Mouse Scroll - Camera Zoom
            float scroll = UnityEngine.Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                OnPinchZoom?.Invoke(scroll);
            }
        }
    }
}