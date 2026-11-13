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

        /// <summary>
        /// Point-in-room test computed from the collider's size/offset and the transform, so it also works
        /// while the room is inactive (Collider2D.bounds is empty on inactive objects).
        /// </summary>
        public bool ContainsPoint(Vector2 worldPoint)
        {
            if (boundsCollider == null) boundsCollider = GetComponent<BoxCollider2D>();
            if (boundsCollider == null) return false;

            Vector3 scale = transform.lossyScale;
            Vector2 center = (Vector2)transform.position + Vector2.Scale(boundsCollider.offset, scale);
            Vector2 half = 0.5f * new Vector2(Mathf.Abs(boundsCollider.size.x * scale.x), Mathf.Abs(boundsCollider.size.y * scale.y));
            Vector2 d = worldPoint - center;
            return Mathf.Abs(d.x) <= half.x && Mathf.Abs(d.y) <= half.y;
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
