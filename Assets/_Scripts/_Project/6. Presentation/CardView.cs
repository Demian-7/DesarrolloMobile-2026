using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game.Core;
using Game.Foundation;
using DG.Tweening;

namespace Game.Presentation
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private GameObject _warningIcon;
        [SerializeField] private LineRenderer _lineRendererPrefab;

        private List<LineRenderer> _activeLines = new();
        
        private IEventBus _eventBus;
        private bool _isSelected;
        private Vector3 _originalLocalPos;

        public Guid CardInstanceId { get; private set; }

        public void Setup(CardInstance card, IEventBus eventBus)
        {
            CardInstanceId = card.InstanceId;
            _eventBus = eventBus;
            
            _backgroundRenderer.color = card.Definition.IconColor;
            _nameText.text = card.Definition.DisplayName;
            _warningIcon.SetActive(false);

            _originalLocalPos = transform.localPosition;
            transform.localScale = Vector3.one;
            _isSelected = false;

            _eventBus.Subscribe<CardSelectedEvent>(OnSelectionChanged);
        }

        public void Setup(CardDefinition definition)
        {
            CardInstanceId = Guid.Empty;
            _backgroundRenderer.color = definition.IconColor;
            _nameText.text = definition.DisplayName;
            _warningIcon.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CardSelectedEvent>(OnSelectionChanged);
            }
            transform.DOKill();
        }

        private void OnSelectionChanged(CardSelectedEvent evt)
        {
            bool isMe = evt.CardInstanceId == CardInstanceId;
            
            if (isMe && !_isSelected)
            {
                _isSelected = true;
                transform.DOKill();
                
                // Scale up
                transform.DOScale(Vector3.one * 1.15f, 0.2f).SetEase(Ease.OutBack);
                
                Vector3 targetPos = new Vector3(_originalLocalPos.x, _originalLocalPos.y + 0.1f, _originalLocalPos.z - 0.5f);
                transform.DOLocalMove(targetPos, 0.2f).SetEase(Ease.OutBack);
            }
            else if (!isMe && _isSelected)
            {
                _isSelected = false;
                transform.DOKill();
                
                transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
                transform.DOLocalMove(_originalLocalPos, 0.2f).SetEase(Ease.OutQuad);
            }
        }

        public void ShowWarning(bool show)
        {
            if (_warningIcon != null) _warningIcon.SetActive(show);
        }

        public void DrawLinesToTargets(Vector3 origin, List<Vector3> targetPositions)
        {
            foreach (var line in _activeLines) Destroy(line.gameObject);
            _activeLines.Clear();

            if (_lineRendererPrefab == null) return;

            Vector3 offsetOrigin = new Vector3(origin.x, origin.y, origin.z + 0.5f);

            foreach (var targetPos in targetPositions)
            {
                LineRenderer line = Instantiate(_lineRendererPrefab, transform);
                Vector3 offsetTarget = new Vector3(targetPos.x, targetPos.y, targetPos.z + 0.5f);
                
                line.SetPosition(0, offsetOrigin);
                line.SetPosition(1, offsetTarget);
                _activeLines.Add(line);
            }
        }
    }
}