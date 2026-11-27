using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Presentation
{
    public enum LabelFlash { None, Gain, GainFinal, Spend }

    public class AnimatedLabel : MonoBehaviour
    {
        [SerializeField] TMP_Text label;

        [Header("Gain (each rate step)")]
        [SerializeField] float gainScale = 0.18f;
        [SerializeField] float gainHop = 5f;
        [SerializeField] Color gainColor = new Color(1f, 0.85f, 0.3f);
        [Tooltip("Optional TMPEffects tag on every step. Leave empty.")]
        [SerializeField] string gainTag = "";

        [Header("Gain final: one-shot jump, character by character")]
        [Tooltip("Jump height as a fraction of the font size.")]
        [SerializeField] float jumpHeightEm = 0.3f;
        [SerializeField] float jumpCharSeconds = 0.25f;
        [SerializeField] float jumpStaggerSeconds = 0.05f;

        [Header("Spend (purchases)")]
        [SerializeField] float spendScale = 0.08f;
        [SerializeField] Color spendColor = new Color(1f, 0.4f, 0.4f);
        [SerializeField] string spendTag = "";

        [Header("Shared")]
        [SerializeField] float effectSeconds = 0.3f;
        [SerializeField] string boostColor = "#FFD24A";

        private RectTransform rect;
        private Color baseColor;
        private Sequence fx;
        private Vector3 restScale;
        private Vector2 restPos;

        private string plain = "";
        private string activeTag;
        private bool boost;
        private Tween tagTimer;
        
        private bool jumping;
        private float jumpTime;
        private Tween jumpTween;

        private void Reset() => label = GetComponent<TMP_Text>();

        private void Awake() => Setup();

        private void OnDestroy()
        {
            fx?.Kill();
            tagTimer?.Kill();
            jumpTween?.Kill();
        }

        private void Setup()
        {
            if (rect != null) return;
            rect = (RectTransform)transform;
            if (label == null) label = GetComponent<TMP_Text>();
            baseColor = label.color;
        }

        public void SetValue(string text, LabelFlash flash = LabelFlash.None)
        {
            Setup();
            plain = text;

            if (flash != LabelFlash.None)
            {
                Play(flash);

                activeTag = flash == LabelFlash.Gain ? gainTag
                          : flash == LabelFlash.Spend ? spendTag
                          : null;

                tagTimer?.Kill();
                if (!string.IsNullOrEmpty(activeTag))
                {
                    tagTimer = DOVirtual.DelayedCall(effectSeconds, () =>
                    {
                        activeTag = null;
                        Render();
                    }).SetLink(gameObject);
                }

                if (flash == LabelFlash.GainFinal)
                    StartCharJump();
            }

            Render();
        }

        public void SetBoost(bool on)
        {
            Setup();
            boost = on;
            Render();
        }

        private void StartCharJump()
        {
            if (jumping) return;

            int n = Mathf.Max(1, CountJumpChars(plain));
            float total = jumpCharSeconds + (n - 1) * jumpStaggerSeconds;

            jumping = true;
            jumpTime = 0f;
            jumpTween = DOVirtual.Float(0f, total, total, t =>
                {
                    jumpTime = t;
                    Render();
                })
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    jumping = false;
                    Render();
                });
        }

        private static int CountJumpChars(string s)
        {
            int count = 0;
            bool inTag = false;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\n') break;
                if (c == '<') { inTag = true; continue; }
                if (inTag) { if (c == '>') inTag = false; continue; }
                count++;
            }

            return count;
        }

        private string BuildJump(string s)
        {
            var sb = new StringBuilder(s.Length * 24);
            float height = label.fontSize * jumpHeightEm;
            bool inTag = false;
            int visibleIndex = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (c == '\n')
                {
                    sb.Append(s, i, s.Length - i);
                    break;
                }

                if (c == '<')
                {
                    inTag = true;
                    sb.Append(c);
                    continue;
                }

                if (inTag)
                {
                    sb.Append(c);
                    if (c == '>') inTag = false;
                    continue;
                }

                float u = (jumpTime - visibleIndex * jumpStaggerSeconds) / jumpCharSeconds;
                int y = 0;
                if (u > 0f && u < 1f)
                {
                    float k = 2f * u - 1f;
                    y = Mathf.RoundToInt(height * (1f - k * k));
                }

                if (y != 0)
                    sb.Append("<voffset=").Append(y).Append('>').Append(c).Append("</voffset>");
                else
                    sb.Append(c);

                visibleIndex++;
            }

            return sb.ToString();
        }

        private void Play(LabelFlash flash)
        {
            bool final = flash == LabelFlash.GainFinal;
            bool gain = flash == LabelFlash.Gain || final;

            float scale = final ? 0f : (gain ? gainScale : spendScale);
            float hop = final ? 0f : (gain ? gainHop : 0f);
            Color flashColor = gain ? gainColor : spendColor;
            float up = effectSeconds * 0.3f;
            float down = effectSeconds * 0.7f;

            if (fx != null && fx.IsActive())
                fx.Kill();
            else
            {
                restScale = rect.localScale;
                restPos = rect.anchoredPosition;
            }

            fx = DOTween.Sequence().SetLink(gameObject);

            fx.Append(rect.DOScale(restScale * (1f + scale), up).SetEase(Ease.OutQuad));
            fx.Join(DOTween.To(() => rect.anchoredPosition, v => rect.anchoredPosition = v,
                restPos + Vector2.up * hop, up).SetEase(Ease.OutQuad));
            fx.Join(DOTween.To(() => label.color, c => label.color = c, flashColor, up));

            fx.Append(rect.DOScale(restScale, down).SetEase(Ease.OutBounce));
            fx.Join(DOTween.To(() => rect.anchoredPosition, v => rect.anchoredPosition = v,
                restPos, down).SetEase(Ease.OutBounce));
            fx.Join(DOTween.To(() => label.color, c => label.color = c, baseColor, down));
        }

        private void Render()
        {
            string s = jumping ? BuildJump(plain) : plain;
            if (boost) s = $"<color={boostColor}><wave>{s}</wave></color>";
            if (!string.IsNullOrEmpty(activeTag))
            {
                string tagName = activeTag.Split(' ')[0];
                s = $"<{activeTag}>{s}</{tagName}>";
            }
            label.text = s;
        }
    }
}