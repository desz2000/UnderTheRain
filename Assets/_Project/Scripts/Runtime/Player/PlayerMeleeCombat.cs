using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Executes Dante's 2-3 hit melee combo using <see cref="WeaponDataSO"/>,
    /// activating the directional <see cref="Hitbox"/> and triggering <see cref="CombatFeedback"/>.
    /// </summary>
    public sealed class PlayerMeleeCombat : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private WeaponDataSO weaponData;
        [SerializeField] private Hitbox meleeHitbox;
        [SerializeField] private SpriteRenderer swingVisual;
        [SerializeField] private CombatFeedback combatFeedback;

        private MeleeComboState _combo;

        public int CurrentComboStep => _combo != null ? _combo.CurrentStep : 0;
        public bool IsAttacking => _combo != null && _combo.IsAttacking;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (weaponData == null)
            {
                Debug.LogError("[PlayerMeleeCombat] WeaponDataSO is not assigned.", this);
                return;
            }

            _combo = new MeleeComboState(weaponData);
            _combo.OnAttackStarted += HandleAttackStarted;
            _combo.OnAttackEnded += HandleAttackEnded;

            if (swingVisual != null) swingVisual.enabled = false;
        }

        private void OnEnable()
        {
            if (input != null) input.OnAttackPressed += HandleAttackPressed;
            if (meleeHitbox != null) meleeHitbox.OnHitLanded += HandleHitLanded;
        }

        private void OnDisable()
        {
            if (input != null) input.OnAttackPressed -= HandleAttackPressed;
            if (meleeHitbox != null) meleeHitbox.OnHitLanded -= HandleHitLanded;
        }

        private void Update()
        {
            _combo?.Tick(Time.deltaTime);
        }

        private void HandleAttackPressed()
        {
            _combo?.TryAttack();
        }

        private void HandleAttackStarted(int step)
        {
            if (meleeHitbox == null || weaponData == null) return;

            int dir = playerController != null ? playerController.FacingDirection : 1;
            Vector3 offset = new Vector3(dir * weaponData.attackRange * 0.65f, 0f, 0f);
            meleeHitbox.transform.localPosition = offset;
            if (meleeHitbox.TryGetComponent(out BoxCollider2D box))
            {
                box.size = weaponData.hitboxSize;
            }

            if (swingVisual != null)
            {
                swingVisual.transform.localPosition = offset;
                swingVisual.transform.localScale = new Vector3(weaponData.hitboxSize.x, weaponData.hitboxSize.y * 0.5f, 1f);
                swingVisual.color = step == weaponData.comboSteps
                    ? new Color(1f, 0.48f, 0.25f, 0.85f)
                    : new Color(0.95f, 0.95f, 0.98f, 0.75f);
                swingVisual.enabled = true;
            }

            Vector2 stepKnockback = weaponData.GetKnockbackForStep(step);
            var ctx = new DamageContext
            {
                amount = weaponData.GetDamageForStep(step),
                knockback = new Vector2(dir * stepKnockback.x, stepKnockback.y),
                damageType = weaponData.damageType,
                hitstopDuration = weaponData.GetHitstopForStep(step),
                source = gameObject
            };
            meleeHitbox.Activate(ctx);
        }

        private void HandleAttackEnded()
        {
            meleeHitbox?.Deactivate();
            if (swingVisual != null) swingVisual.enabled = false;
        }

        private void HandleHitLanded(DamageContext ctx)
        {
            if (weaponData == null) return;
            combatFeedback?.TriggerImpact(ctx.hitstopDuration, weaponData.screenShakeIntensity, weaponData.screenShakeDuration);

            // Subtle horizontal recoil on Dante when landing a melee hit.
            if (playerController != null && weaponData.recoilSpeed > 0f)
            {
                int dir = playerController.FacingDirection;
                playerController.ApplyRecoil(new Vector2(-dir * weaponData.recoilSpeed, 0f));
            }
        }
    }
}
