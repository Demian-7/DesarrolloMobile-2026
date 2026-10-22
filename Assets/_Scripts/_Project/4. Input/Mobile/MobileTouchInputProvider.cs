using System;
using Game.Foundation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Game.Input.Mobile
{
    public class MobileTouchInputProvider : MonoBehaviour, IInputProvider
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

        private bool _isInteracting = false;
        private int _interactTouchId = -1;
        private Vector2 _lastTouchPos;

        public bool NotifyCardDroppedUI(Guid cardId, Vector2 screenPos) => OnCardDropped?.Invoke(cardId, screenPos) ?? false;
        public void NotifyUICardDragBegan(Guid cardId, Vector2 screenPos) => OnUICardDragBegan?.Invoke(cardId, screenPos);
        public void NotifyUICardDragMoved(Guid cardId, Vector2 screenPos) => OnUICardDragMoved?.Invoke(cardId, screenPos);
        public void NotifyUICardDragEnded(Guid cardId) => OnUICardDragEnded?.Invoke(cardId);

        private void OnEnable() => EnhancedTouchSupport.Enable();
        private void OnDisable() => EnhancedTouchSupport.Disable();

        private void Update()
        {
            var touches = Touch.activeTouches;
            int touchCount = touches.Count;

            if (touchCount == 2)
            {
                if (_isInteracting) CancelInteraction();

                Touch touch0 = touches[0];
                Touch touch1 = touches[1];

                Vector2 averageDelta = (touch0.delta + touch1.delta) / 2f;
                if (averageDelta != Vector2.zero)
                {
                    OnPanDelta?.Invoke(averageDelta * 0.05f); 
                }

                Vector2 touch0PrevPos = touch0.screenPosition - touch0.delta;
                Vector2 touch1PrevPos = touch1.screenPosition - touch1.delta;

                float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
                float currentMagnitude = (touch0.screenPosition - touch1.screenPosition).magnitude;
                float deltaMagnitudeDiff = currentMagnitude - prevMagnitude;

                if (Mathf.Abs(deltaMagnitudeDiff) > 0.1f)
                {
                    OnPinchZoom?.Invoke(deltaMagnitudeDiff * 0.01f);
                }
                return;
            }

            if (touchCount == 1)
            {
                Touch touch = touches[0];

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId))
                            return;

                        _isInteracting = true;
                        _interactTouchId = touch.touchId;
                        _lastTouchPos = touch.screenPosition;
                        OnInteractBegan?.Invoke(touch.screenPosition);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_isInteracting && touch.touchId == _interactTouchId)
                        {
                            _lastTouchPos = touch.screenPosition;
                            OnInteractMoved?.Invoke(touch.screenPosition);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (_isInteracting && touch.touchId == _interactTouchId)
                        {
                            CancelInteraction();
                        }
                        break;
                }
            }
            else if (touchCount == 0)
            {
                if (_isInteracting) CancelInteraction();
            }
        }

        private void CancelInteraction()
        {
            if (!_isInteracting) return;
            
            _isInteracting = false;
            _interactTouchId = -1;
            
            OnInteractEnded?.Invoke(_lastTouchPos);
        }

        public void NotifyUICardTapped(Guid cardId) => OnUICardTapped?.Invoke(cardId);
    }
}