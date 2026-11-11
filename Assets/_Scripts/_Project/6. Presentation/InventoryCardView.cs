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
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;

        private IInputProvider _inputProvider;
        private IEventBus _eventBus;
        private Transform _originalParent;
        private Vector3 _originalPosition;
        private int _originalSiblingIndex;
        private RectTransform _dragCanvasRect;
        private ICardFx _fx;

        public Guid CardInstanceId { get; private set; }
        public ICardFx Fx => _fx;

        public void Setup(CardInstance card, IInputProvider inputProvider, IEventBus eventBus)
        {
            CardInstanceId = card.InstanceId;
            _iconImage.sprite = card.Definition.Icon;
            if (_nameText != null) _nameText.text = card.Definition.DisplayName;
            _inputProvider = inputProvider;

            _fx = _iconImage.GetComponent<ICardFx>();
            _fx?.SetTier(card.Definition.Tier);

            _eventBus = eventBus;
            _eventBus.Subscribe<CardSelectedEvent>(OnSelectionChanged);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
                _eventBus.Unsubscribe<CardSelectedEvent>(OnSelectionChanged);

            transform.DOKill();
        }

        private void OnSelectionChanged(CardSelectedEvent evt)
        {
            transform.DOKill();

            if (evt.CardInstanceId == CardInstanceId)
            {
                transform.DOScale(Vector3.one * 1.15f, 0.2f).SetEase(Ease.OutBack);
                _fx?.SetGlow(1f);
            }
            else
            {
                transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
                _fx?.SetGlow(0f);
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
            _originalSiblingIndex = transform.GetSiblingIndex();

            _dragCanvasRect = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
            transform.SetParent(_dragCanvasRect, true);
            transform.SetAsLastSibling();

            transform.DOScale(Vector3.one, 0.1f);

            MoveToPointer(eventData);
            _inputProvider.NotifyUICardDragBegan(CardInstanceId, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveToPointer(eventData);
            _inputProvider.NotifyUICardDragMoved(CardInstanceId, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _inputProvider.NotifyUICardDragEnded(CardInstanceId);
            bool wasPlaced = _inputProvider.NotifyCardDroppedUI(CardInstanceId, eventData.position);

            if (!wasPlaced)
            {
                transform.SetParent(_originalParent);
                transform.SetSiblingIndex(_originalSiblingIndex);
                transform.position = _originalPosition;
            }
        }

        private void MoveToPointer(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _dragCanvasRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
            {
                transform.position = worldPoint;
            }
        }
    }
}