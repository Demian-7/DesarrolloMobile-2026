using UnityEngine;
using TMPro;

namespace Game.Presentation
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private TextMeshPro _priceText;
        [SerializeField] private SpriteRenderer _highlightRenderer;
        
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _unlockedColor = Color.white;

        public void Setup(bool isUnlocked, string priceString = "")
        {
            _renderer.color = isUnlocked ? _unlockedColor : _lockedColor;

            if (_priceText != null)
            {
                _priceText.gameObject.SetActive(!isUnlocked);
                _priceText.text = priceString;
            }
        }

        public void SetHighlight(bool active)
        {
            if (_highlightRenderer != null)
            {
                _highlightRenderer.gameObject.SetActive(active);
            }
        }
    }
}