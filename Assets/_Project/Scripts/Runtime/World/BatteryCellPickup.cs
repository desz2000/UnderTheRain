using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Dropped battery cell that partially recharges Dante's battery when collected.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class BatteryCellPickup : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float rechargeAmount = 35f;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var battery = other.GetComponentInParent<BatteryComponent>();
            if (battery == null || battery.CurrentCharge >= battery.MaxCharge) return;

            battery.Recharge(rechargeAmount);
            Destroy(gameObject);
        }
    }
}
