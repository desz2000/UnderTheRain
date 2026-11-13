using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Subtle unscaled-time camera shake that layers cleanly on top of RoomCamera2D.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class CameraShake2D : MonoBehaviour
    {
        private float _timer;
        private float _duration;
        private float _intensity;
        private Vector3 _lastOffset;

        public void TriggerShake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f) return;
            _intensity = Mathf.Max(_intensity, intensity);
            _duration = Mathf.Max(_duration, duration);
            _timer = _duration;
        }

        private void Update()
        {
            if (_lastOffset != Vector3.zero)
            {
                transform.position -= _lastOffset;
                _lastOffset = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            if (_timer <= 0f)
            {
                _intensity = 0f;
                return;
            }

            _timer = Mathf.Max(0f, _timer - Time.unscaledDeltaTime);
            float falloff = _duration > 0f ? _timer / _duration : 0f;
            float amp = _intensity * falloff;

            _lastOffset = new Vector3(
                Random.Range(-amp, amp),
                Random.Range(-amp, amp),
                0f);
            transform.position += _lastOffset;
        }
    }
}
