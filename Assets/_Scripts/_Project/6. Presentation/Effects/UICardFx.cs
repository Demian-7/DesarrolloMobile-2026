using DG.Tweening;
using Game.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    [RequireComponent(typeof(Image))]
    public class UICardFx : MonoBehaviour, ICardFx
    {
        static readonly int GlowId = Shader.PropertyToID("_Glow");
        static readonly int ShineId = Shader.PropertyToID("_ShineStrength");
        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        static readonly int ShineScaleId = Shader.PropertyToID("_ShineScale");
        static readonly int PixelSizeId = Shader.PropertyToID("_PixelSize");

        [SerializeField] int _artPixels = 16;

        Image _image;
        Material _mat;

        Material Mat
        {
            get
            {
                if (_mat == null)
                {
                    _image = GetComponent<Image>();
                    _mat = new Material(_image.material);
                    _image.material = _mat;
                    UpdateSizeDependentValues();
                }

                return _mat;
            }
        }

        void Awake()
        {
            _ = Mat;
        }

        void OnRectTransformDimensionsChange()
        {
            if (_mat != null) UpdateSizeDependentValues();
        }

        void OnDestroy()
        {
            DOTween.Kill(this);
            if (_mat != null) Destroy(_mat);
        }

        void UpdateSizeDependentValues()
        {
            var rect = ((RectTransform)transform).rect;
            float size = Mathf.Max(rect.width, rect.height);
            if (size <= 0f) return;
            _mat.SetFloat(ShineScaleId, 2f / size);
            _mat.SetFloat(PixelSizeId, size / _artPixels);
        }

        public void SetTier(CardTier tier) => Mat.SetFloat(ShineId, tier == CardTier.Tier3 ? 1f : 0f);

        public void SetGlow(float value) => Mat.SetFloat(GlowId, value);

        public void SetUnpowered(bool on, bool instant = false) { }

        public Tween Pulse()
        {
            float start = Mat.GetFloat(GlowId);
            return DOTween.Sequence().SetTarget(this)
                .Append(Mat.DOFloat(2f, GlowId, 0.08f))
                .Append(Mat.DOFloat(start, GlowId, 0.25f));
        }

        public Tween DissolveIn(float duration = 0.5f, float delay = 0f)
        {
            Mat.SetFloat(DissolveId, 1f);
            return Mat.DOFloat(0f, DissolveId, duration).SetDelay(delay).SetTarget(this);
        }

        public Tween DissolveOut(float duration = 0.5f, float delay = 0f)
            => Mat.DOFloat(1f, DissolveId, duration).SetDelay(delay).SetTarget(this);
    }
}