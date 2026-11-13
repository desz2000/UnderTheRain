using System.Collections.Generic;
using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// Defines a room's playable bounds for camera confinement and its entry spawn points.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class RoomBounds : MonoBehaviour
    {
        [SerializeField] private string roomId;
        [SerializeField] private BoxCollider2D boundsCollider;

        private readonly Dictionary<string, RoomSpawnPoint> _spawnPoints = new Dictionary<string, RoomSpawnPoint>();

        public string RoomId => roomId;

        public Bounds WorldBounds
        {
            get
            {
                if (boundsCollider == null)
                {
                    boundsCollider = GetComponent<BoxCollider2D>();
                }
                return boundsCollider != null ? boundsCollider.bounds : new Bounds(transform.position, new Vector3(20f, 12f, 0f));
            }
        }

        private void Awake()
        {
            if (boundsCollider == null)
            {
                boundsCollider = GetComponent<BoxCollider2D>();
            }

            if (boundsCollider != null)
            {
                boundsCollider.isTrigger = true;
            }

            CacheSpawnPoints();
        }

        public void CacheSpawnPoints()
        {
            _spawnPoints.Clear();
            var points = GetComponentsInChildren<RoomSpawnPoint>(true);
            foreach (var point in points)
            {
                if (!string.IsNullOrEmpty(point.SpawnId))
                {
                    _spawnPoints[point.SpawnId] = point;
                }
            }
        }

        public bool TryGetSpawnPosition(string spawnId, out Vector3 position)
        {
            if (_spawnPoints.Count == 0)
            {
                CacheSpawnPoints();
            }

            if (!string.IsNullOrEmpty(spawnId) && _spawnPoints.TryGetValue(spawnId, out var point))
            {
                position = point.Position;
                return true;
            }

            position = transform.position;
            return false;
        }

        public void SetRoomId(string id)
        {
            roomId = id;
        }
    }
}
