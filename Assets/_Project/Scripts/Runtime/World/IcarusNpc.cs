using UnityEngine;
using UnderTheRain.Core.Input;
using UnderTheRain.Narrative;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Icarus, leader of the sewer resistance. Triggers the Act I confrontation once Dante
    /// presents Domus Corp evidence, unlocking the ascent toward the surface.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class IcarusNpc : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private NarrativeState narrativeState;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private DialogueSequenceSO beforeEvidenceDialogue;
        [SerializeField] private DialogueSequenceSO confrontationDialogue;
        [SerializeField] private DialogueSequenceSO afterConfrontationDialogue;
        [SerializeField] private string promptLabel = "[E] Falar com Icarus";

        private bool _playerInRange;
        private Camera _mainCamera;
        private GUIStyle _promptStyle;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (input != null) input.OnInteractPressed += HandleInteractPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnInteractPressed -= HandleInteractPressed;
        }

        public void Configure(
            GameInputReader inputReader,
            NarrativeState state,
            DialogueRunner runner,
            DialogueSequenceSO beforeEvidence,
            DialogueSequenceSO confrontation,
            DialogueSequenceSO afterConfrontation)
        {
            if (input != null) input.OnInteractPressed -= HandleInteractPressed;
            input = inputReader;
            narrativeState = state;
            dialogueRunner = runner;
            beforeEvidenceDialogue = beforeEvidence;
            confrontationDialogue = confrontation;
            afterConfrontationDialogue = afterConfrontation;
            if (isActiveAndEnabled && input != null)
            {
                input.OnInteractPressed += HandleInteractPressed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                _playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                _playerInRange = false;
            }
        }

        private void HandleInteractPressed()
        {
            if (!_playerInRange) return;
            if (dialogueRunner == null || dialogueRunner.IsBusy) return;

            bool hasEvidence = narrativeState != null && narrativeState.HasFlag(NarrativeState.FlagDomusEvidenceFound);
            bool confronted = narrativeState != null && narrativeState.HasFlag(NarrativeState.FlagIcarusConfronted);

            if (!hasEvidence)
            {
                dialogueRunner.Play(beforeEvidenceDialogue);
                return;
            }

            if (!confronted)
            {
                dialogueRunner.Play(confrontationDialogue, () =>
                {
                    if (narrativeState != null)
                    {
                        narrativeState.SetFlag(NarrativeState.FlagIcarusConfronted);
                        narrativeState.SetFlag(NarrativeState.FlagSewerExitUnlocked);
                    }
                });
                return;
            }

            dialogueRunner.Play(afterConfrontationDialogue);
        }

        private void OnGUI()
        {
            if (!_playerInRange) return;
            if (dialogueRunner != null && dialogueRunner.IsBusy) return;
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            if (_promptStyle == null)
            {
                _promptStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }

            Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
            if (screenPos.z < 0f) return;

            float guiY = Screen.height - screenPos.y;
            GUI.Box(new Rect(screenPos.x - 90f, guiY - 14f, 180f, 26f), promptLabel, _promptStyle);
        }
    }
}
