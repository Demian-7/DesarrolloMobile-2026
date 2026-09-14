using System;
using System.Collections.Generic;
using Game.Foundation;
using Game.Core;
using UnityEngine;

namespace Game.Input.Desktop
{
    public class DesktopInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<Vector2> OnScreenTapped;
        
        public event Func<Guid, Vector2, bool> OnCardDropped; 
        
        public event Action<float> OnPinchZoom;
        public event Action<Vector2> OnPanDelta;

        private Queue<Guid> _mockCardQueue = new Queue<Guid>();

        public void SetMockCardQueue(List<CardInstance> cards)
        {
            _mockCardQueue.Clear();
            foreach (var card in cards)
            {
                _mockCardQueue.Enqueue(card.InstanceId);
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnScreenTapped?.Invoke(UnityEngine.Input.mousePosition);
            }

            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (_mockCardQueue.Count > 0)
                {
                    Guid nextCardId = _mockCardQueue.Peek();
                    
                    bool wasPlaced = OnCardDropped?.Invoke(nextCardId, UnityEngine.Input.mousePosition) ?? false;
                    
                    if (wasPlaced)
                    {
                        _mockCardQueue.Dequeue();
                    }
                }
                else
                {
                    Debug.LogWarning("Mock inventory empty! No more cards to drop.");
                }
            }
        }
    }
}