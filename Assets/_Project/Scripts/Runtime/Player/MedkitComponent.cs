using System;
using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Pure medkit charge logic: starts with fixed charges, prevents waste at full health,
    /// and restores fully at shelters. Unit-testable in EditMode.
    /// </summary>
    public sealed class MedkitModel
    {
        private readonly int _healAmount;

        public int MaxCharges { get; private set; }
        public int CurrentCharges { get; private set; }

        public event Action<int, int> OnChargesChanged;

        public MedkitModel(int maxCharges = 3, int healAmount = 2)
        {
            MaxCharges = Mathf.Max(1, maxCharges);
            CurrentCharges = MaxCharges;
            _healAmount = Mathf.Max(1, healAmount);
        }

        public bool TryUse(HealthModel health)
        {
            if (health == null || CurrentCharges <= 0 || health.IsDead || health.CurrentHealth >= health.MaxHealth)
            {
                return false;
            }

            CurrentCharges--;
            health.Heal(_healAmount);
            OnChargesChanged?.Invoke(CurrentCharges, MaxCharges);
            return true;
        }

        public bool TryAddCharge(int amount = 1)
        {
            if (amount <= 0 || CurrentCharges >= MaxCharges) return false;
            CurrentCharges = Mathf.Min(MaxCharges, CurrentCharges + amount);
            OnChargesChanged?.Invoke(CurrentCharges, MaxCharges);
            return true;
        }

        public void RestoreFull()
        {
            CurrentCharges = MaxCharges;
            OnChargesChanged?.Invoke(CurrentCharges, MaxCharges);
        }
    }

    /// <summary>
    /// Consumes a medkit charge to restore Dante's health on heal input.
    /// </summary>
    public sealed class MedkitComponent : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField, Min(1)] private int maxCharges = 3;
        [SerializeField, Min(1)] private int healAmount = 2;

        private MedkitModel _model;

        public MedkitModel Model
        {
            get
            {
                EnsureModel();
                return _model;
            }
        }

        public int MaxCharges => Model.MaxCharges;
        public int CurrentCharges => Model.CurrentCharges;

        public event Action<int, int> OnChargesChanged;

        private void Awake()
        {
            if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
            EnsureModel();
        }

        private void OnEnable()
        {
            if (input != null) input.OnHealPressed += HandleHealPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnHealPressed -= HandleHealPressed;
        }

        private void HandleHealPressed()
        {
            if (TryUse())
            {
                Debug.Log($"[Medkit] Used! Health: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}, Charges: {CurrentCharges}/{MaxCharges}");
            }
            else if (healthComponent != null && healthComponent.IsDead)
            {
                Debug.Log("[Medkit] Dante is dead (0 HP). Rest at the Shelter (E) to revive.");
            }
            else if (healthComponent != null && healthComponent.CurrentHealth >= healthComponent.MaxHealth)
            {
                Debug.Log("[Medkit] Health already full.");
            }
            else
            {
                Debug.Log("[Medkit] No charges left.");
            }
        }

        public bool TryUse() => healthComponent != null && Model.TryUse(healthComponent.Model);
        public bool TryAddCharge(int amount = 1) => Model.TryAddCharge(amount);
        public void RestoreFull() => Model.RestoreFull();

        private void EnsureModel()
        {
            if (_model != null) return;
            _model = new MedkitModel(maxCharges, healAmount);
            _model.OnChargesChanged += (c, m) => OnChargesChanged?.Invoke(c, m);
        }
    }
}
