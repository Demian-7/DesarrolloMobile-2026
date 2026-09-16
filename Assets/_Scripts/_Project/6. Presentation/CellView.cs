using UnityEngine;

namespace Game.Presentation
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _unlockedColor = Color.white;

        public void Setup(bool isUnlocked)
        {
            _renderer.color = isUnlocked ? _unlockedColor : _lockedColor;
        }
    }
}