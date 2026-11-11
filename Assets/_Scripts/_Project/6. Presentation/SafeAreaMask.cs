using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaMask : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.black;

        private RectTransform _left, _right, _top, _bottom;
        private Rect _lastSafeArea;
        private int _lastWidth, _lastHeight;

        private void Awake()
        {
            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            transform.SetAsLastSibling();

            _left = CreateStrip("Mask_Left");
            _right = CreateStrip("Mask_Right");
            _top = CreateStrip("Mask_Top");
            _bottom = CreateStrip("Mask_Bottom");

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

        private RectTransform CreateStrip(string stripName)
        {
            var go = new GameObject(stripName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var img = go.GetComponent<Image>();
            img.color = _color;
            img.raycastTarget = false;

            return (RectTransform)go.transform;
        }

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect sa = Screen.safeArea;
            float xMin = sa.xMin / Screen.width;
            float xMax = sa.xMax / Screen.width;
            float yMin = sa.yMin / Screen.height;
            float yMax = sa.yMax / Screen.height;

            Place(_left, 0f, 0f, xMin, 1f);
            Place(_right, xMax, 0f, 1f, 1f);
            Place(_bottom, 0f, 0f, 1f, yMin);
            Place(_top, 0f, yMax, 1f, 1f);

            _lastSafeArea = sa;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
        }

        private static void Place(RectTransform r, float x0, float y0, float x1, float y1)
        {
            r.anchorMin = new Vector2(x0, y0);
            r.anchorMax = new Vector2(x1, y1);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.gameObject.SetActive(x1 - x0 > 0.0001f && y1 - y0 > 0.0001f);
        }
    }
}