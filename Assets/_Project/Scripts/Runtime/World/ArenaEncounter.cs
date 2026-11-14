using System.Collections.Generic;
using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Enemies;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Arena: when Dante enters, the lock objects close the exits and the registered robots wake up;
    /// the locks reopen once every robot is destroyed. Dying aborts the fight (locks reopen) so the
    /// player is never sealed out of the path. Cleared arenas stay cleared for the session.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ArenaEncounter : MonoBehaviour
    {
        [SerializeField] private GameObject[] lockObjects;
        [SerializeField] private HealthComponent[] enemies;
        [SerializeField] private string persistentId;

        private static readonly HashSet<string> ClearedIds = new HashSet<string>();

        private const float OutsideAbortDelay = 0.75f;

        private HealthComponent _playerHealth;
        private Collider2D _playerCollider;
        private Collider2D _zone;
        private float _outsideTimer;
        private bool _reopenOnEnable;

        public bool IsActive { get; private set; }
        public bool IsCleared { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetSession()
        {
            ClearedIds.Clear();
        }

        private void Awake()
        {
            _zone = GetComponent<Collider2D>();
            _zone.isTrigger = true;
            SetLocks(false);

            if (!string.IsNullOrEmpty(persistentId) && ClearedIds.Contains(persistentId))
            {
                IsCleared = true;
                foreach (var enemy in enemies)
                {
                    if (enemy != null) Destroy(enemy.gameObject);
                }
            }
        }

        private void OnEnable()
        {
            if (!_reopenOnEnable) return;
            _reopenOnEnable = false;
            SetLocks(false);
        }

        private void OnDisable()
        {
            // Room switched off mid-fight (respawn, transition): end the fight so it never comes back sealed.
            // The locks are reopened in OnEnable, because toggling siblings while the room itself is being
            // deactivated is not allowed.
            if (IsActive)
            {
                IsActive = false;
                _reopenOnEnable = true;
            }
            UnsubscribePlayer();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActive || IsCleared) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            // Both safety nets (locks clear of Dante, abort when outside) need his body collider.
            var body = player.GetComponent<Collider2D>();
            if (body == null)
            {
                Debug.LogWarning($"[ArenaEncounter] {name}: player has no Collider2D; the arena will not start.", this);
                return;
            }

            var health = player.GetComponent<HealthComponent>();
            if (health != null && health.IsDead) return;
            Begin(health, body);
        }

        private void Begin(HealthComponent playerHealth, Collider2D playerBody)
        {
            IsActive = true;
            _outsideTimer = 0f;
            _playerCollider = playerBody;
            // Locks close in Update, each one only once Dante is clear of it: a lock appearing inside him
            // would push him out of the arena (e.g. on top of the shutter) and soft-lock the fight.

            _playerHealth = playerHealth;
            if (_playerHealth != null) _playerHealth.OnDied += Abort;

            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.TryGetComponent(out PatrolInterferenceTarget patrol)) patrol.Wake();
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            CloseLocksClearOfPlayer();

            // Safety net: if Dante ends up outside the arena while it is active, reopen everything
            // so the path can never stay sealed with him on the wrong side.
            if (_playerCollider != null && !_zone.bounds.Intersects(_playerCollider.bounds))
            {
                _outsideTimer += Time.deltaTime;
                if (_outsideTimer >= OutsideAbortDelay)
                {
                    Abort();
                    return;
                }
            }
            else
            {
                _outsideTimer = 0f;
            }

            foreach (var enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead) return;
            }
            Clear();
        }

        private void CloseLocksClearOfPlayer()
        {
            if (lockObjects == null) return;
            foreach (var lockObject in lockObjects)
            {
                if (lockObject == null || lockObject.activeSelf) continue;
                if (_playerCollider == null || !LockBounds(lockObject).Intersects(_playerCollider.bounds))
                {
                    lockObject.SetActive(true);
                }
            }
        }

        /// <summary>World bounds of an (inactive) lock block, computed from its transform and BoxCollider2D.</summary>
        private static Bounds LockBounds(GameObject lockObject)
        {
            Vector3 scale = lockObject.transform.lossyScale;
            Vector2 size = Vector2.one;
            Vector2 offset = Vector2.zero;
            if (lockObject.TryGetComponent(out BoxCollider2D box))
            {
                size = box.size;
                offset = box.offset;
            }
            Vector3 center = lockObject.transform.position + new Vector3(offset.x * scale.x, offset.y * scale.y, 0f);
            var bounds = new Bounds(center, new Vector3(Mathf.Abs(size.x * scale.x), Mathf.Abs(size.y * scale.y), 1f));
            bounds.Expand(new Vector3(0.1f, 0.1f, 10f));
            return bounds;
        }

        private void Clear()
        {
            IsActive = false;
            IsCleared = true;
            if (!string.IsNullOrEmpty(persistentId)) ClearedIds.Add(persistentId);
            SetLocks(false);
            UnsubscribePlayer();
        }

        private void Abort()
        {
            IsActive = false;
            SetLocks(false);
            UnsubscribePlayer();
        }

        private void UnsubscribePlayer()
        {
            if (_playerHealth != null) _playerHealth.OnDied -= Abort;
            _playerHealth = null;
        }

        private void SetLocks(bool locked)
        {
            if (lockObjects == null) return;
            foreach (var lockObject in lockObjects)
            {
                if (lockObject != null) lockObject.SetActive(locked);
            }
        }
    }
}
