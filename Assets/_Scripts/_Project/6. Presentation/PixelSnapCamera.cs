using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Presentation
{
    [RequireComponent(typeof(Camera))]
    public class PixelSnapCamera : MonoBehaviour
    {
        [SerializeField] private float _artPixelsPerUnit = 16f;

        private Camera _cam;
        private Vector3 _savedPos;
        private bool _snapped;

        private void Awake() => _cam = GetComponent<Camera>();

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnEndCamera;
        }

        private void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            if (cam != _cam) return;

            _savedPos = transform.position;
            float unit = 1f / _artPixelsPerUnit;
            transform.position = new Vector3(
                Mathf.Round(_savedPos.x / unit) * unit,
                Mathf.Round(_savedPos.y / unit) * unit,
                _savedPos.z);
            _snapped = true;
        }

        private void OnEndCamera(ScriptableRenderContext ctx, Camera cam)
        {
            if (!_snapped || cam != _cam) return;
            transform.position = _savedPos;
            _snapped = false;
        }
    }
}