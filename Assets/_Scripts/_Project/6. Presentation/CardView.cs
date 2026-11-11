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
        [SerializeField] private SpriteRenderer _iconRenderer;
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private LineRenderer _lineRendererPrefab;

        private readonly List<LineRenderer> _activeLines = new();

        private IEventBus _eventBus;
        private bool _isSelected;
        private bool _removing;
        private bool _unpoweredApplied;
        private Vector3 _originalLocalPos;
        private ICardFx _fx;

        public Guid CardInstanceId { get; private set; }
        public ICardFx Fx => _fx;

        public void Setup(CardInstance card, IEventBus eventBus)
        {
            CardInstanceId = card.InstanceId;
            _iconRenderer.sprite = card.Definition.Icon;
            _eventBus = eventBus;

            if (_nameText != null) _nameText.text = card.Definition.DisplayName;

            _fx = GetComponent<ICardFx>();
            _fx?.SetTier(card.Definition.Tier);

            _originalLocalPos = transform.localPosition;
            transform.localScale = Vector3.one;
            _isSelected = false;

            _eventBus.Subscribe<CardSelectedEvent>(OnSelectionChanged);
        }

        public void Setup(CardDefinition definition)
        {
            CardInstanceId = Guid.Empty;
            _iconRenderer.sprite = definition.Icon;
            if (_nameText != null) _nameText.text = definition.DisplayName;

            _fx = GetComponent<ICardFx>();
            _fx?.SetTier(definition.Tier);
        }

        private void OnDestroy()
        {
            Unsubscribe();
            transform.DOKill();
        }

        private void Unsubscribe()
        {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<CardSelectedEvent>(OnSelectionChanged);
            _eventBus = null;
        }

        private void OnSelectionChanged(CardSelectedEvent evt)
        {
            if (_removing) return;

            bool isMe = evt.CardInstanceId == CardInstanceId;

            if (isMe && !_isSelected)
            {
                _isSelected = true;
                transform.DOKill();

                transform.DOScale(Vector3.one * 1.15f, 0.2f).SetEase(Ease.OutBack);

                Vector3 targetPos = new Vector3(_originalLocalPos.x, _originalLocalPos.y + 0.1f,
                    _originalLocalPos.z - 0.5f);
                transform.DOLocalMove(targetPos, 0.2f).SetEase(Ease.OutBack);

                _fx?.SetGlow(1f);
            }
            else if (!isMe && _isSelected)
            {
                _isSelected = false;
                transform.DOKill();

                transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
                transform.DOLocalMove(_originalLocalPos, 0.2f).SetEase(Ease.OutQuad);

                _fx?.SetGlow(0f);
            }
        }

        public void SetUnpowered(bool on)
        {
            if (_removing) return;
            _fx?.SetUnpowered(on, !_unpoweredApplied);
            _unpoweredApplied = true;
        }

        public void PlayRemoval(float duration = 0.3f, float delay = 0f)
        {
            if (_removing) return;
            _removing = true;

            Unsubscribe();
            CardInstanceId = Guid.Empty;
            ClearLines();
            transform.DOKill();

            if (_fx == null)
            {
                Destroy(gameObject, delay + duration);
                return;
            }

            _fx.DissolveOut(duration, delay).OnComplete(() => Destroy(gameObject));
        }

        private void ClearLines()
        {
            foreach (var line in _activeLines)
                if (line != null) Destroy(line.gameObject);
            _activeLines.Clear();
        }

        public void DrawLinesToTargets(Vector3 origin, List<Vector3> targetPositions)
        {
            ClearLines();

            if (_lineRendererPrefab == null || _removing) return;

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