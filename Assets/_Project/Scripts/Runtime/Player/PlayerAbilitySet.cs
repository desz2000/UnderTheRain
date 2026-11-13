using System;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Tracks unlocked movement/traversal abilities on the player state.
    /// Persisted by the save system and checked by <see cref="PlayerMotor"/>.
    /// </summary>
    [Serializable]
    public struct PlayerAbilitySet
    {
        public bool wallJump;
    }
}
