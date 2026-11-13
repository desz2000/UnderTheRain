using System;
using UnityEngine;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Pure discrete health + post-damage invulnerability state.
    /// Wrapped by <see cref="HealthComponent"/> and unit-testable in EditMode.
    /// </summary>
    public sealed class HealthModel
    {
        private readonly float _invulnerabilityDuration;
        private float _invulnerabilityTimer;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        public event Action<int, int> OnHealthChanged;
        public event Action<DamageContext> OnDamaged;
        public event Action OnDied;

        public HealthModel(int maxHealth, float invulnerabilityDuration = 0.5f)
        {
            MaxHealth = Mathf.Max(1, maxHealth);
            CurrentHealth = MaxHealth;
            _invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
        }

        public void Tick(float dt)
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer = Mathf.Max(0f, _invulnerabilityTimer - dt);
            }
        }

        public bool TryTakeDamage(DamageContext context)
        {
            if (IsDead || IsInvulnerable || context.amount <= 0) return false;

            CurrentHealth = Mathf.Max(0, CurrentHealth - context.amount);
            _invulnerabilityTimer = _invulnerabilityDuration;

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnDamaged?.Invoke(context);

            if (IsDead)
            {
                OnDied?.Invoke();
            }

            return true;
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            int next = Mathf.Min(MaxHealth, CurrentHealth + amount);
            if (next == CurrentHealth) return;

            CurrentHealth = next;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void RestoreFull()
        {
            _invulnerabilityTimer = 0f;
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }

    /// <summary>
    /// Discrete health cells with post-damage invulnerability window (~0.5s).
    /// </summary>
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 5;
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.5f;

        private HealthModel _model;

        public HealthModel Model
        {
            get
            {
                EnsureModel();
                return _model;
            }
        }

        public int MaxHealth => Model.MaxHealth;
        public int CurrentHealth => Model.CurrentHealth;
        public bool IsDead => Model.IsDead;
        public bool IsInvulnerable => Model.IsInvulnerable;

        public event Action<int, int> OnHealthChanged;
        public event Action<DamageContext> OnDamaged;
        public event Action OnDied;

        private void Awake()
        {
            EnsureModel();
        }

        private void Update()
        {
            _model?.Tick(Time.deltaTime);
        }

        public bool TryTakeDamage(DamageContext context) => Model.TryTakeDamage(context);
        public void Heal(int amount) => Model.Heal(amount);
        public void RestoreFull() => Model.RestoreFull();

        private void EnsureModel()
        {
            if (_model != null) return;
            _model = new HealthModel(maxHealth, invulnerabilityDuration);
            _model.OnHealthChanged += (c, m) => OnHealthChanged?.Invoke(c, m);
            _model.OnDamaged += ctx => OnDamaged?.Invoke(ctx);
            _model.OnDied += () => OnDied?.Invoke();
        }
    }
}
