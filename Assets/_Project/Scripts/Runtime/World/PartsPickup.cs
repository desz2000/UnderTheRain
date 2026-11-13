using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Mechanical parts pickup dropped by defeated enemies.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class PartsPickup : MonoBehaviour
    {
        [SerializeField, Min(1)] private int partsAmount = 10;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void SetAmount(int amount)
        {
            partsAmount = Mathf.Max(1, amount);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var pack = other.GetComponentInParent<PartsPackComponent>();
            if (pack == null || pack.CurrentParts >= pack.MaxCapacity) return;

            pack.AddParts(partsAmount);
            Destroy(gameObject);
        }
    }
}
