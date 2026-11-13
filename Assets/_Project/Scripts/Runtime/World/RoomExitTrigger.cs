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
        [SerializeField] private string playerTag = "Player";

        private RoomTransitionController _controller;

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
                _controller?.TransitionToScene(targetSceneName);
                return;
            }

            _controller?.TransitionToRoom(other.transform, targetRoom, targetSpawnId);
        }

        public void Configure(RoomBounds destination, string spawnId)
        {
            targetRoom = destination;
            targetSpawnId = spawnId;
            targetSceneName = null;
        }

        public void ConfigureScene(string sceneName)
        {
            targetSceneName = sceneName;
            targetRoom = null;
        }
    }
}
