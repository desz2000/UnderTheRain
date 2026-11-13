using UnityEngine;
using UnderTheRain.Narrative;

namespace UnderTheRain.World
{
    /// <summary>
    /// Physical barrier that opens once a required narrative flag is set.
    /// </summary>
    public sealed class NarrativeGate : MonoBehaviour
    {
        [SerializeField] private NarrativeState narrativeState;
        [SerializeField] private string requiredFlag = NarrativeState.FlagSewerExitUnlocked;

        private bool _isOpen;

        private void OnEnable()
        {
            if (narrativeState != null)
            {
                narrativeState.OnFlagSet += HandleFlagSet;
            }
        }

        private void OnDisable()
        {
            if (narrativeState != null)
            {
                narrativeState.OnFlagSet -= HandleFlagSet;
            }
        }

        private void Start()
        {
            if (narrativeState != null && narrativeState.HasFlag(requiredFlag))
            {
                OpenGate();
            }
        }

        public void Configure(NarrativeState state, string flag)
        {
            if (narrativeState != null) narrativeState.OnFlagSet -= HandleFlagSet;
            narrativeState = state;
            requiredFlag = flag;
            if (isActiveAndEnabled && narrativeState != null)
            {
                narrativeState.OnFlagSet += HandleFlagSet;
                if (narrativeState.HasFlag(requiredFlag))
                {
                    OpenGate();
                }
            }
        }

        private void HandleFlagSet(string flag)
        {
            if (flag == requiredFlag)
            {
                OpenGate();
            }
        }

        private void OpenGate()
        {
            if (_isOpen) return;
            _isOpen = true;
            gameObject.SetActive(false);
        }
    }
}
