using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Game.Core;
using Game.Foundation;
using Game.Input.Desktop;

namespace Game.Presentation
{
    public class InventoryCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        
        private DesktopInputProvider _inputProvider;
        private Transform _originalParent;
        private Vector3 _originalPosition;

        public Guid CardInstanceId { get; private set; }

        public void Setup(CardInstance card, IInputProvider inputProvider)
        {
            CardInstanceId = card.InstanceId;
            _backgroundImage.color = card.Definition.IconColor;
            _nameText.text = card.Definition.DisplayName;
            _inputProvider = inputProvider as DesktopInputProvider;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalParent = transform.parent;
            _originalPosition = transform.position;
            
            transform.SetParent(GetComponentInParent<Canvas>().transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            bool wasPlaced = _inputProvider.NotifyCardDroppedUI(CardInstanceId, eventData.position);

            if (!wasPlaced)
            {
                transform.SetParent(_originalParent);
                transform.position = _originalPosition;
            }
        }
    }
}