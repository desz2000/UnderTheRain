using System.Collections.Generic;
using UnityEngine;
using UnderTheRain.Combat;

namespace UnderTheRain.Player
{
    public enum UpgradeType
    {
        BatteryCapacity,
        HealthCell
    }

    /// <summary>
    /// Session memory of permanent upgrades (until the Epic 8 save system persists them).
    /// Pure static state so it survives scene loads; reset when entering Play Mode.
    /// </summary>
    public static class PlayerUpgradeSession
    {
        private static readonly HashSet<string> CollectedIds = new HashSet<string>();

        public static float BatteryBonus { get; private set; }
        public static int HealthBonus { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            CollectedIds.Clear();
            BatteryBonus = 0f;
            HealthBonus = 0;
        }

        public static bool IsCollected(string id) => !string.IsNullOrEmpty(id) && CollectedIds.Contains(id);

        /// <summary>Registers the upgrade once; returns false if this id was already collected.</summary>
        public static bool TryCollect(string id, UpgradeType type, float amount)
        {
            if (string.IsNullOrEmpty(id) || amount <= 0f || !CollectedIds.Add(id)) return false;

            if (type == UpgradeType.BatteryCapacity) BatteryBonus += amount;
            else HealthBonus += Mathf.RoundToInt(amount);
            return true;
        }
    }

    /// <summary>
    /// Re-applies session upgrades to Dante's freshly created components in every scene.
    /// </summary>
    public sealed class PlayerUpgradeApplier : MonoBehaviour
    {
        private void Start()
        {
            if (PlayerUpgradeSession.BatteryBonus > 0f && TryGetComponent(out BatteryComponent battery))
            {
                battery.IncreaseMax(PlayerUpgradeSession.BatteryBonus);
            }

            if (PlayerUpgradeSession.HealthBonus > 0 && TryGetComponent(out HealthComponent health))
            {
                health.IncreaseMax(PlayerUpgradeSession.HealthBonus);
            }
        }
    }
}
