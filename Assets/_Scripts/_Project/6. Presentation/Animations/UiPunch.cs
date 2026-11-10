using DG.Tweening;
using UnityEngine;

namespace Game.Presentation
{
    public class UiPunch : MonoBehaviour
    {
        [SerializeField] float _strength = 0.25f;
        [SerializeField] float _duration = 0.3f;

        Vector3 _rest;
        Tween _tween;

        void Awake() => _rest = transform.localScale;

        void OnDestroy() => _tween?.Kill();

        public void Punch()
        {
            _tween?.Kill();
            transform.localScale = _rest;
            _tween = transform.DOPunchScale(Vector3.one * _strength, _duration, 6, 0.8f)
                .SetLink(gameObject);
        }
    }
}