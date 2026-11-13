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
        }

        public bool CanInteract() => !IsCompleted;

        public void Complete()
        {
            if (IsCompleted) return;
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

            onCompleted?.Invoke();
            Debug.Log($"[CatExplorationPoint] Completed {explorationType} interaction on {name}.");
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
