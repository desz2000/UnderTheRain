using System;
using UnityEngine;
using UnderTheRain.Core.Input;
using UnderTheRain.Player;

namespace UnderTheRain.Narrative
{
    /// <summary>
    /// Minimalist neo-noir dialogue runner for Story 5.1 (uses IMGUI until Epic 6 uGUI polish).
    /// Locks player locomotion while active and advances lines via the Interact input.
    /// </summary>
    public sealed class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private InputDeviceTracker deviceTracker;

        private DialogueSequenceSO _activeSequence;
        private int _lineIndex;
        private int _openedFrame = -1;
        private Action _onComplete;

        private GUIStyle _boxStyle;
        private GUIStyle _speakerStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _hintStyle;
        private Texture2D _bgTex;

        public bool IsPlaying => _activeSequence != null;

        private void OnEnable()
        {
            if (input != null)
            {
                input.OnInteractPressed += HandleInteractPressed;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.OnInteractPressed -= HandleInteractPressed;
            }
        }

        private void OnDestroy()
        {
            if (_bgTex != null)
            {
                Destroy(_bgTex);
                _bgTex = null;
            }
        }

        public void Initialize(GameInputReader inputReader, PlayerController player, InputDeviceTracker tracker = null)
        {
            if (input != null) input.OnInteractPressed -= HandleInteractPressed;
            input = inputReader;
            playerController = player;
            deviceTracker = tracker;
            if (isActiveAndEnabled && input != null)
            {
                input.OnInteractPressed += HandleInteractPressed;
            }
        }

        public bool Play(DialogueSequenceSO sequence, Action onComplete = null)
        {
            if (IsPlaying || sequence == null || sequence.lines == null || sequence.lines.Count == 0)
            {
                return false;
            }

            _activeSequence = sequence;
            _lineIndex = 0;
            _openedFrame = Time.frameCount;
            _onComplete = onComplete;
            playerController?.SetControlLocked(true);
            return true;
        }

        private void HandleInteractPressed()
        {
            if (!IsPlaying || Time.frameCount == _openedFrame) return;

            _lineIndex++;
            if (_lineIndex >= _activeSequence.lines.Count)
            {
                CompleteDialogue();
            }
        }

        private void CompleteDialogue()
        {
            var callback = _onComplete;
            _activeSequence = null;
            _lineIndex = 0;
            _onComplete = null;
            playerController?.SetControlLocked(false);
            callback?.Invoke();
        }

        private void OnGUI()
        {
            if (!IsPlaying) return;

            EnsureStyles();
            DialogueLine line = _activeSequence.lines[_lineIndex];

            float width = Mathf.Min(920f, Screen.width - 80f);
            float height = 165f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 36f;

            GUI.Box(new Rect(x, y, width, height), GUIContent.none, _boxStyle);

            _speakerStyle.normal.textColor = line.speakerColor.a > 0.05f
                ? line.speakerColor
                : new Color(0.306f, 0.804f, 0.769f, 1f);

            GUI.Label(new Rect(x + 22f, y + 16f, width - 44f, 28f), line.speakerName, _speakerStyle);
            GUI.Label(new Rect(x + 22f, y + 48f, width - 44f, 82f), line.text, _bodyStyle);

            string promptKey = deviceTracker != null && deviceTracker.CurrentDevice == InputDeviceType.Gamepad
                ? "[Y / Triângulo]"
                : "[E]";
            string suffix = _lineIndex + 1 < _activeSequence.lines.Count ? "Continuar" : "Fechar";
            GUI.Label(new Rect(x + 22f, y + height - 32f, width - 44f, 22f),
                $"{promptKey} {suffix} ({_lineIndex + 1}/{_activeSequence.lines.Count})", _hintStyle);
        }

        private void EnsureStyles()
        {
            if (_bgTex == null)
            {
                _bgTex = new Texture2D(1, 1);
                _bgTex.SetPixel(0, 0, new Color(0.05f, 0.06f, 0.08f, 0.92f));
                _bgTex.Apply();
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = _bgTex }
                };
            }

            if (_speakerStyle == null)
            {
                _speakerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold
                };
            }

            if (_bodyStyle == null)
            {
                _bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    wordWrap = true,
                    normal = { textColor = new Color(0.92f, 0.94f, 0.96f, 1f) }
                };
            }

            if (_hintStyle == null)
            {
                _hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(0.65f, 0.72f, 0.78f, 1f) }
                };
            }
        }
    }
}
