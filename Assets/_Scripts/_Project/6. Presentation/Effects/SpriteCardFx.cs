using DG.Tweening;
using Game.Foundation;
using UnityEngine;

namespace Game.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteCardFx : MonoBehaviour, ICardFx
    {
        static readonly int GlowId = Shader.PropertyToID("_Glow");
        static readonly int ShineId = Shader.PropertyToID("_ShineStrength");
        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        static readonly int ShineScaleId = Shader.PropertyToID("_ShineScale");
        static readonly int PixelSizeId = Shader.PropertyToID("_PixelSize");
        static readonly int DesatId = Shader.PropertyToID("_Desaturate");

        [Header("Pulse")]
        [SerializeField] float _pulseGlow = 2.5f;
        [SerializeField] float _pulseScale = 0.3f;
        [SerializeField] float _pulseHop = 0.15f;
        [SerializeField] float _pulseUpTime = 0.1f;
        [SerializeField] float _pulseDownTime = 0.4f;

        [Header("Unpowered")]
        [SerializeField] float _unpoweredFadeSeconds = 0.25f;

        SpriteRenderer _sr;
        MaterialPropertyBlock _block;
        float _glow, _shine, _dissolve, _pulseGlowAmount, _desat;

        bool _unpowered;
        Tween _desatTween;

        Sequence _pulse;
        Vector3 _restScale, _restPos;

        void Init()
        {
            if (_sr != null) return;
            _sr = GetComponent<SpriteRenderer>();
            _block = new MaterialPropertyBlock();
        }   

        void Awake() => Init();

        void OnDestroy() => DOTween.Kill(this);

        public void Refresh() => Apply();

        void Apply()
        {
            Init();
            _sr.GetPropertyBlock(_block);
            _block.SetFloat(GlowId, _glow + _pulseGlowAmount);
            _block.SetFloat(ShineId, _shine);
            _block.SetFloat(DissolveId, _dissolve);
            _block.SetFloat(DesatId, _desat);
    
            var sprite = _sr.sprite;
            if (sprite != null)
            {
                float size = sprite.rect.width / sprite.pixelsPerUnit;
                _block.SetFloat(ShineScaleId, 2f / size);
                _block.SetFloat(PixelSizeId, 1f / sprite.pixelsPerUnit);
            }   

            _sr.SetPropertyBlock(_block);
        }   

        public void SetTier(CardTier tier)
        {
            _shine = tier == CardTier.Tier3 ? 1f : 0f;
            Apply();
        }

        public void SetGlow(float value)
        {
            _glow = value;
            Apply();
        }

        public void SetUnpowered(bool on, bool instant = false)
        {
            if (on == _unpowered && !instant) return;
            _unpowered = on;

            float target = on ? 1f : 0f;
            _desatTween?.Kill();

            if (instant)
            {
                _desat = target;
                Apply();
                return;
            }

            _desatTween = DOTween.To(() => _desat, v => { _desat = v; Apply(); },
                target, _unpoweredFadeSeconds).SetTarget(this);
        }

        public Tween Pulse()
        {
            if (_pulse != null && _pulse.IsActive())
            {
                _pulse.Kill();
                transform.localScale = _restScale;
                transform.localPosition = _restPos;
            }

            _restScale = transform.localScale;
            _restPos = transform.localPosition;
            _pulseGlowAmount = 0f;

            _pulse = DOTween.Sequence().SetTarget(this);

            _pulse.Append(DOTween.To(() => _pulseGlowAmount, v => { _pulseGlowAmount = v; Apply(); },
                _pulseGlow, _pulseUpTime).SetEase(Ease.OutQuad));
            _pulse.Join(transform.DOScale(_restScale * (1f + _pulseScale), _pulseUpTime).SetEase(Ease.OutQuad));
            _pulse.Join(transform.DOLocalMove(_restPos + Vector3.up * _pulseHop, _pulseUpTime).SetEase(Ease.OutQuad));

            _pulse.Append(DOTween.To(() => _pulseGlowAmount, v => { _pulseGlowAmount = v; Apply(); },
                0f, _pulseDownTime).SetEase(Ease.InQuad));
            _pulse.Join(transform.DOScale(_restScale, _pulseDownTime).SetEase(Ease.OutBounce));
            _pulse.Join(transform.DOLocalMove(_restPos, _pulseDownTime).SetEase(Ease.OutBounce));

            _pulse.OnKill(() =>
            {
                _pulseGlowAmount = 0f;
                if (_sr != null) Apply();
            });

            return _pulse;
        }

        public Tween DissolveIn(float duration = 0.5f, float delay = 0f)
        {
            _dissolve = 1f;
            Apply();
            return DOTween.To(() => _dissolve, v =>
            {
                _dissolve = v;
                Apply();
            }, 0f, duration).SetDelay(delay).SetTarget(this);
        }

        public Tween DissolveOut(float duration = 0.5f, float delay = 0f)
            => DOTween.To(() => _dissolve, v =>
            {
                _dissolve = v;
                Apply();
            }, 1f, duration).SetDelay(delay).SetTarget(this);
    }
}