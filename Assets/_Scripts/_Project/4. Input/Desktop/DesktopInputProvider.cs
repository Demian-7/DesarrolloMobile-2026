using System;
using Game.Foundation;
using UnityEngine;

namespace Game.Input.Desktop
{
    public class DesktopInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<Vector2> OnInteractBegan;
        public event Action<Vector2> OnInteractMoved;
        public event Action<Vector2> OnInteractEnded;
        
        public event Action<Guid, Vector2> OnUICardDragBegan;
        public event Action<Guid, Vector2> OnUICardDragMoved;
        public event Action<Guid> OnUICardDragEnded;
        public event Action<Guid> OnUICardTapped;
        
        public event Func<Guid, Vector2, bool> OnCardDropped; 
        public event Action<float> OnPinchZoom;
        public event Action<Vector2> OnPanDelta;

        private bool _isInteracting;
        private Vector2 _lastInteractPos;

        public bool NotifyCardDroppedUI(Guid cardId, Vector2 screenPos)
        {
            return OnCardDropped?.Invoke(cardId, screenPos) ?? false;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _isInteracting = true;
                _lastInteractPos = UnityEngine.Input.mousePosition;
                OnInteractBegan?.Invoke(_lastInteractPos);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _isInteracting = false;
                OnInteractEnded?.Invoke(UnityEngine.Input.mousePosition);
            }
            else if (_isInteracting)
            {
                Vector2 currentPos = UnityEngine.Input.mousePosition;
                if (currentPos != _lastInteractPos)
                {
                    _lastInteractPos = currentPos;
                    OnInteractMoved?.Invoke(currentPos);
                }
            }

            if (UnityEngine.Input.GetMouseButton(1)) 
            {
                float moveX = UnityEngine.Input.GetAxis("Mouse X");
                float moveY = UnityEngine.Input.GetAxis("Mouse Y");
                if (moveX != 0 || moveY != 0)
                {
                    OnPanDelta?.Invoke(new Vector2(moveX, moveY));
                }
            }

            float scroll = UnityEngine.Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                OnPinchZoom?.Invoke(scroll);
            }
        }
        
        public void NotifyUICardDragBegan(Guid cardId, Vector2 screenPos) => OnUICardDragBegan?.Invoke(cardId, screenPos);
        public void NotifyUICardDragMoved(Guid cardId, Vector2 screenPos) => OnUICardDragMoved?.Invoke(cardId, screenPos);
        public void NotifyUICardDragEnded(Guid cardId) => OnUICardDragEnded?.Invoke(cardId);
        public void NotifyUICardTapped(Guid cardId) => OnUICardTapped?.Invoke(cardId);
    }
}