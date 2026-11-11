using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ZoneBackground : MonoBehaviour
    {
        [Serializable]
        private class ZonePalette
        {
            public string zoneId;
            public Color dark = Color.black;
            public Color mid = Color.gray;
            public Color accent = Color.white;
        }

        static readonly int DarkId = Shader.PropertyToID("_ColorDark");
        static readonly int MidId = Shader.PropertyToID("_ColorMid");
        static readonly int AccentId = Shader.PropertyToID("_ColorAccent");
        static readonly int PixelRowsId = Shader.PropertyToID("_PixelRows");
        static readonly int AspectId = Shader.PropertyToID("_Aspect");
        static readonly int SwirlId = Shader.PropertyToID("_Swirl");
        static readonly int PulseId = Shader.PropertyToID("_Pulse");
        static readonly int ParallaxId = Shader.PropertyToID("_Parallax");
        static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        static readonly int SpriteSizeId = Shader.PropertyToID("_SpriteSize");

        [Header("Palettes")]
        [SerializeField] private ZonePalette[] _palettes;

        [Header("Placement")]
        [SerializeField] private Camera _camera;
        [Tooltip("Distance from the camera. Must be farther than the board.")]
        [SerializeField] private float _depth = 30f;

        [Header("Look")]
        [Tooltip("Background pixels from top to bottom of the screen. The pixel size on screen " +
                 "is fixed: pan and zoom never change it.")]
        [SerializeField] private float _targetRows = 90f;
        [SerializeField, Range(0f, 1f)] private float _brightness = 0.8f;
        [SerializeField] private float _swirl = 1f;

        [Header("Pulse")]
        [SerializeField] private float _pulseKick = 0.6f;
        [SerializeField] private float _pulseDecayPerSecond = 2f;

        private SpriteRenderer _sr;
        private MaterialPropertyBlock _block;
        private Color _dark, _mid, _accent;
        private float _pulse;
        private Tween _paletteTween;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _block = new MaterialPropertyBlock();

            if (_camera == null) _camera = Camera.main;
            transform.SetParent(_camera.transform, false);

            if (_palettes != null && _palettes.Length > 0)
            {
                _dark = _palettes[0].dark;
                _mid = _palettes[0].mid;
                _accent = _palettes[0].accent;
            }
        }

        private void OnDestroy() => _paletteTween?.Kill();

        public void ShowZone(string zoneId, float duration)
        {
            ZonePalette target = null;
            foreach (var p in _palettes)
            {
                if (p.zoneId == zoneId) { target = p; break; }
            }
            if (target == null) return;

            _paletteTween?.Kill();

            if (duration <= 0f)
            {
                _dark = target.dark; _mid = target.mid; _accent = target.accent;
                return;
            }

            Color d0 = _dark, m0 = _mid, a0 = _accent;
            float blend = 0f;
            _paletteTween = DOTween.To(() => blend, v =>
            {
                blend = v;
                _dark = Color.Lerp(d0, target.dark, v);
                _mid = Color.Lerp(m0, target.mid, v);
                _accent = Color.Lerp(a0, target.accent, v);
            }, 1f, duration).SetEase(Ease.InOutSine).SetLink(gameObject);
        }

        public Color GetMidColor(string zoneId)
        {
            foreach (var p in _palettes)
            {
                if (p.zoneId == zoneId) return p.mid;
            }
            return Color.white;
        }

        public void Kick() => _pulse = Mathf.Min(1f, _pulse + _pulseKick);

        public void SetEffectsEnabled(bool on)
        {
            _sr.enabled = on;
            if (!on) _camera.backgroundColor = _dark;
        }

        private void LateUpdate()
        {
            if (!_sr.enabled) return;

            float height = _camera.orthographic
                ? _camera.orthographicSize * 2f
                : 2f * _depth * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * _camera.aspect;

            Vector2 spriteSize = _sr.sprite.bounds.size;
            transform.localPosition = new Vector3(0f, 0f, _depth);
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);

            _pulse = Mathf.MoveTowards(_pulse, 0f, _pulseDecayPerSecond * Time.deltaTime);

            int screenH = Mathf.Max(1, Screen.height);
            int pixelSize = Mathf.Max(1, Mathf.RoundToInt(screenH / _targetRows));
            float rows = (float)screenH / pixelSize;

            _sr.GetPropertyBlock(_block);
            _block.SetColor(DarkId, _dark);
            _block.SetColor(MidId, _mid);
            _block.SetColor(AccentId, _accent);
            _block.SetFloat(PixelRowsId, rows);
            _block.SetFloat(AspectId, width / height);
            _block.SetFloat(SwirlId, _swirl);
            _block.SetFloat(PulseId, _pulse);
            _block.SetVector(ParallaxId, Vector2.zero);
            _block.SetFloat(BrightnessId, _brightness);
            _block.SetVector(SpriteSizeId, spriteSize);
            _sr.SetPropertyBlock(_block);
        }
    }
}