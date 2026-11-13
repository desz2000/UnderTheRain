using System;
using System.Collections;
using UnityEngine;
using UnderTheRain.Core.Input;
using UnderTheRain.World;

namespace UnderTheRain.Companion
{
    /// <summary>
    /// Pure state guard for cat exploration interactions (prevents concurrent or repeated runs).
    /// </summary>
    public sealed class CatExplorationModel
    {
        public bool IsExploring { get; private set; }

        public bool CanStart(bool pointAvailable, bool pointAlreadyCompleted)
        {
            return !IsExploring && pointAvailable && !pointAlreadyCompleted;
        }

        public bool TryBegin(bool pointAvailable, bool pointAlreadyCompleted)
        {
            if (!CanStart(pointAvailable, pointAlreadyCompleted)) return false;
            IsExploring = true;
            return true;
        }

        public void Finish()
        {
            IsExploring = false;
        }
    }

    /// <summary>
    /// Sends the metal cat through a nearby <see cref="CatExplorationPoint"/> (duct or terminal)
    /// when Dante presses Interact, unlocking shortcuts from the other side.
    /// </summary>
    public sealed class CatExploration : MonoBehaviour
    {
        private const float EntryMoveDuration = 0.22f;

        [SerializeField] private GameInputReader input;
        [SerializeField] private CatController catController;

        private readonly CatExplorationModel _model = new CatExplorationModel();
        private CatExplorationPoint _nearbyPoint;

        public bool IsExploring => _model.IsExploring;

        public event Action<CatExplorationPoint> OnExplorationCompleted;

        private void Awake()
        {
            if (catController == null) catController = GetComponent<CatController>();
        }

        private void OnEnable()
        {
            if (input != null) input.OnInteractPressed += HandleInteractPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnInteractPressed -= HandleInteractPressed;
        }

        public void RegisterNearbyPoint(CatExplorationPoint point)
        {
            if (point != null && !point.IsCompleted)
            {
                _nearbyPoint = point;
            }
        }

        public void UnregisterNearbyPoint(CatExplorationPoint point)
        {
            if (_nearbyPoint == point)
            {
                _nearbyPoint = null;
            }
        }

        private void HandleInteractPressed()
        {
            TryStartExploration();
        }

        public bool TryStartExploration()
        {
            bool hasPoint = _nearbyPoint != null;
            bool alreadyCompleted = hasPoint && _nearbyPoint.IsCompleted;
            if (!_model.TryBegin(hasPoint, alreadyCompleted)) return false;

            StartCoroutine(ExplorationRoutine(_nearbyPoint));
            return true;
        }

        private IEnumerator ExplorationRoutine(CatExplorationPoint point)
        {
            if (catController != null) catController.SetExternalControl(true);

            Vector3 startPos = transform.position;
            Vector3 entryPos = point.EntryPosition;
            Vector3 targetPos = point.TargetPosition;

            yield return MoveSmooth(startPos, entryPos, EntryMoveDuration);
            yield return MoveSmooth(entryPos, targetPos, point.TravelDuration);

            point.Complete();
            yield return new WaitForSeconds(0.15f);

            // Return through the duct if it's a duct, then release back to CatController follow.
            if (point.ExplorationType == CatExplorationType.Duct)
            {
                yield return MoveSmooth(targetPos, entryPos, point.TravelDuration * 0.8f);
            }

            if (catController != null) catController.SetExternalControl(false);
            _model.Finish();
            _nearbyPoint = null;
            OnExplorationCompleted?.Invoke(point);
        }

        private IEnumerator MoveSmooth(Vector3 from, Vector3 to, float duration)
        {
            if (duration <= 0f)
            {
                transform.position = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(from, to, eased);
                yield return null;
            }
            transform.position = to;
        }
    }
}
