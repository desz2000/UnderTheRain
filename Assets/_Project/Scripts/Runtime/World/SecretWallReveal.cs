using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Fake wall: decor without collision that fades when Dante walks into it, revealing the secret behind.
    /// Stays revealed for the session (persistentId) until the save system lands.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class SecretWallReveal : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] covers;
        [SerializeField, Range(0f, 1f)] private float revealedAlpha = 0.25f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.35f;
        [SerializeField] private string persistentId;

        private static readonly HashSet<string> RevealedIds = new HashSet<string>();

        public bool IsRevealed { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetSession()
        {
            RevealedIds.Clear();
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (!string.IsNullOrEmpty(persistentId) && RevealedIds.Contains(persistentId))
            {
                IsRevealed = true;
                SetAlpha(revealedAlpha);
            }
        }

        private void OnEnable()
        {
            // A fade cut short by the room deactivating would otherwise stay half-way for the session.
            if (IsRevealed) SetAlpha(revealedAlpha);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsRevealed || other.GetComponentInParent<PlayerController>() == null) return;
            Reveal();
        }

        public void Reveal()
        {
            if (IsRevealed) return;
            IsRevealed = true;
            if (!string.IsNullOrEmpty(persistentId)) RevealedIds.Add(persistentId);
            if (isActiveAndEnabled) StartCoroutine(FadeRoutine());
            else SetAlpha(revealedAlpha);
        }

        private IEnumerator FadeRoutine()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                SetAlpha(Mathf.Lerp(1f, revealedAlpha, t / fadeDuration));
                yield return null;
            }
            SetAlpha(revealedAlpha);
        }

        private void SetAlpha(float alpha)
        {
            if (covers == null) return;
            foreach (var cover in covers)
            {
                if (cover == null) continue;
                Color c = cover.color;
                c.a = alpha;
                cover.color = c;
            }
        }
    }
}
