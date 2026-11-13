using UnityEngine;

namespace UnderTheRain.Combat
{
    public enum DamageType
    {
        Physical,
        Electric
    }

    public struct DamageContext
    {
        public int amount;
        public Vector2 knockback;
        public DamageType damageType;
        public float hitstopDuration;
        public GameObject source;
    }

    public interface IDamageable
    {
        bool TryTakeDamage(DamageContext context);
    }
}
