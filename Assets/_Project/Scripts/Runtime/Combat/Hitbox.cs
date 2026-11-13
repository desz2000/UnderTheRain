using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Trigger-based attack volume. Tracks targets hit per activation so one swing
    /// never damages the same <see cref="Hurtbox"/> twice.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hitbox : MonoBehaviour
    {
        [Tooltip("When true, hitbox stays active continuously (e.g. environmental hazards or enemy body contact).")]
        [SerializeField] private bool alwaysActive;
        [SerializeField] private DamageContext passiveDamage = new DamageContext { amount = 1, knockback = new Vector2(6.5f, 4f), hitstopDuration = 0.04f };

        private readonly HashSet<Hurtbox> _hitThisSwing = new HashSet<Hurtbox>();
        private Collider2D _collider;
        private DamageContext _currentContext;
        private bool _isActive;

        public event Action<DamageContext> OnHitLanded;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            _isActive = alwaysActive;
            _collider.enabled = alwaysActive;
            if (alwaysActive && passiveDamage.source == null)
            {
                passiveDamage.source = gameObject;
                _currentContext = passiveDamage;
            }
        }

        public void Activate(DamageContext context)
        {
            _hitThisSwing.Clear();
            _currentContext = context;
            _isActive = true;
            if (_collider != null) _collider.enabled = true;
        }

        public void Deactivate()
        {
            _isActive = false;
            if (_collider != null) _collider.enabled = false;
            _hitThisSwing.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Passive hazards/contact hitboxes can re-attempt once invulnerability expires.
            if (!alwaysActive) return;
            TryHit(other, trackPerSwing: false);
        }

        private void TryHit(Collider2D other, bool trackPerSwing = true)
        {
            if (!_isActive) return;
            if (!other.TryGetComponent(out Hurtbox hurtbox)) return;
            if (trackPerSwing && !_hitThisSwing.Add(hurtbox)) return;

            DamageContext ctx = alwaysActive ? passiveDamage : _currentContext;
            if (ctx.source != null && ctx.knockback.x != 0f)
            {
                float dir = Mathf.Sign(other.transform.position.x - ctx.source.transform.position.x);
                if (dir == 0f) dir = 1f;
                ctx.knockback.x = Mathf.Abs(ctx.knockback.x) * dir;
            }

            if (hurtbox.TakeDamage(ctx))
            {
                OnHitLanded?.Invoke(ctx);
            }
        }
    }
}
