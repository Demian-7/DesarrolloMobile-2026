using UnityEngine;

namespace Game.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool _applyLeft = true;
        [SerializeField] private bool _applyRight = true;
        [SerializeField] private bool _applyTop = true;
        [SerializeField] private bool _applyBottom = true;

        private RectTransform _rect;
        private Rect _lastSafeArea;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastWidth ||
                Screen.height != _lastHeight)
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect sa = Screen.safeArea;

            float xMin = _applyLeft ? sa.xMin / Screen.width : 0f;
            float xMax = _applyRight ? sa.xMax / Screen.width : 1f;
            float yMin = _applyBottom ? sa.yMin / Screen.height : 0f;
            float yMax = _applyTop ? sa.yMax / Screen.height : 1f;

            _rect.anchorMin = new Vector2(xMin, yMin);
            _rect.anchorMax = new Vector2(xMax, yMax);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;

            _lastSafeArea = sa;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
        }
    }
}