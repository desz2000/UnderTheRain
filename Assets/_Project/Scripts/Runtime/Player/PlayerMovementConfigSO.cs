using UnityEngine;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Tunable locomotion values for Dante. Kept in a ScriptableObject so game feel
    /// can be iterated without touching code (no magic numbers).
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "UnderTheRain/Player/Movement Config")]
    public sealed class PlayerMovementConfigSO : ScriptableObject
    {
        [Header("Run")]
        [Min(0.1f)] public float maxRunSpeed = 8f;
        [Min(0.001f)] public float accelerationTime = 0.08f;
        [Min(0.001f)] public float decelerationTime = 0.06f;
        [Range(0f, 1f)] public float airControlMultiplier = 0.85f;

        [Header("Jump")]
        [Min(0.1f)] public float jumpHeight = 3.2f;
        [Min(0.1f)] public float riseGravity = 38f;
        [Min(1f)] public float fallGravityMultiplier = 1.6f;
        [Min(1f)] public float maxFallSpeed = 22f;
        [Tooltip("Applied to upward velocity when jump is released early (variable jump height).")]
        [Range(0.05f, 1f)] public float jumpCutMultiplier = 0.4f;

        [Header("Forgiveness")]
        [Min(0f)] public float coyoteTime = 0.04f;
        [Min(0f)] public float jumpBufferTime = 0.12f;
        [Min(0f)] public float cornerCorrectionDistance = 0.25f;

        [Header("Thin Platforms")]
        [Min(0f)] public float dropThroughDuration = 0.25f;

        [Header("Wall Slide & Wall Jump")]
        [Min(0.1f)] public float wallSlideMaxSpeed = 3.5f;
        public Vector2 wallJumpVelocity = new Vector2(6.5f, 14f);
        [Tooltip("Short window after a wall jump before air steering kicks back in, allowing single-wall climbing like Hollow Knight.")]
        [Min(0f)] public float wallJumpLockTime = 0.05f;
        [Tooltip("Grace window after leaving a wall where pressing jump still triggers a wall jump.")]
        [Min(0f)] public float wallCoyoteTime = 0.08f;

        /// <summary>Initial upward speed that reaches exactly <see cref="jumpHeight"/>.</summary>
        public float JumpVelocity => Mathf.Sqrt(2f * riseGravity * jumpHeight);
    }
}
