using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Game.Core;
using Game.Foundation;
using DG.Tweening;

namespace Game.Presentation
{
    public class InventoryCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        
        private IInputProvider _inputProvider; 
        private IEventBus _eventBus;
        private Transform _originalParent;
        private Vector3 _originalPosition;

        public Guid CardInstanceId { get; private set; }

        public void Setup(CardInstance card, IInputProvider inputProvider, IEventBus eventBus)
        {
            CardInstanceId = card.InstanceId;
            _backgroundImage.color = card.Definition.IconColor;
            _nameText.text = card.Definition.DisplayName;
            _inputProvider = inputProvider; 
            
            _eventBus = eventBus;
            _eventBus.Subscribe<CardSelectedEvent>(OnSelectionChanged);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
                _eventBus.Unsubscribe<CardSelectedEvent>(OnSelectionChanged);
        }

        private void OnSelectionChanged(CardSelectedEvent evt)
        {
            if (evt.CardInstanceId == CardInstanceId)
            {
                transform.DOScale(Vector3.one * 1.15f, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;
            _inputProvider.NotifyUICardTapped(CardInstanceId);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalParent = transform.parent;
            _originalPosition = transform.position;
            transform.SetParent(GetComponentInParent<Canvas>().transform, true);
            
            transform.DOScale(Vector3.one, 0.1f);
            
            _inputProvider.NotifyUICardDragBegan(CardInstanceId, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            _inputProvider.NotifyUICardDragMoved(CardInstanceId, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _inputProvider.NotifyUICardDragEnded(CardInstanceId);
            bool wasPlaced = _inputProvider.NotifyCardDroppedUI(CardInstanceId, eventData.position);

            if (!wasPlaced)
            {
                transform.SetParent(_originalParent);
                transform.position = _originalPosition;
            }
        }
    }
}