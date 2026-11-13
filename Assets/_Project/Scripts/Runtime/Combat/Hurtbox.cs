using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Receives hits from <see cref="Hitbox"/> colliders and forwards them to the owning <see cref="IDamageable"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hurtbox : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour damageableTarget;

        private IDamageable _damageable;

        private void Awake()
        {
            if (damageableTarget != null)
            {
                _damageable = damageableTarget as IDamageable;
            }

            if (_damageable == null)
            {
                _damageable = GetComponentInParent<IDamageable>();
            }

            if (_damageable == null)
            {
                Debug.LogError("[Hurtbox] No IDamageable found on target or parents.", this);
            }
        }

        public bool TakeDamage(DamageContext context)
        {
            return _damageable != null && _damageable.TryTakeDamage(context);
        }
    }
}
