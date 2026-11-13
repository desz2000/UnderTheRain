using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.Combat
{
    /// <summary>
    /// Minimal IMGUI overlay for combat test scenes before the diegetic uGUI HUD (Epic 6).
    /// </summary>
    public sealed class DebugCombatHud : MonoBehaviour
    {
        private HealthComponent _health;
        private BatteryComponent _battery;
        private MedkitComponent _medkit;
        private GUIStyle _style;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _battery = GetComponent<BatteryComponent>();
            _medkit = GetComponent<MedkitComponent>();
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    alignment = TextAnchor.UpperLeft,
                    normal = { textColor = Color.white }
                };
            }

            string hp = _health != null
                ? (_health.IsDead ? $"0/{_health.MaxHealth} (DEAD - Rest at Shelter)" : $"{_health.CurrentHealth}/{_health.MaxHealth}")
                : "-";
            string bat = _battery != null ? $"{_battery.CurrentCharge:0}/{_battery.MaxCharge:0}" : "-";
            string kits = _medkit != null ? $"{_medkit.CurrentCharges}/{_medkit.MaxCharges}" : "-";

            string text = $"HP: {hp}   |   Battery: {bat}   |   Medkits: {kits}\n" +
                          "LMB: Melee  |  RMB / F: Battery Shot  |  Q: Medkit  |  E: Rest (Shelter)";
            GUI.Box(new Rect(12, 12, 560, 52), text, _style);
        }
    }
}
