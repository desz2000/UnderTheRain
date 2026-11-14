using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnderTheRain.World
{
    /// <summary>
    /// Orchestrates seamless Hollow Knight-style room transitions: fade out, swap active room,
    /// move player to spawn point, update camera confiner, and fade in.
    /// </summary>
    public sealed class RoomTransitionController : MonoBehaviour
    {
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private RoomCamera2D roomCamera;
        [SerializeField] private RoomBounds initialRoom;
        [SerializeField] private float fadeDuration = 0.25f;

        // Entry requested by the previous scene's exit; consumed once by the next controller that starts.
        private static string _pendingRoomId;
        private static string _pendingSpawnId;

        private RoomBounds _currentRoom;
        private bool _isTransitioning;

        public bool IsTransitioning => _isTransitioning;
        public RoomBounds CurrentRoom => _currentRoom;

        public event Action<RoomBounds> OnRoomChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPendingEntry()
        {
            _pendingRoomId = null;
            _pendingSpawnId = null;
        }

        private void Start()
        {
            if (TryApplyPendingSceneEntry()) return;

            if (_currentRoom == null && initialRoom != null)
            {
                SetActiveRoomImmediate(initialRoom);
            }
        }

        private bool TryApplyPendingSceneEntry()
        {
            if (string.IsNullOrEmpty(_pendingRoomId)) return false;

            string roomId = _pendingRoomId;
            string spawnId = _pendingSpawnId;
            _pendingRoomId = null;
            _pendingSpawnId = null;

            RoomBounds targetRoom = null;
            foreach (var room in GetSceneRooms())
            {
                if (room.RoomId == roomId)
                {
                    targetRoom = room;
                    break;
                }
            }

            if (targetRoom == null)
            {
                Debug.LogWarning($"[RoomTransitionController] Pending entry room '{roomId}' not found; using initial room.", this);
                return false;
            }

            // Move the player before activating the room so the camera snaps to the right spot.
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && targetRoom.TryGetSpawnPosition(spawnId, out Vector3 spawnPos))
            {
                Teleport(player.transform, spawnPos);
            }

            if (initialRoom != null && initialRoom != targetRoom)
            {
                initialRoom.gameObject.SetActive(false);
            }

            SetActiveRoomImmediate(targetRoom);

            // The previous scene faded to black; continue from black instead of popping in.
            if (screenFader != null)
            {
                screenFader.SetAlpha(1f);
                StartCoroutine(screenFader.FadeIn(fadeDuration));
            }
            return true;
        }

        /// <summary>
        /// All rooms of this controller's scene, including inactive ones.
        /// </summary>
        private List<RoomBounds> GetSceneRooms()
        {
            var rooms = new List<RoomBounds>();
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                rooms.AddRange(root.GetComponentsInChildren<RoomBounds>(true));
            }
            return rooms;
        }

        public RoomBounds FindRoomContaining(Vector3 worldPosition)
        {
            foreach (var room in GetSceneRooms())
            {
                if (room.ContainsPoint(worldPosition)) return room;
            }
            return null;
        }

        /// <summary>
        /// Death respawn: fade out, move the traveler, activate the room that contains the respawn point, fade in.
        /// Without this, respawning into a different room leaves Dante in an inactive (floorless) room.
        /// </summary>
        public IEnumerator RespawnRoutine(Transform traveler, Vector3 position)
        {
            _isTransitioning = true;

            if (screenFader != null)
            {
                yield return screenFader.FadeOut(fadeDuration);
            }

            Teleport(traveler, position);

            RoomBounds room = FindRoomContaining(position);
            if (room != null)
            {
                SetActiveRoomImmediate(room);
            }

            if (screenFader != null)
            {
                yield return screenFader.FadeIn(fadeDuration);
            }

            _isTransitioning = false;
        }

        private static void Teleport(Transform traveler, Vector3 position)
        {
            traveler.position = position;
            if (traveler.TryGetComponent(out Rigidbody2D body))
            {
                body.position = position;
                body.linearVelocity = Vector2.zero;
            }
        }

        public void Initialize(ScreenFader fader, RoomCamera2D cameraFollower, RoomBounds initialRoom = null)
        {
            screenFader = fader;
            roomCamera = cameraFollower;
            if (initialRoom != null)
            {
                SetActiveRoomImmediate(initialRoom);
            }
        }

        public void SetActiveRoomImmediate(RoomBounds room)
        {
            if (_currentRoom != null && _currentRoom != room)
            {
                _currentRoom.gameObject.SetActive(false);
            }

            _currentRoom = room;
            if (_currentRoom != null)
            {
                _currentRoom.gameObject.SetActive(true);
            }

            roomCamera?.SetRoom(_currentRoom, snapImmediately: true);
            OnRoomChanged?.Invoke(_currentRoom);
        }

        public void TransitionToRoom(Transform traveler, RoomBounds targetRoom, string spawnId)
        {
            if (_isTransitioning || traveler == null || targetRoom == null)
            {
                return;
            }

            StartCoroutine(TransitionRoutine(traveler, targetRoom, spawnId));
        }

        private IEnumerator TransitionRoutine(Transform traveler, RoomBounds targetRoom, string spawnId)
        {
            _isTransitioning = true;

            if (screenFader != null)
            {
                yield return screenFader.FadeOut(fadeDuration);
            }

            if (_currentRoom != null && _currentRoom != targetRoom)
            {
                _currentRoom.gameObject.SetActive(false);
            }

            _currentRoom = targetRoom;
            _currentRoom.gameObject.SetActive(true);

            if (_currentRoom.TryGetSpawnPosition(spawnId, out Vector3 spawnPos))
            {
                // Must move the Rigidbody2D too: with interpolation (and corner correction writing
                // body.position mid-jump), a transform-only move gets overwritten and Dante is left
                // behind in the room that was just deactivated.
                Teleport(traveler, spawnPos);
            }

            roomCamera?.SetRoom(_currentRoom, snapImmediately: true);
            OnRoomChanged?.Invoke(_currentRoom);

            if (screenFader != null)
            {
                yield return screenFader.FadeIn(fadeDuration);
            }

            _isTransitioning = false;
        }

        /// <summary>
        /// Loads another scene. When targetRoomId is set, the next scene starts in that room at targetSpawnId
        /// instead of its default initial room.
        /// </summary>
        public void TransitionToScene(string sceneName, string targetRoomId = null, string targetSpawnId = null)
        {
            if (_isTransitioning || string.IsNullOrEmpty(sceneName)) return;
            _pendingRoomId = targetRoomId;
            _pendingSpawnId = targetSpawnId;
            StartCoroutine(SceneTransitionRoutine(sceneName));
        }

        private IEnumerator SceneTransitionRoutine(string sceneName)
        {
            _isTransitioning = true;

            if (screenFader != null)
            {
                yield return screenFader.FadeOut(fadeDuration);
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
