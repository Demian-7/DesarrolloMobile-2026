using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game.Core;

namespace Game.Presentation
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private GameObject _warningIcon;
        [SerializeField] private LineRenderer _lineRendererPrefab;

        private List<LineRenderer> _activeLines = new();

        public void Setup(CardDefinition definition)
        {
            _backgroundRenderer.color = definition.IconColor;
            _nameText.text = definition.DisplayName;
            _warningIcon.SetActive(false);
        }

        public void ShowWarning(bool show)
        {
            if (_warningIcon != null) _warningIcon.SetActive(show);
        }

        public void DrawLinesToTargets(Vector3 origin, List<Vector3> targetPositions)
        {
            // Clear old lines
            foreach (var line in _activeLines) Destroy(line.gameObject);
            _activeLines.Clear();

            if (_lineRendererPrefab == null) return;

            foreach (var targetPos in targetPositions)
            {
                LineRenderer line = Instantiate(_lineRendererPrefab, transform);
                line.SetPosition(0, origin);
                line.SetPosition(1, targetPos);
                _activeLines.Add(line);
            }
        }
    }
}