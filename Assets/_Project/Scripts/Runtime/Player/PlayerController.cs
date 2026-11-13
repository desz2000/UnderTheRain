using System.Collections;
using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Bridges input and physics to <see cref="PlayerMotor"/>: ground check,
    /// corner correction and drop-through on thin platforms.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float GroundProbeDistance = 0.05f;
        private const float CeilingProbeDistance = 0.05f;
        private const float WallProbeDistance = 0.06f;
        private const float CornerStep = 0.05f;
        private const float DamageKnockbackLockTime = 0.16f;

        [SerializeField] private GameInputReader input;
        [SerializeField] private PlayerMovementConfigSO config;
        [SerializeField] private LayerMask groundMask;
        [Tooltip("Subset of ground layers that can be dropped through (thin platforms).")]
        [SerializeField] private LayerMask oneWayMask;
        [SerializeField] private PlayerAbilitySet abilities;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private HealthComponent _health;
        private SpriteRenderer _sprite;
        private PlayerMotor _motor;
        private bool _jumpPressed;
        private bool _jumpReleased;
        private Collider2D _groundCollider;
        private Coroutine _invulnFlashRoutine;
        private float _knockbackLockTimer;
        private bool _controlLocked;

        public PlayerLocomotionState State => _motor != null ? _motor.State : PlayerLocomotionState.Idle;
        public ref PlayerAbilitySet Abilities => ref abilities;
        public int FacingDirection { get; private set; } = 1;
        public bool IsControlLocked => _controlLocked;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _health = GetComponent<HealthComponent>();
            _sprite = GetComponent<SpriteRenderer>();
            _body.gravityScale = 0f; // gravity is owned by PlayerMotor for tunable game feel
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (config != null) _motor = new PlayerMotor(config);
            else Debug.LogError("[PlayerController] PlayerMovementConfigSO is not assigned.", this);
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.OnJumpPressed += HandleJumpPressed;
                input.OnJumpReleased += HandleJumpReleased;
            }

            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.OnJumpPressed -= HandleJumpPressed;
                input.OnJumpReleased -= HandleJumpReleased;
            }

            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
            }
        }

        public void SetControlLocked(bool locked)
        {
            _controlLocked = locked;
            if (locked)
            {
                _jumpPressed = false;
                _jumpReleased = false;
            }
        }

        public void ApplyRecoil(Vector2 velocity, float lockDuration = 0f)
        {
            if (_body == null) return;
            _body.linearVelocity = new Vector2(velocity.x, Mathf.Abs(velocity.y) > 0.01f ? velocity.y : _body.linearVelocity.y);
            if (lockDuration > _knockbackLockTimer)
            {
                _knockbackLockTimer = lockDuration;
            }
        }

        private void HandleDamaged(DamageContext ctx)
        {
            ApplyRecoil(ctx.knockback, DamageKnockbackLockTime);
            if (_sprite != null)
            {
                if (_invulnFlashRoutine != null) StopCoroutine(_invulnFlashRoutine);
                _invulnFlashRoutine = StartCoroutine(InvulnerabilityFlashRoutine());
            }
        }

        private IEnumerator InvulnerabilityFlashRoutine()
        {
            Color baseColor = _sprite.color;
            while (_health != null && _health.IsInvulnerable)
            {
                _sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);
                yield return new WaitForSeconds(0.08f);
                _sprite.color = baseColor;
                yield return new WaitForSeconds(0.08f);
            }
            _sprite.color = baseColor;
            _invulnFlashRoutine = null;
        }

        private void HandleJumpPressed()
        {
            if (TryDropThrough()) return;
            _jumpPressed = true;
        }

        private void HandleJumpReleased() => _jumpReleased = true;

        private void FixedUpdate()
        {
            if (_motor == null || input == null) return;

            if (_knockbackLockTimer > 0f)
            {
                _knockbackLockTimer = Mathf.Max(0f, _knockbackLockTimer - Time.fixedDeltaTime);
            }

            if (!_controlLocked && Mathf.Abs(input.MoveInput.x) > 0.1f)
            {
                FacingDirection = input.MoveInput.x > 0f ? 1 : -1;
            }

            bool grounded = CheckGround();
            int wallDir = grounded ? 0 : CheckWall();
            var frame = new PlayerMotorInput
            {
                moveX = _controlLocked ? 0f : input.MoveInput.x,
                jumpPressed = !_controlLocked && _jumpPressed,
                jumpHeld = !_controlLocked && input.IsJumpHeld,
                jumpReleased = !_controlLocked && _jumpReleased,
                wallContactDir = wallDir,
                wallJumpUnlocked = abilities.wallJump,
                lockHorizontal = _controlLocked || _knockbackLockTimer > 0f
            };
            _jumpPressed = false;
            _jumpReleased = false;

            _motor.Velocity = _body.linearVelocity;
            if (_motor.Velocity.y > 0f && HeadBlocked() && !TryCornerCorrection())
                _motor.OnCeilingHit();

            _motor.Step(Time.fixedDeltaTime, frame, grounded);
            _body.linearVelocity = _motor.Velocity;
        }

        private bool CheckGround()
        {
            Bounds b = _collider.bounds;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(b.center, new Vector2(b.size.x * 0.95f, b.size.y), 0f,
                Vector2.down, GroundProbeDistance, groundMask);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null || hit.normal.y <= 0.5f) continue;

                bool isOneWay = (oneWayMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                if (isOneWay)
                {
                    // Ignore one-way platforms while ascending or while Dante's feet are still inside them.
                    if (_body.linearVelocity.y > 0.05f) continue;
                    if (b.min.y < hit.collider.bounds.max.y - 0.1f) continue;
                }

                _groundCollider = hit.collider;
                return true;
            }

            _groundCollider = null;
            return false;
        }

        // Only solid walls (exclude one-way platforms) allow wall slide and wall jump.
        private int CheckWall()
        {
            LayerMask solidMask = groundMask & ~oneWayMask;
            Bounds b = _collider.bounds;
            Vector2 probeSize = new Vector2(b.size.x, b.size.y * 0.8f);

            RaycastHit2D rightHit = Physics2D.BoxCast(b.center, probeSize, 0f, Vector2.right, WallProbeDistance, solidMask);
            if (rightHit.collider != null && rightHit.normal.x < -0.5f) return 1;

            RaycastHit2D leftHit = Physics2D.BoxCast(b.center, probeSize, 0f, Vector2.left, WallProbeDistance, solidMask);
            if (leftHit.collider != null && leftHit.normal.x > 0.5f) return -1;

            return 0;
        }

        private bool HeadBlocked()
        {
            LayerMask solidMask = groundMask & ~oneWayMask;
            Bounds b = _collider.bounds;
            return Physics2D.BoxCast(b.center, b.size, 0f, Vector2.up, CeilingProbeDistance, solidMask).collider != null;
        }

        // Nudges Dante sideways around a ceiling corner so near-misses still count as jumps.
        private bool TryCornerCorrection()
        {
            LayerMask solidMask = groundMask & ~oneWayMask;
            Bounds b = _collider.bounds;
            for (float offset = CornerStep; offset <= config.cornerCorrectionDistance; offset += CornerStep)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector2 origin = (Vector2)b.center + Vector2.right * (side * offset);
                    bool blocked = Physics2D.BoxCast(origin, b.size, 0f, Vector2.up, CeilingProbeDistance, solidMask).collider != null;
                    bool overlapping = Physics2D.OverlapBox(origin, b.size, 0f, solidMask) != null;
                    if (!blocked && !overlapping)
                    {
                        _body.position += Vector2.right * (side * offset);
                        return true;
                    }
                }
            }
            return false;
        }

        // Down + Jump on a thin platform drops through instead of jumping.
        private bool TryDropThrough()
        {
            if (_groundCollider == null || input.MoveInput.y > -0.5f && !input.IsCrouchHeld) return false;
            if ((oneWayMask.value & (1 << _groundCollider.gameObject.layer)) == 0) return false;
            StartCoroutine(DropThrough(_groundCollider));
            return true;
        }

        private IEnumerator DropThrough(Collider2D platform)
        {
            Physics2D.IgnoreCollision(_collider, platform, true);
            yield return new WaitForSeconds(config.dropThroughDuration);
            if (platform != null) Physics2D.IgnoreCollision(_collider, platform, false);
        }
    }
}
