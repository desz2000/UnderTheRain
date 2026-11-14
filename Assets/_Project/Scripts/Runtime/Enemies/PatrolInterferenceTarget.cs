using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Core of a hive robot: faction, Kitty interference (hesitation), dormancy and ground movement.
    /// Without an <see cref="EnemyAwareness"/> it only patrols; with one it chases Dante while it sees him,
    /// searches his last known position and then walks back to its post.
    /// Flying robots keep this core for state and move with <see cref="FlyingDroneMotor"/> instead.
    /// </summary>
    public sealed class PatrolInterferenceTarget : MonoBehaviour, ICatAffectable
    {
        [SerializeField] private EnemyFaction faction = EnemyFaction.CollectiveMachine;
        [SerializeField, Min(0.5f)] private float patrolSpeed = 2.2f;
        [SerializeField, Min(0.2f)] private float patrolHalfWidth = 2.0f;
        [SerializeField, Min(0f)] private float immunityCooldownAfterRecover = 1.5f;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer hesitationGlyph;
        [Tooltip("Starts powered down (no patrol, no contact damage) until woken by proximity, damage or an arena.")]
        [SerializeField] private bool startDormant;
        [Tooltip("Wakes when Dante comes this close. 0 = only wakes from damage or Wake().")]
        [SerializeField, Min(0f)] private float wakeRadius;

        [Header("Chase (needs EnemyAwareness)")]
        [SerializeField, Min(0.5f)] private float chaseSpeed = 4f;
        [Tooltip("Never chases farther than this from its post. 0 = no leash.")]
        [SerializeField, Min(0f)] private float leashDistance = 14f;
        [Tooltip("Solid layers that stop the robot (walls).")]
        [SerializeField] private LayerMask obstacleMask;
        [Tooltip("Layers the robot can stand on. It stops at ledges instead of walking off.")]
        [SerializeField] private LayerMask floorMask;

        private const float ArriveDistance = 0.15f;
        private const float LedgeProbeDepth = 0.9f;
        private const float RoomEdgeMargin = 1f;

        private Transform _player;
        private EnemyAwareness _awareness;
        private RoomBounds _room;
        private bool _externalMovement;
        private Vector3 _origin;
        private int _patrolDir = 1;
        private float _hesitationTimer;
        private float _immunityTimer;
        private Color _baseColor;

        public EnemyFaction Faction => faction;
        // A powered-down robot is not a valid Kitty target (it cannot hesitate while dormant either).
        public bool IsAffectedByCat => faction == EnemyFaction.CollectiveMachine && !IsDormant;
        public bool IsHesitating => _hesitationTimer > 0f;
        public bool IsDormant { get; private set; }
        /// <summary>Wind-up before an attack (pose/glow the player can read). Cancelled by hesitation.</summary>
        public bool IsTelegraphing { get; private set; }
        /// <summary>+1 facing right, -1 facing left.</summary>
        public int Facing { get; private set; } = 1;
        /// <summary>Post the robot patrols around and returns to.</summary>
        public Vector3 Origin => _origin;

        private static readonly Color HesitationColor = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color TelegraphColor = new Color(1f, 0.32f, 0.22f, 1f);
        private const float DormantDim = 0.45f;

        /// <summary>Color the body should show right now (lets hit flashes restore the correct state).</summary>
        public Color CurrentBodyColor
        {
            get
            {
                if (IsHesitating) return HesitationColor;
                if (IsTelegraphing) return TelegraphColor;
                if (IsDormant) return new Color(_baseColor.r * DormantDim, _baseColor.g * DormantDim, _baseColor.b * DormantDim, _baseColor.a);
                return _baseColor;
            }
        }

        private void Awake()
        {
            _origin = transform.position;
            _awareness = GetComponent<EnemyAwareness>();
            _externalMovement = GetComponent<FlyingDroneMotor>() != null;
            _room = GetComponentInParent<RoomBounds>();
            if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
            if (bodyRenderer != null) _baseColor = bodyRenderer.color;
            if (hesitationGlyph != null) hesitationGlyph.enabled = false;
            if (obstacleMask.value == 0) obstacleMask = LayerMask.GetMask("Ground");
            if (floorMask.value == 0) floorMask = LayerMask.GetMask("Ground", "OneWayPlatform");

            IsDormant = startDormant;
            ApplyBodyColor();
        }

        private void OnEnable()
        {
            // A re-entered room starts with its robots back at their posts (awareness resets too).
            if (!_externalMovement) transform.position = _origin;
            IsTelegraphing = false;
            ApplyBodyColor();
        }

        /// <summary>Powers the robot up: it starts patrolling and its contact damage turns on.</summary>
        public void Wake()
        {
            if (!IsDormant) return;
            IsDormant = false;
            ApplyBodyColor();
        }

        /// <summary>Shows or clears the attack wind-up tint. Ignored while hesitating or dormant.</summary>
        public void SetTelegraphing(bool on)
        {
            if (on && (IsHesitating || IsDormant)) return;
            if (IsTelegraphing == on) return;
            IsTelegraphing = on;
            ApplyBodyColor();
        }

        /// <summary>Turns the robot (and its sight) to the given side.</summary>
        public void SetFacing(int direction)
        {
            if (direction == 0) return;
            Facing = direction > 0 ? 1 : -1;
            if (_awareness != null) _awareness.Facing = Facing;
            if (bodyRenderer != null) bodyRenderer.flipX = Facing < 0;
        }

        public bool TryApplyCatInterference(float duration)
        {
            if (!IsAffectedByCat || IsHesitating || _immunityTimer > 0f) return false;

            _hesitationTimer = Mathf.Max(0.1f, duration);
            IsTelegraphing = false;
            if (hesitationGlyph != null) hesitationGlyph.enabled = true;
            ApplyBodyColor();
            return true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Ticks in every state so a robot that wakes up is not still immune from before.
            if (_immunityTimer > 0f)
            {
                _immunityTimer = Mathf.Max(0f, _immunityTimer - dt);
            }

            if (_hesitationTimer > 0f)
            {
                _hesitationTimer = Mathf.Max(0f, _hesitationTimer - dt);
                if (_hesitationTimer <= 0f)
                {
                    _immunityTimer = immunityCooldownAfterRecover;
                    if (hesitationGlyph != null) hesitationGlyph.enabled = false;
                    ApplyBodyColor();
                }
                return;
            }

            if (IsDormant)
            {
                if (wakeRadius > 0f)
                {
                    if (_player == null)
                    {
                        var playerGo = GameObject.FindGameObjectWithTag("Player");
                        if (playerGo != null) _player = playerGo.transform;
                    }
                    if (_player != null && (_player.position - transform.position).sqrMagnitude <= wakeRadius * wakeRadius)
                    {
                        Wake();
                    }
                }
                return;
            }

            if (_awareness != null) _awareness.Tick(dt);
            if (_externalMovement) return;

            switch (_awareness != null ? _awareness.State : EnemyAwarenessState.Idle)
            {
                case EnemyAwarenessState.Alerted:
                    FaceTowards(_awareness.LastKnownPosition.x);
                    break;

                case EnemyAwarenessState.Chasing:
                case EnemyAwarenessState.Searching:
                    MoveTowards(_awareness.LastKnownPosition.x, chaseSpeed, dt);
                    break;

                case EnemyAwarenessState.Returning:
                    if (MoveTowards(_origin.x, patrolSpeed, dt)) _awareness.NotifyReturned();
                    break;

                default:
                    Patrol(dt);
                    break;
            }
        }

        private void Patrol(float dt)
        {
            Vector3 pos = transform.position;
            pos.x += _patrolDir * patrolSpeed * dt;
            if (pos.x >= _origin.x + patrolHalfWidth)
            {
                pos.x = _origin.x + patrolHalfWidth;
                _patrolDir = -1;
            }
            else if (pos.x <= _origin.x - patrolHalfWidth)
            {
                pos.x = _origin.x - patrolHalfWidth;
                _patrolDir = 1;
            }
            transform.position = pos;
            SetFacing(_patrolDir);
        }

        private void FaceTowards(float targetX)
        {
            float dx = targetX - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f) SetFacing(dx > 0f ? 1 : -1);
        }

        /// <summary>Walks towards a world x. Returns true once it has arrived.</summary>
        private bool MoveTowards(float targetX, float speed, float dt)
        {
            Vector3 pos = transform.position;
            float dx = targetX - pos.x;
            if (Mathf.Abs(dx) <= ArriveDistance) return true;

            int dir = dx > 0f ? 1 : -1;
            SetFacing(dir);
            if (!CanAdvance(dir)) return false;

            pos.x += dir * Mathf.Min(speed * dt, Mathf.Abs(dx));
            transform.position = pos;
            _patrolDir = dir;
            return false;
        }

        /// <summary>No wall ahead, floor under the next step, inside the leash and inside the room.</summary>
        private bool CanAdvance(int dir)
        {
            Vector2 pos = transform.position;
            Vector2 extents = bodyRenderer != null ? (Vector2)bodyRenderer.bounds.extents : new Vector2(0.5f, 0.5f);
            float aheadX = pos.x + dir * (extents.x + 0.15f);

            if (leashDistance > 0f && Mathf.Abs(aheadX - _origin.x) > leashDistance
                && Mathf.Abs(aheadX - _origin.x) > Mathf.Abs(pos.x - _origin.x))
            {
                return false;
            }

            if (_room != null && !_room.ContainsPoint(new Vector2(aheadX + dir * RoomEdgeMargin, pos.y))) return false;

            if (Physics2D.Raycast(pos, new Vector2(dir, 0f), extents.x + 0.15f, obstacleMask).collider != null) return false;

            var feetAhead = new Vector2(aheadX, pos.y - extents.y + 0.25f);
            return Physics2D.Raycast(feetAhead, Vector2.down, LedgeProbeDepth, floorMask).collider != null;
        }

        private void ApplyBodyColor()
        {
            if (bodyRenderer != null) bodyRenderer.color = CurrentBodyColor;
        }
    }
}
