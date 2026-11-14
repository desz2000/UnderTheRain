using UnityEngine;
using UnderTheRain.Core.Input;
using UnderTheRain.Narrative;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Interactive terminal that reveals a Domus Corp document and sets a narrative flag.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class NarrativeTerminal : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private NarrativeState narrativeState;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private DocumentDataSO document;
        [SerializeField] private DialogueSequenceSO readDialogue;
        [SerializeField] private string flagOnRead = NarrativeState.FlagDomusEvidenceFound;
        [SerializeField] private string promptLabel = "[E] Acessar Terminal";
        [SerializeField] private SpriteRenderer statusIndicator;
        [SerializeField] private Color unreadColor = new Color(0.306f, 0.804f, 0.769f, 1f);
        [SerializeField] private Color readColor = new Color(0.35f, 0.45f, 0.50f, 1f);

        private bool _playerInRange;
        private Camera _mainCamera;
        private GUIStyle _promptStyle;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _mainCamera = Camera.main;
            UpdateIndicator();
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
            DocumentDataSO doc,
            DialogueSequenceSO dialogue)
        {
            if (input != null) input.OnInteractPressed -= HandleInteractPressed;
            input = inputReader;
            narrativeState = state;
            dialogueRunner = runner;
            document = doc;
            readDialogue = dialogue;
            if (isActiveAndEnabled && input != null)
            {
                input.OnInteractPressed += HandleInteractPressed;
            }
            UpdateIndicator();
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
            if (dialogueRunner != null && dialogueRunner.IsBusy) return;

            if (narrativeState != null)
            {
                if (document != null) narrativeState.CollectDocument(document);
                if (!string.IsNullOrEmpty(flagOnRead)) narrativeState.SetFlag(flagOnRead);
            }

            UpdateIndicator();

            if (dialogueRunner != null && readDialogue != null)
            {
                dialogueRunner.Play(readDialogue);
            }
        }

        private void UpdateIndicator()
        {
            if (statusIndicator == null) return;
            bool alreadyRead = narrativeState != null && document != null && narrativeState.HasDocument(document);
            statusIndicator.color = alreadyRead ? readColor : unreadColor;
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

            Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 1.4f);
            if (screenPos.z < 0f) return;

            float guiY = Screen.height - screenPos.y;
            GUI.Box(new Rect(screenPos.x - 90f, guiY - 14f, 180f, 26f), promptLabel, _promptStyle);
        }
    }
}
