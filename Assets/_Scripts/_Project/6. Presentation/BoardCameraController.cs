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

        public void Initialize(IInputProvider input)
        {
            _input = input;
            _cam = GetComponent<Camera>();
            
            _input.OnPanDelta += HandlePan;
            _input.OnPinchZoom += HandleZoom;
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnPanDelta -= HandlePan;
                _input.OnPinchZoom -= HandleZoom;
            }
        }

        private void HandlePan(Vector2 delta)
        {
            float zoomScaledSpeed = _panSpeed * _cam.orthographicSize * 0.05f; 

            Vector3 movement = new Vector3(-delta.x, -delta.y, 0) * zoomScaledSpeed;
            transform.Translate(movement, Space.World);
        }

        private void HandleZoom(float delta)
        {
            float newSize = _cam.orthographicSize - (delta * _zoomSpeed);
            _cam.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
        }
    }
}