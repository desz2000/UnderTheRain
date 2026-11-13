using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Electric bolt fired by Dante's battery weapon. Travels straight and destroys on hurtbox or solid wall impact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class BatteryProjectile : MonoBehaviour
    {
        private Vector2 _velocity;
        private DamageContext _damage;
        private LayerMask _solidMask;
        private float _lifeTimer;
        private bool _consumed;

        public void Launch(Vector2 velocity, DamageContext damage, LayerMask solidMask, float lifetime = 1.8f)
        {
            _velocity = velocity;
            _damage = damage;
            _solidMask = solidMask;
            _lifeTimer = lifetime;
            _consumed = false;
        }

        private void Update()
        {
            transform.position += (Vector3)(_velocity * Time.deltaTime);
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;

            if (other.TryGetComponent(out Hurtbox hurtbox))
            {
                if (hurtbox.TakeDamage(_damage))
                {
                    _consumed = true;
                    Destroy(gameObject);
                }
                return;
            }

            if ((_solidMask.value & (1 << other.gameObject.layer)) != 0)
            {
                _consumed = true;
                Destroy(gameObject);
            }
        }
    }
}
