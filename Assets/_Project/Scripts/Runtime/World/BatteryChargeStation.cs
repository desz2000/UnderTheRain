using UnityEngine;
using UnderTheRain.Core.Input;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Map battery charging station: interacting restores 100% battery without healing HP or changing respawn.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class BatteryChargeStation : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;

        private BatteryComponent _batteryInRange;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
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
            if (other.GetComponentInParent<BatteryComponent>() is { } battery)
            {
                _batteryInRange = battery;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_batteryInRange != null && other.transform.IsChildOf(_batteryInRange.transform))
            {
                _batteryInRange = null;
            }
        }

        private void HandleInteractPressed()
        {
            if (_batteryInRange == null) return;
            if (_batteryInRange.CurrentCharge >= _batteryInRange.MaxCharge)
            {
                Debug.Log("[BatteryChargeStation] Battery already at 100%.");
                return;
            }

            _batteryInRange.RestoreFull();
            Debug.Log("[BatteryChargeStation] Battery recharged to 100% (HP unchanged).");
        }
    }
}
