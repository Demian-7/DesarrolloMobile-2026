using UnityEngine;
using TMPro;

namespace Game.Presentation
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private TextMeshPro _priceText;
        [SerializeField] private SpriteRenderer _highlightRenderer;

        [Header("Tiles")]
        [SerializeField] private Sprite _unlockedSprite;
        [SerializeField] private Sprite _lockedSprite;
        [SerializeField, Range(0f, 1f)] private float _lockedAlpha = 0.7f;

        public void Setup(bool isUnlocked, string priceString, Color tint)
        {
            Sprite sprite = isUnlocked ? _unlockedSprite : _lockedSprite;
            if (sprite != null) _renderer.sprite = sprite;

            tint.a = isUnlocked ? 1f : _lockedAlpha;
            _renderer.color = tint;

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