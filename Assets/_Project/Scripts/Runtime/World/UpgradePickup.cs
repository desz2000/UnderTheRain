using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Permanent upgrade found by exploring (+battery capacity or +1 health cell). Collected once per session.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class UpgradePickup : MonoBehaviour
    {
        [SerializeField] private UpgradeType upgradeType = UpgradeType.BatteryCapacity;
        [SerializeField, Min(1f)] private float amount = 25f;
        [SerializeField] private string persistentId;

        public UpgradeType Type => upgradeType;
        public float Amount => amount;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (string.IsNullOrEmpty(persistentId))
            {
                Debug.LogError($"[UpgradePickup] {name} has no persistentId and can never be collected.", this);
                return;
            }
            if (PlayerUpgradeSession.IsCollected(persistentId))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;
            if (!PlayerUpgradeSession.TryCollect(persistentId, upgradeType, amount))
            {
                // Same id already taken elsewhere (duplicate pickup): don't leave an inert orb behind.
                if (PlayerUpgradeSession.IsCollected(persistentId)) gameObject.SetActive(false);
                return;
            }

            if (upgradeType == UpgradeType.BatteryCapacity)
            {
                player.GetComponent<BatteryComponent>()?.IncreaseMax(amount);
            }
            else
            {
                player.GetComponent<HealthComponent>()?.IncreaseMax(Mathf.RoundToInt(amount));
            }

            Debug.Log($"[UpgradePickup] Collected {upgradeType} +{amount} ({persistentId}).");
            gameObject.SetActive(false);
        }
    }
}
