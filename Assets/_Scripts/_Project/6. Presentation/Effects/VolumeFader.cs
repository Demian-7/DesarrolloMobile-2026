using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Presentation
{
    public class VolumeFader : MonoBehaviour
    {
        [SerializeField] Volume _volume;
        Tween _tween;

        void Reset() => _volume = GetComponent<Volume>();

        public void FadeTo(float weight, float duration = 1.2f)
        {
            _tween?.Kill();
            _tween = DOTween.To(() => _volume.weight, w => _volume.weight = w, weight, duration)
                .SetEase(Ease.InOutSine);
        }
    }
}