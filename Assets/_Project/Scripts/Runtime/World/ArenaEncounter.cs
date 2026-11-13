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

        private HealthComponent _playerHealth;

        public bool IsActive { get; private set; }
        public bool IsCleared { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            ClearedIds.Clear();
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
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

        private void OnDisable()
        {
            UnsubscribePlayer();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActive || IsCleared) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            var health = player.GetComponent<HealthComponent>();
            if (health != null && health.IsDead) return;
            Begin(health);
        }

        private void Begin(HealthComponent playerHealth)
        {
            IsActive = true;
            SetLocks(true);

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

            foreach (var enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead) return;
            }
            Clear();
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
