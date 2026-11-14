using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// 2D trigger placed at room exits to initiate a fade-to-black transition to a target room and spawn point.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RoomExitTrigger : MonoBehaviour
    {
        [SerializeField] private RoomBounds targetRoom;
        [SerializeField] private string targetSpawnId = "default";
        [SerializeField] private string targetSceneName;
        [Tooltip("Room (RoomBounds.RoomId) to enter in the target scene. Empty = that scene's initial room.")]
        [SerializeField] private string targetSceneRoomId;
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Keep retrying while the player stays inside (e.g. a pit exit he cannot climb out of if a transition was busy).")]
        [SerializeField] private bool retryWhileInside;

        private RoomTransitionController _controller;

        public RoomBounds TargetRoom => targetRoom;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            _controller = FindFirstObjectByType<RoomTransitionController>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (targetRoom == null && string.IsNullOrEmpty(targetSceneName)) return;
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

            if (_controller == null)
            {
                _controller = FindFirstObjectByType<RoomTransitionController>();
            }

            if (!string.IsNullOrEmpty(targetSceneName))
            {
                _controller?.TransitionToScene(targetSceneName, targetSceneRoomId, targetSpawnId);
                return;
            }

            _controller?.TransitionToRoom(other.transform, targetRoom, targetSpawnId);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!retryWhileInside) return;
            if (_controller != null && _controller.IsTransitioning) return;
            OnTriggerEnter2D(other);
        }

        public void Configure(RoomBounds destination, string spawnId)
        {
            targetRoom = destination;
            targetSpawnId = spawnId;
            targetSceneName = null;
        }

        public void ConfigureScene(string sceneName, string roomId = null, string spawnId = "default")
        {
            targetSceneName = sceneName;
            targetSceneRoomId = roomId;
            targetSpawnId = spawnId;
            targetRoom = null;
        }
    }
}
