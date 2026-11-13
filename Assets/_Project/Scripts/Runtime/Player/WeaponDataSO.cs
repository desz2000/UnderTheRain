using UnityEngine;
using UnderTheRain.Combat;

namespace UnderTheRain.Player
{
    public enum WeaponStage
    {
        SimpleBaton,
        ElectrifiedBaton,
        HighFreqBlade
    }

    /// <summary>
    /// Tunable weapon configuration (stage, damage, combo timing, hitstop and screenshake).
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "UnderTheRain/Combat/Weapon Data")]
    public sealed class WeaponDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Simple Baton";
        public WeaponStage stage = WeaponStage.SimpleBaton;
        public DamageType damageType = DamageType.Physical;

        [Header("Attack & Combo")]
        [Min(1)] public int damage = 2;
        [Tooltip("Extra damage applied on the final step of the combo.")]
        [Min(0)] public int finalComboDamageBonus = 1;
        [Min(0.2f)] public float attackRange = 1.4f;
        public Vector2 hitboxSize = new Vector2(1.4f, 1.0f);
        [Range(1, 3)] public int comboSteps = 3;
        [Min(0.02f)] public float activeDuration = 0.14f;
        [Tooltip("Grace window after an attack ends during which the next combo step can be chained.")]
        [Min(0.05f)] public float comboWindow = 0.35f;
        [Tooltip("Cooldown after finishing the full combo before starting step 1 again.")]
        [Min(0f)] public float comboResetCooldown = 0.22f;

        [Header("Game Feel")]
        [Min(0f)] public float hitstopDuration = 0.05f;
        [Min(1f)] public float finalComboHitstopMultiplier = 1.4f;
        public Vector2 knockbackForce = new Vector2(6f, 2f);
        [Min(1f)] public float finalComboKnockbackMultiplier = 1.35f;
        [Min(0f)] public float recoilSpeed = 2.5f;
        [Min(0f)] public float screenShakeIntensity = 0.10f;
        [Min(0f)] public float screenShakeDuration = 0.08f;

        public int GetDamageForStep(int step) => step >= comboSteps ? damage + finalComboDamageBonus : damage;
        public float GetHitstopForStep(int step) => step >= comboSteps ? hitstopDuration * finalComboHitstopMultiplier : hitstopDuration;
        public Vector2 GetKnockbackForStep(int step) => step >= comboSteps ? knockbackForce * finalComboKnockbackMultiplier : knockbackForce;
    }
}
