using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnderTheRain.Companion;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    public enum CatExplorationType
    {
        Duct,
        Terminal
    }

    /// <summary>
    /// Interactive world point (narrow duct or offline terminal) where Dante can send
    /// the metal cat to unlock a shortcut or door from the other side.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class CatExplorationPoint : MonoBehaviour
    {
        [SerializeField] private CatExplorationType explorationType = CatExplorationType.Duct;
        [SerializeField] private Transform catEntryPoint;
        [SerializeField] private Transform catTargetPoint;
        [SerializeField, Min(0.1f)] private float travelDuration = 0.65f;
        [SerializeField] private GameObject[] objectsToDisableOnComplete;
        [SerializeField] private GameObject promptIndicator;
        [SerializeField] private UnityEvent onCompleted;
        [Tooltip("Unique id that keeps this shortcut open when the scene is reloaded in the same session. Empty = not remembered.")]
        [SerializeField] private string persistentId;

        // Session memory until the save system (Epic 8) persists opened shortcuts.
        private static readonly HashSet<string> CompletedIds = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            CompletedIds.Clear();
        }

        public CatExplorationType ExplorationType => explorationType;
        public Vector3 EntryPosition => catEntryPoint != null ? catEntryPoint.position : transform.position;
        public Vector3 TargetPosition => catTargetPoint != null ? catTargetPoint.position : transform.position;
        public float TravelDuration => travelDuration;
        public bool IsCompleted { get; private set; }

        private void Awake()
        {
            if (TryGetComponent(out BoxCollider2D col))
            {
                col.isTrigger = true;
            }
            if (promptIndicator != null)
            {
                promptIndicator.SetActive(false);
            }

            if (!string.IsNullOrEmpty(persistentId) && CompletedIds.Contains(persistentId))
            {
                ApplyCompletedState();
            }
        }

        public bool CanInteract() => !IsCompleted;

        public void Complete()
        {
            if (IsCompleted) return;
            if (!string.IsNullOrEmpty(persistentId)) CompletedIds.Add(persistentId);
            ApplyCompletedState();
            onCompleted?.Invoke();
            Debug.Log($"[CatExplorationPoint] Completed {explorationType} interaction on {name}.");
        }

        private void ApplyCompletedState()
        {
            IsCompleted = true;

            if (promptIndicator != null)
            {
                promptIndicator.SetActive(false);
            }

            if (objectsToDisableOnComplete != null)
            {
                for (int i = 0; i < objectsToDisableOnComplete.Length; i++)
                {
                    if (objectsToDisableOnComplete[i] != null)
                    {
                        objectsToDisableOnComplete[i].SetActive(false);
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsCompleted || !other.TryGetComponent(out PlayerController _)) return;

            if (promptIndicator != null) promptIndicator.SetActive(true);
            CatExploration explorer = FindFirstObjectByType<CatExploration>();
            if (explorer != null) explorer.RegisterNearbyPoint(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController _)) return;

            if (promptIndicator != null) promptIndicator.SetActive(false);
            CatExploration explorer = FindFirstObjectByType<CatExploration>();
            if (explorer != null) explorer.UnregisterNearbyPoint(this);
        }
    }
}
