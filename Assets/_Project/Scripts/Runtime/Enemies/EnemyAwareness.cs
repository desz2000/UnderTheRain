using System.Collections.Generic;
using UnityEngine;
using UnderTheRain.Combat;

namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Sight and awareness of a hive robot: a vision cone in front, a small all-around radius up close,
    /// line of sight blocked by solid ground, and a hive call that alerts nearby robots (shared mind).
    /// Passive component: the robot's core (<see cref="PatrolInterferenceTarget"/>) ticks it, so a
    /// dormant or hesitating robot perceives nothing.
    /// </summary>
    public sealed class EnemyAwareness : MonoBehaviour
    {
        [Header("Sight")]
        [SerializeField, Min(0.5f)] private float viewRange = 9f;
        [Tooltip("Half angle of the vision cone in front of the robot.")]
        [SerializeField, Range(5f, 180f)] private float viewHalfAngle = 50f;
        [Tooltip("Dante is noticed inside this radius even from behind.")]
        [SerializeField, Min(0f)] private float closeAwarenessRadius = 2.2f;
        [Tooltip("Once engaged the robot sees all around, up to viewRange times this.")]
        [SerializeField, Min(1f)] private float engagedRangeMultiplier = 1.35f;
        [Tooltip("Solid layers that block line of sight (Ground). One-way platforms do not block.")]
        [SerializeField] private LayerMask sightBlockers;
        [SerializeField] private Vector2 eyeOffset = new Vector2(0f, 0.3f);

        [Header("Reaction")]
        [Tooltip("Readable pause (alert glyph) between noticing Dante and chasing him.")]
        [SerializeField, Min(0f)] private float reactionTime = 0.35f;
        [Tooltip("How long it keeps looking after losing sight before going back to its post.")]
        [SerializeField, Min(0f)] private float memoryDuration = 2.5f;
        [Tooltip("Robots this close are alerted too when this one notices Dante. 0 = no hive call.")]
        [SerializeField, Min(0f)] private float hiveAlertRadius = 7f;
        [SerializeField] private SpriteRenderer alertGlyph;

        private static readonly List<EnemyAwareness> s_active = new List<EnemyAwareness>();

        private EnemyAwarenessModel _model;
        private PatrolInterferenceTarget _core;
        private Transform _target;
        private HealthComponent _targetHealth;

        public EnemyAwarenessState State => Model.State;
        public bool IsEngaged => Model.IsEngaged;
        /// <summary>True when the last tick had a clear view of Dante.</summary>
        public bool HasTargetInSight { get; private set; }
        /// <summary>Where Dante was last seen (or reported by the hive).</summary>
        public Vector2 LastKnownPosition { get; private set; }
        /// <summary>+1 looking right, -1 looking left. Set by whoever moves the robot.</summary>
        public int Facing { get; set; } = 1;
        public float ViewRange => viewRange;

        private EnemyAwarenessModel Model => _model ??= new EnemyAwarenessModel(reactionTime, memoryDuration);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => s_active.Clear();

        private void Awake()
        {
            _core = GetComponent<PatrolInterferenceTarget>();
            if (sightBlockers.value == 0) sightBlockers = LayerMask.GetMask("Ground");
            if (alertGlyph != null) alertGlyph.enabled = false;
        }

        private void OnEnable()
        {
            // A room that is re-entered starts with its robots unaware again.
            Model.Reset();
            HasTargetInSight = false;
            s_active.Add(this);
        }

        private void OnDisable()
        {
            s_active.Remove(this);
            if (alertGlyph != null) alertGlyph.enabled = false;
        }

        /// <summary>Runs perception and the state machine. Called by the robot's core while it is awake.</summary>
        public void Tick(float dt)
        {
            HasTargetInSight = ComputeCanSee();
            if (HasTargetInSight) LastKnownPosition = _target.position;

            if (Model.Tick(dt, HasTargetInSight)) CallHive();

            if (alertGlyph != null) alertGlyph.enabled = Model.State == EnemyAwarenessState.Alerted;
        }

        /// <summary>Alert from damage: the robot turns on Dante even without seeing him.</summary>
        public void AlertToTarget()
        {
            if (!ResolveTarget()) return;
            ReceiveAlert(_target.position);
        }

        /// <summary>Overrides the vision cone (e.g. drones scan the floor below them, not just ahead).</summary>
        public void SetViewHalfAngle(float halfAngle) => viewHalfAngle = Mathf.Clamp(halfAngle, 5f, 180f);

        /// <summary>The mover arrived back at the post.</summary>
        public void NotifyReturned() => Model.NotifyReturned();

        private void ReceiveAlert(Vector2 reportedPosition)
        {
            if (!isActiveAndEnabled) return;
            if (_core != null && _core.IsDormant) return;

            LastKnownPosition = reportedPosition;
            Model.ForceAlert();
        }

        private void CallHive()
        {
            if (hiveAlertRadius <= 0f) return;

            float sqrRadius = hiveAlertRadius * hiveAlertRadius;
            Vector2 self = transform.position;
            for (int i = 0; i < s_active.Count; i++)
            {
                var other = s_active[i];
                if (other == this) continue;
                if (((Vector2)other.transform.position - self).sqrMagnitude > sqrRadius) continue;
                other.ReceiveAlert(LastKnownPosition);
            }
        }

        private bool ComputeCanSee()
        {
            if (!ResolveTarget()) return false;
            if (_targetHealth != null && _targetHealth.IsDead) return false;

            Vector2 eye = (Vector2)transform.position + eyeOffset;
            Vector2 toTarget = (Vector2)_target.position - eye;
            float distance = toTarget.magnitude;

            bool engaged = Model.IsEngaged;
            if (distance > (engaged ? viewRange * engagedRangeMultiplier : viewRange)) return false;

            if (!engaged && distance > closeAwarenessRadius)
            {
                float angle = Vector2.Angle(new Vector2(Facing >= 0 ? 1f : -1f, 0f), toTarget);
                if (angle > viewHalfAngle) return false;
            }

            return Physics2D.Linecast(eye, _target.position, sightBlockers).collider == null;
        }

        private bool ResolveTarget()
        {
            if (_target != null) return true;

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo == null) return false;

            _target = playerGo.transform;
            _targetHealth = playerGo.GetComponent<HealthComponent>();
            return true;
        }
    }
}
