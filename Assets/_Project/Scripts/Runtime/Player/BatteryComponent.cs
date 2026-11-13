using System;
using UnityEngine;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Pure battery resource logic for ranged shots and exploration interactions.
    /// Unit-testable in EditMode.
    /// </summary>
    public sealed class BatteryModel
    {
        public float MaxCharge { get; private set; }
        public float CurrentCharge { get; private set; }

        public event Action<float, float> OnChargeChanged;

        public BatteryModel(float maxCharge = 100f)
        {
            MaxCharge = Mathf.Max(1f, maxCharge);
            CurrentCharge = MaxCharge;
        }

        public bool CanConsume(float amount) => amount > 0f && CurrentCharge >= amount;

        public bool TryConsume(float amount)
        {
            if (!CanConsume(amount)) return false;
            CurrentCharge = Mathf.Max(0f, CurrentCharge - amount);
            OnChargeChanged?.Invoke(CurrentCharge, MaxCharge);
            return true;
        }

        public void Recharge(float amount)
        {
            if (amount <= 0f) return;
            float next = Mathf.Min(MaxCharge, CurrentCharge + amount);
            if (Mathf.Approximately(next, CurrentCharge)) return;
            CurrentCharge = next;
            OnChargeChanged?.Invoke(CurrentCharge, MaxCharge);
        }

        public void RestoreFull()
        {
            CurrentCharge = MaxCharge;
            OnChargeChanged?.Invoke(CurrentCharge, MaxCharge);
        }
    }

    /// <summary>
    /// Tracks Dante's battery charge; consumed by ranged shots and recharged at shelters or via dropped cells.
    /// </summary>
    public sealed class BatteryComponent : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxCharge = 100f;

        private BatteryModel _model;

        public BatteryModel Model
        {
            get
            {
                EnsureModel();
                return _model;
            }
        }

        public float MaxCharge => Model.MaxCharge;
        public float CurrentCharge => Model.CurrentCharge;

        public event Action<float, float> OnChargeChanged;

        private void Awake() => EnsureModel();

        public bool CanConsume(float amount) => Model.CanConsume(amount);
        public bool TryConsume(float amount) => Model.TryConsume(amount);
        public void Recharge(float amount) => Model.Recharge(amount);
        public void RestoreFull() => Model.RestoreFull();

        private void EnsureModel()
        {
            if (_model != null) return;
            _model = new BatteryModel(maxCharge);
            _model.OnChargeChanged += (c, m) => OnChargeChanged?.Invoke(c, m);
        }
    }
}
