using System.Collections;
using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.World;

namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Hive robot body: moves via <see cref="PatrolInterferenceTarget"/> (patrol and chase), hurts Dante on body contact,
    /// takes damage from melee/battery shots and drops parts on death.
    /// While Kitty's interference makes it hesitate, its contact hitbox is disabled.
    /// </summary>
    [RequireComponent(typeof(HealthComponent), typeof(PatrolInterferenceTarget))]
    public sealed class ScoutRobotEnemy : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private GameObject contactHitbox;
        [SerializeField] private PartsPickup partsPickupPrefab;
        [SerializeField, Min(0)] private int partsDropAmount = 10;
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;

        private HealthComponent _health;
        private PatrolInterferenceTarget _patrol;
        private EnemyAwareness _awareness;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _patrol = GetComponent<PatrolInterferenceTarget>();
            _awareness = GetComponent<EnemyAwareness>();
            if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= HandleDied;
        }

        private void Update()
        {
            if (contactHitbox == null) return;

            // Dormant (powered-down) and hesitating robots do not hurt on contact.
            bool shouldHurt = !_patrol.IsHesitating && !_patrol.IsDormant && !_health.IsDead;
            if (contactHitbox.activeSelf != shouldHurt) contactHitbox.SetActive(shouldHurt);
        }

        private void HandleDamaged(DamageContext ctx)
        {
            _patrol.Wake();
            // Getting hit gives Dante away even from behind or from out of sight.
            if (_awareness != null) _awareness.AlertToTarget();
            if (bodyRenderer == null) return;

            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private void HandleDied()
        {
            if (partsPickupPrefab != null && partsDropAmount > 0)
            {
                var drop = Instantiate(partsPickupPrefab, transform.position + Vector3.up * 0.4f, Quaternion.identity);
                drop.SetAmount(partsDropAmount);
            }

            Destroy(gameObject);
        }

        private IEnumerator FlashRoutine()
        {
            bodyRenderer.color = hitFlashColor;
            yield return new WaitForSecondsRealtime(flashDuration);
            // Ask the patrol for the current state: hesitation may have started or ended during the flash.
            bodyRenderer.color = _patrol.CurrentBodyColor;
            _flashRoutine = null;
        }
    }
}
