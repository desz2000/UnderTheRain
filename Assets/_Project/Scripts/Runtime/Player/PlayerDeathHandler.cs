using System.Collections;
using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;
using UnderTheRain.World;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Handles Dante's death loop: blocks input briefly, spawns a <see cref="DroppedPack"/>
    /// at the exact fall position, and respawns Dante at the last shelter with full resources.
    /// </summary>
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private HealthComponent health;
        [SerializeField] private BatteryComponent battery;
        [SerializeField] private MedkitComponent medkit;
        [SerializeField] private PartsPackComponent partsPack;
        [SerializeField] private DroppedPack droppedPackPrefab;
        [SerializeField] private float respawnDelay = 0.35f;

        private Rigidbody2D _body;
        private Vector3 _respawnPosition;
        private DroppedPack _activeDroppedPack;
        private bool _isRespawning;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            if (health == null) health = GetComponent<HealthComponent>();
            if (battery == null) battery = GetComponent<BatteryComponent>();
            if (medkit == null) medkit = GetComponent<MedkitComponent>();
            if (partsPack == null) partsPack = GetComponent<PartsPackComponent>();
            _respawnPosition = transform.position;
        }

        private void OnEnable()
        {
            if (health != null) health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (health != null) health.OnDied -= HandleDied;
        }

        public void SetRespawnPoint(Vector3 position)
        {
            _respawnPosition = position;
        }

        private void HandleDied()
        {
            if (_isRespawning) return;
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            _isRespawning = true;
            Vector3 deathPos = transform.position;

            input?.EnableUI(); // blocks gameplay movement/combat during death transition
            if (_body != null) _body.linearVelocity = Vector2.zero;

            int dropped = partsPack != null ? partsPack.ExtractAllOnDeath() : 0;
            if (dropped > 0 && droppedPackPrefab != null)
            {
                if (_activeDroppedPack != null) Destroy(_activeDroppedPack.gameObject);
                _activeDroppedPack = Instantiate(droppedPackPrefab, deathPos, Quaternion.identity);
                _activeDroppedPack.Initialize(dropped);
            }

            yield return new WaitForSeconds(respawnDelay);

            transform.position = _respawnPosition;
            if (_body != null) _body.linearVelocity = Vector2.zero;

            health?.RestoreFull();
            battery?.RestoreFull();
            // Medkits are NOT restored on death (kept as consumable inventory to preserve tension).

            input?.EnableGameplay();
            _isRespawning = false;
            Debug.Log($"[PlayerDeathHandler] Respawned at shelter ({_respawnPosition}). Dropped {dropped} parts.");
        }
    }
}
