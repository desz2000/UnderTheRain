using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.World
{
    /// <summary>
    /// Displays a lightweight contextual tutorial hint when Dante stands nearby.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class TutorialSignpost : MonoBehaviour
    {
        [SerializeField] private string hintText = "[A/D] Mover  |  [Espaço] Pular";

        private bool _playerInRange;
        private Camera _mainCamera;
        private GUIStyle _style;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _mainCamera = Camera.main;
        }

        public void Configure(string text)
        {
            hintText = text;
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

        private void OnGUI()
        {
            if (!_playerInRange) return;
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.85f, 0.94f, 0.98f, 1f) }
                };
            }

            Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
            if (screenPos.z < 0f) return;

            float guiY = Screen.height - screenPos.y;
            float width = Mathf.Max(220f, hintText.Length * 7.5f);
            GUI.Box(new Rect(screenPos.x - width * 0.5f, guiY - 14f, width, 28f), hintText, _style);
        }
    }
}
