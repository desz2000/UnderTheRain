using System.Collections;
using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Simple target dummy for validating melee damage, knockback, hitstop and recovery.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private float knockbackDamping = 12f;

        private HealthComponent _health;
        private Rigidbody2D _body;
        private Color _baseColor;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _body = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) _baseColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= HandleDied;
        }

        private void FixedUpdate()
        {
            if (_body != null)
            {
                _body.linearVelocity = Vector2.Lerp(_body.linearVelocity, Vector2.zero, knockbackDamping * Time.fixedDeltaTime);
            }
        }

        private void HandleDamaged(DamageContext ctx)
        {
            if (_body != null)
            {
                _body.linearVelocity = ctx.knockback;
            }

            if (spriteRenderer != null)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(FlashRoutine());
            }
        }

        private void HandleDied()
        {
            // Dummy resets to full health so playtesters can keep testing combos.
            _health.RestoreFull();
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSecondsRealtime(flashDuration);
            spriteRenderer.color = _baseColor;
            _flashRoutine = null;
        }
    }
}
