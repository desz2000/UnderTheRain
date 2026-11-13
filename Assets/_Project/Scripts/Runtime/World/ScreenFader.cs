using System.Collections;
using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// Controls a CanvasGroup overlay for fast fade-to-black room transitions (~0.25s).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float defaultDuration = 0.25f;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public IEnumerator FadeOut(float duration = -1f)
        {
            yield return Fade(1f, duration > 0f ? duration : defaultDuration);
        }

        public IEnumerator FadeIn(float duration = -1f)
        {
            yield return Fade(0f, duration > 0f ? duration : defaultDuration);
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }

        private IEnumerator Fade(float targetAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }
    }
}
