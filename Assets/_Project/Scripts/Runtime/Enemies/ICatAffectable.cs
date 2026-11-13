namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Implemented by enemies or machines that may react to the metal cat's neural interference pulse.
    /// Only CollectiveMachine targets should accept interference.
    /// </summary>
    public interface ICatAffectable
    {
        bool IsAffectedByCat { get; }
        bool IsHesitating { get; }
        bool TryApplyCatInterference(float duration);
    }
}
