using System;
using System.Collections;
using UnityEngine;

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
        [SerializeField] private float fadeDuration = 0.25f;

        private RoomBounds _currentRoom;
        private bool _isTransitioning;

        public bool IsTransitioning => _isTransitioning;
        public RoomBounds CurrentRoom => _currentRoom;

        public event Action<RoomBounds> OnRoomChanged;

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
                traveler.position = spawnPos;
            }

            roomCamera?.SetRoom(_currentRoom, snapImmediately: true);
            OnRoomChanged?.Invoke(_currentRoom);

            if (screenFader != null)
            {
                yield return screenFader.FadeIn(fadeDuration);
            }

            _isTransitioning = false;
        }
    }
}
