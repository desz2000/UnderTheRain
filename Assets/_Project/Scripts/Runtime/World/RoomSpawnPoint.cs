using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// Marks a named entry position inside a room for room-to-room transitions and shelter respawns.
    /// </summary>
    public sealed class RoomSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId = "default";

        public string SpawnId => spawnId;
        public Vector3 Position => transform.position;

        public void SetSpawnId(string id)
        {
            spawnId = id;
        }
    }
}
