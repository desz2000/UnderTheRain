using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Spawned at Dante's exact death position holding his dropped parts until recovered.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DroppedPack : MonoBehaviour
    {
        private int _storedParts;
        private bool _recovered;

        public int StoredParts => _storedParts;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Initialize(int amount)
        {
            _storedParts = Mathf.Max(0, amount);
            _recovered = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_recovered) return;
            var pack = other.GetComponentInParent<PartsPackComponent>();
            if (pack == null) return;

            _recovered = true;
            int added = pack.AddParts(_storedParts);
            Debug.Log($"[DroppedPack] Recovered {added} parts!");
            Destroy(gameObject);
        }
    }
}
