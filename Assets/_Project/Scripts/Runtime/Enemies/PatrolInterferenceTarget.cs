using UnityEngine;

namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Simple patrolling actor for validating cat interference behavior:
    /// CollectiveMachine targets hesitate and spark when pulsed; MisanthropeHuman targets ignore it.
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

        private Transform _player;
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

        private static readonly Color HesitationColor = new Color(1f, 0.85f, 0.3f, 1f);
        private const float DormantDim = 0.45f;

        /// <summary>Color the body should show right now (lets hit flashes restore the correct state).</summary>
        public Color CurrentBodyColor => IsHesitating
            ? HesitationColor
            : IsDormant ? new Color(_baseColor.r * DormantDim, _baseColor.g * DormantDim, _baseColor.b * DormantDim, _baseColor.a) : _baseColor;

        private void Awake()
        {
            _origin = transform.position;
            if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
            if (bodyRenderer != null) _baseColor = bodyRenderer.color;
            if (hesitationGlyph != null) hesitationGlyph.enabled = false;

            IsDormant = startDormant;
            if (bodyRenderer != null) bodyRenderer.color = CurrentBodyColor;
        }

        /// <summary>Powers the robot up: it starts patrolling and its contact damage turns on.</summary>
        public void Wake()
        {
            if (!IsDormant) return;
            IsDormant = false;
            if (bodyRenderer != null) bodyRenderer.color = CurrentBodyColor;
        }

        public bool TryApplyCatInterference(float duration)
        {
            if (!IsAffectedByCat || IsHesitating || _immunityTimer > 0f) return false;

            _hesitationTimer = Mathf.Max(0.1f, duration);
            if (hesitationGlyph != null) hesitationGlyph.enabled = true;
            if (bodyRenderer != null) bodyRenderer.color = HesitationColor;
            return true;
        }

        private void Update()
        {
            // Ticks in every state so a robot that wakes up is not still immune from before.
            if (_immunityTimer > 0f)
            {
                _immunityTimer = Mathf.Max(0f, _immunityTimer - Time.deltaTime);
            }

            if (_hesitationTimer > 0f)
            {
                _hesitationTimer = Mathf.Max(0f, _hesitationTimer - Time.deltaTime);
                if (_hesitationTimer <= 0f)
                {
                    _immunityTimer = immunityCooldownAfterRecover;
                    if (hesitationGlyph != null) hesitationGlyph.enabled = false;
                    if (bodyRenderer != null) bodyRenderer.color = CurrentBodyColor;
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

            Vector3 pos = transform.position;
            pos.x += _patrolDir * patrolSpeed * Time.deltaTime;
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
        }
    }
}
