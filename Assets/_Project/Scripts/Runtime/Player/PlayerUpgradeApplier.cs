using UnityEngine;
using UnderTheRain.Combat;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Re-applies session upgrades to Dante's freshly created components in every scene.
    /// Lives in its own file so scenes reference it by script GUID (Unity matches MonoBehaviours by file name).
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
