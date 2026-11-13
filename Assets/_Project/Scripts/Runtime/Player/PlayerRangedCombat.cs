using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Fires a battery-powered electric projectile when Dante has enough charge.
    /// </summary>
    public sealed class PlayerRangedCombat : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private BatteryComponent battery;
        [SerializeField] private BatteryProjectile projectilePrefab;
        [SerializeField] private LayerMask solidMask;

        [Header("Tuning")]
        [SerializeField, Min(1f)] private float shotCost = 5f;
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(1f)] private float projectileSpeed = 16f;
        [SerializeField, Min(0.05f)] private float fireCooldown = 0.25f;
        [SerializeField] private Vector2 knockback = new Vector2(2f, 0.5f);

        private float _cooldownTimer;

        public event System.Action OnShotFired;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (battery == null) battery = GetComponent<BatteryComponent>();
        }

        private void OnEnable()
        {
            if (input != null) input.OnShootPressed += HandleShootPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnShootPressed -= HandleShootPressed;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer = Mathf.Max(0f, _cooldownTimer - Time.deltaTime);
        }

        private void HandleShootPressed()
        {
            TryFire();
        }

        public bool TryFire()
        {
            if (_cooldownTimer > 0f || projectilePrefab == null || battery == null) return false;
            if (!battery.TryConsume(shotCost))
            {
                Debug.Log("[PlayerRangedCombat] Out of battery!");
                return false;
            }

            int dir = playerController != null ? playerController.FacingDirection : 1;
            Vector3 spawnPos = transform.position + new Vector3(dir * 0.7f, 0.1f, 0f);
            BatteryProjectile bolt = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            var ctx = new DamageContext
            {
                amount = damage,
                knockback = new Vector2(dir * knockback.x, knockback.y),
                damageType = DamageType.Electric,
                hitstopDuration = 0.03f,
                source = gameObject
            };

            bolt.Launch(new Vector2(dir * projectileSpeed, 0f), ctx, solidMask);
            _cooldownTimer = fireCooldown;
            OnShotFired?.Invoke();
            return true;
        }
    }
}
