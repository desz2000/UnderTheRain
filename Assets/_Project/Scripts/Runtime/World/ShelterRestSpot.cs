using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Shelter rest point: interacting while inside restores 100% health, battery and medkit charges.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ShelterRestSpot : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private bool isDefaultShelter = true;

        private GameObject _playerInRange;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            if (isDefaultShelter)
            {
                var deathHandler = FindFirstObjectByType<PlayerDeathHandler>();
                deathHandler?.SetRespawnPoint(transform.position);
            }
        }

        private void OnEnable()
        {
            if (input != null) input.OnInteractPressed += HandleInteractPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnInteractPressed -= HandleInteractPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() is { } player)
            {
                _playerInRange = player.gameObject;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_playerInRange != null && other.transform.IsChildOf(_playerInRange.transform))
            {
                _playerInRange = null;
            }
        }

        private void HandleInteractPressed() => TryRest();

        /// <summary>Rests if Dante is in range: restores him fully and makes this the respawn point.</summary>
        public bool TryRest()
        {
            if (_playerInRange == null) return false;

            _playerInRange.GetComponent<HealthComponent>()?.RestoreFull();
            _playerInRange.GetComponent<BatteryComponent>()?.RestoreFull();
            _playerInRange.GetComponent<MedkitComponent>()?.RestoreFull();
            _playerInRange.GetComponent<PlayerDeathHandler>()?.SetRespawnPoint(transform.position);
            Debug.Log("[ShelterRestSpot] Rested: health, battery and medkits restored to 100%, respawn updated.");
            return true;
        }
    }
}
