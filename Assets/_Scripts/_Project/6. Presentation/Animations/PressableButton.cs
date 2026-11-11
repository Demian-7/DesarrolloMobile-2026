using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Presentation
{
    public class PressableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] float _pressedScale = 0.92f;

        Vector3 _rest;
        Tween _tween;

        void Awake() => _rest = transform.localScale;

        void OnDestroy() => _tween?.Kill();

        public void OnPointerDown(PointerEventData e) => To(_rest * _pressedScale, 0.06f, Ease.OutQuad);
        public void OnPointerUp(PointerEventData e) => To(_rest, 0.25f, Ease.OutBack);
        public void OnPointerExit(PointerEventData e) => To(_rest, 0.25f, Ease.OutBack);

        void To(Vector3 target, float time, Ease ease)
        {
            _tween?.Kill();
            _tween = transform.DOScale(target, time).SetEase(ease).SetUpdate(true).SetLink(gameObject);
        }
    }
}