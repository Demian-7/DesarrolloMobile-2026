using DG.Tweening;
using UnityEngine;
using Game.Foundation;

namespace Game.Presentation
{
    [RequireComponent(typeof(Camera))]
    public class BoardCameraController : MonoBehaviour
    {
        private IInputProvider _input;
        private Camera _cam;

        [Header("Settings")]
        [SerializeField] private float _panSpeed = 1.0f;
        [SerializeField] private float _zoomSpeed = 1.5f;
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 15f;

        [Header("Pixel-perfect zoom")]
        [SerializeField] private float _artPixelsPerUnit = 16f;
        [Tooltip("How long after the last pinch before it snaps to a whole-number scale.")]
        [SerializeField] private float _snapDelay = 0.12f;
        [SerializeField] private float _snapSeconds = 0.15f;

        private bool _zoomPending;
        private float _lastZoomTime;
        private int _lastScreenHeight;
        private Tween _snapTween;

        private Vector2 _virtualPos;

        public void Initialize(IInputProvider input)
        {
            _input = input;
            _cam = GetComponent<Camera>();

            _virtualPos = transform.position;

            _input.OnPanDelta += HandlePan;
            _input.OnPinchZoom += HandleZoom;

            _lastScreenHeight = Screen.height;
            SnapNow();
        }

        private void OnDestroy()
        {
            _snapTween?.Kill();

            if (_input != null)
            {
                _input.OnPanDelta -= HandlePan;
                _input.OnPinchZoom -= HandleZoom;
            }
        }

        private void Update()
        {
            if (_cam == null) return;

            if (Screen.height != _lastScreenHeight)
            {
                _lastScreenHeight = Screen.height;
                SnapNow();
                return;
            }

            if (_zoomPending && Time.unscaledTime - _lastZoomTime >= _snapDelay)
            {
                _zoomPending = false;
                SnapSmooth();
            }
        }

        private void LateUpdate()
        {
            if (_cam == null) return;

            float unit = (_cam.orthographicSize * 2f) / Mathf.Max(1, Screen.height);

            Vector3 p = transform.position;
            p.x = Mathf.Round(_virtualPos.x / unit) * unit;
            p.y = Mathf.Round(_virtualPos.y / unit) * unit;
            transform.position = p;
        }

        private float SizeForScale(int scale) => Screen.height / (2f * _artPixelsPerUnit * scale);
        private float ScaleForSize(float size) => Screen.height / (2f * _artPixelsPerUnit * size);

        private int MinScale => Mathf.Max(1, Mathf.CeilToInt(ScaleForSize(_maxZoom)));
        private int MaxScale => Mathf.Max(MinScale, Mathf.FloorToInt(ScaleForSize(_minZoom)));

        private int NearestScale()
            => Mathf.Clamp(Mathf.RoundToInt(ScaleForSize(_cam.orthographicSize)), MinScale, MaxScale);

        private void SnapNow()
        {
            _snapTween?.Kill();
            _cam.orthographicSize = SizeForScale(NearestScale());
        }

        private void SnapSmooth()
        {
            _snapTween?.Kill();
            float target = SizeForScale(NearestScale());
            _snapTween = DOTween.To(() => _cam.orthographicSize, v => _cam.orthographicSize = v,
                    target, _snapSeconds)
                .SetEase(Ease.OutQuad)
                .SetTarget(this);
        }

        private void HandlePan(Vector2 delta)
        {
            float zoomScaledSpeed = _panSpeed * _cam.orthographicSize * 0.05f;

            _virtualPos += new Vector2(-delta.x, -delta.y) * zoomScaledSpeed;
        }

        private void HandleZoom(float delta)
        {
            _snapTween?.Kill();

            float minSize = SizeForScale(MaxScale);
            float maxSize = SizeForScale(MinScale);

            float newSize = _cam.orthographicSize - (delta * _zoomSpeed);
            _cam.orthographicSize = Mathf.Clamp(newSize, minSize, maxSize);

            _zoomPending = true;
            _lastZoomTime = Time.unscaledTime;
        }
    }
}