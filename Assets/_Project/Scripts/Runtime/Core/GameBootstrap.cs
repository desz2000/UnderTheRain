using UnityEngine;

namespace UnderTheRain.Core
{
    /// <summary>
    /// Entry point for the persistent Core scene. Ensures session-level
    /// systems remain alive across room transitions.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
