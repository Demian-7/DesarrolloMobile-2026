using UnityEngine;
using TMPro;
using Game.Core;

namespace Game.Presentation
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private TextMeshPro _nameText;

        public void Setup(CardDefinition definition)
        {
            _backgroundRenderer.color = definition.IconColor;
            _nameText.text = definition.DisplayName;
        }
    }
}