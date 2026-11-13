using System.Collections;
using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Applies tactile combat game feel: brief hitstop (~40-60ms) and subtle camera shake.
    /// </summary>
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private CameraShake2D cameraShake;

        private Coroutine _hitstopRoutine;
        private float _hitstopEndUnscaled;

        public void TriggerHitstop(float duration)
        {
            if (duration <= 0f) return;

            float targetEnd = Time.unscaledTime + duration;
            if (targetEnd > _hitstopEndUnscaled)
            {
                _hitstopEndUnscaled = targetEnd;
            }

            if (_hitstopRoutine == null)
            {
                _hitstopRoutine = StartCoroutine(HitstopRoutine());
            }
        }

        public void TriggerImpact(float hitstopDuration, float shakeIntensity, float shakeDuration)
        {
            TriggerHitstop(hitstopDuration);
            if (cameraShake == null && Camera.main != null)
            {
                cameraShake = Camera.main.GetComponent<CameraShake2D>();
            }
            cameraShake?.TriggerShake(shakeIntensity, shakeDuration);
        }

        private IEnumerator HitstopRoutine()
        {
            float previousScale = Time.timeScale;
            Time.timeScale = 0f;

            while (Time.unscaledTime < _hitstopEndUnscaled)
            {
                yield return null;
            }

            Time.timeScale = previousScale > 0f ? previousScale : 1f;
            _hitstopRoutine = null;
        }

        private void OnDisable()
        {
            if (_hitstopRoutine != null)
            {
                StopCoroutine(_hitstopRoutine);
                _hitstopRoutine = null;
                Time.timeScale = 1f;
            }
        }
    }
}
