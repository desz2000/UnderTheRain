using System;
using UnityEngine;
using UnderTheRain.Core.Input;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Pure parts-pack economy logic: bounded storage, spending, medkit crafting,
    /// and full extraction on death for <see cref="World.DroppedPack"/>.
    /// </summary>
    public sealed class PartsPackModel
    {
        public int MaxCapacity { get; private set; }
        public int CurrentParts { get; private set; }

        public event Action<int, int> OnPartsChanged;

        public PartsPackModel(int maxCapacity = 50, int initialParts = 0)
        {
            MaxCapacity = Mathf.Max(1, maxCapacity);
            CurrentParts = Mathf.Clamp(initialParts, 0, MaxCapacity);
        }

        public int AddParts(int amount)
        {
            if (amount <= 0 || CurrentParts >= MaxCapacity) return 0;

            int added = Mathf.Min(amount, MaxCapacity - CurrentParts);
            CurrentParts += added;
            OnPartsChanged?.Invoke(CurrentParts, MaxCapacity);
            return added;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || CurrentParts < amount) return false;

            CurrentParts -= amount;
            OnPartsChanged?.Invoke(CurrentParts, MaxCapacity);
            return true;
        }

        public bool TryCraftMedkit(MedkitModel medkit, int cost)
        {
            if (medkit == null || cost <= 0 || CurrentParts < cost) return false;
            if (medkit.CurrentCharges >= medkit.MaxCharges) return false;

            if (!medkit.TryAddCharge(1)) return false;
            TrySpend(cost);
            return true;
        }

        public int ExtractAllOnDeath()
        {
            int dropped = CurrentParts;
            if (dropped > 0)
            {
                CurrentParts = 0;
                OnPartsChanged?.Invoke(CurrentParts, MaxCapacity);
            }
            return dropped;
        }

        public void ExpandCapacity(int extraCapacity)
        {
            if (extraCapacity <= 0) return;
            MaxCapacity += extraCapacity;
            OnPartsChanged?.Invoke(CurrentParts, MaxCapacity);
        }
    }

    /// <summary>
    /// Stores collected mechanical parts up to <see cref="MaxCapacity"/> and supports crafting medkits.
    /// </summary>
    public sealed class PartsPackComponent : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private MedkitComponent medkitComponent;
        [SerializeField, Min(1)] private int maxCapacity = 50;
        [SerializeField, Min(1)] private int medkitCraftCost = 15;

        private PartsPackModel _model;

        public PartsPackModel Model
        {
            get
            {
                EnsureModel();
                return _model;
            }
        }

        public int MaxCapacity => Model.MaxCapacity;
        public int CurrentParts => Model.CurrentParts;

        public event Action<int, int> OnPartsChanged;

        private void Awake()
        {
            if (medkitComponent == null) medkitComponent = GetComponent<MedkitComponent>();
            EnsureModel();
        }

        private void OnEnable()
        {
            if (input != null) input.OnCraftPressed += HandleCraftPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnCraftPressed -= HandleCraftPressed;
        }

        private void HandleCraftPressed()
        {
            if (medkitComponent == null) return;
            if (Model.TryCraftMedkit(medkitComponent.Model, medkitCraftCost))
            {
                Debug.Log($"[PartsPack] Crafted 1 medkit for {medkitCraftCost} parts. Parts left: {CurrentParts}/{MaxCapacity}");
            }
            else
            {
                Debug.Log($"[PartsPack] Cannot craft medkit (need {medkitCraftCost} parts and < {medkitComponent.MaxCharges} charges).");
            }
        }

        public int AddParts(int amount) => Model.AddParts(amount);
        public bool TrySpend(int amount) => Model.TrySpend(amount);
        public int ExtractAllOnDeath() => Model.ExtractAllOnDeath();
        public void ExpandCapacity(int extraCapacity) => Model.ExpandCapacity(extraCapacity);

        private void EnsureModel()
        {
            if (_model != null) return;
            _model = new PartsPackModel(maxCapacity);
            _model.OnPartsChanged += (c, m) => OnPartsChanged?.Invoke(c, m);
        }
    }
}
