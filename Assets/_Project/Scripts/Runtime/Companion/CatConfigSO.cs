using UnityEngine;

namespace UnderTheRain.Companion
{
    /// <summary>
    /// Tuning parameters for the metal cat companion's follow and shoulder perch behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "CatConfig", menuName = "UnderTheRain/Companion/Cat Config")]
    public sealed class CatConfigSO : ScriptableObject
    {
        [Header("Offsets (relative to Dante, x multiplied by FacingDirection)")]
        [Tooltip("Resting follow offset close behind Dante when on the ground.")]
        public Vector2 followOffset = new Vector2(-0.68f, 0f);
        [Tooltip("Perch offset on Dante's shoulder/backpack during climbing, jumping, or combat.")]
        public Vector2 shoulderOffset = new Vector2(-0.20f, 0.52f);

        [Header("Follow Movement")]
        [Min(0.5f)] public float followSpeed = 10f;
        [Min(1f)] public float acceleration = 56f;
        [Min(1f)] public float deceleration = 64f;
        [Min(0.02f)] public float stopDistance = 0.08f;

        [Header("Gravity & Safety")]
        [Min(1f)] public float gravity = 32f;
        [Min(1f)] public float maxFallSpeed = 16f;
        [Min(2f)] public float teleportDistance = 11f;

        [Header("Shoulder Perch")]
        [Min(0.05f)] public float perchTransitionDuration = 0.18f;
        [Min(0.1f)] public float combatPerchHoldDuration = 1.6f;
        [Tooltip("How long the cat stays on Dante's shoulder after Dante lands from jumping/climbing.")]
        [Min(0.1f)] public float traversalPerchHoldDuration = 2.0f;
        [Tooltip("Height difference above the cat that triggers a smooth shoulder perch.")]
        [Min(0.3f)] public float highGroundPerchThresholdY = 0.9f;
        [Min(0.02f)] public float highGroundPerchDelay = 0.15f;

        [Header("Active Interference Command")]
        [Min(0.5f)] public float interferenceRadius = 4.5f;
        [Min(0.2f)] public float interferenceCooldown = 3.0f;
        [Min(0.2f)] public float hesitationDuration = 1.35f;
        [Min(0f)] public float interferenceBatteryCost = 50f;
    }
}
