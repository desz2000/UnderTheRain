namespace UnderTheRain.Enemies
{
    public enum EnemyAwarenessState
    {
        /// <summary>Unaware: patrols its post.</summary>
        Idle,
        /// <summary>Just noticed Dante: short readable reaction pause before chasing.</summary>
        Alerted,
        /// <summary>Has Dante in sight and goes after him.</summary>
        Chasing,
        /// <summary>Lost sight: heads to the last known position until its memory runs out.</summary>
        Searching,
        /// <summary>Gave up: walks back to its post (still notices Dante on the way).</summary>
        Returning
    }

    /// <summary>
    /// Pure awareness state machine for hive robots (no Unity dependencies, EditMode testable).
    /// The owner feeds it "can I see the target right now?" every tick.
    /// </summary>
    public sealed class EnemyAwarenessModel
    {
        private readonly float _reactionTime;
        private readonly float _memoryDuration;
        private float _timer;

        public EnemyAwarenessState State { get; private set; } = EnemyAwarenessState.Idle;

        /// <summary>True while the robot is acting on Dante (reacting, chasing or searching).</summary>
        public bool IsEngaged => State == EnemyAwarenessState.Alerted
                                 || State == EnemyAwarenessState.Chasing
                                 || State == EnemyAwarenessState.Searching;

        public EnemyAwarenessModel(float reactionTime, float memoryDuration)
        {
            _reactionTime = reactionTime < 0f ? 0f : reactionTime;
            _memoryDuration = memoryDuration < 0f ? 0f : memoryDuration;
        }

        /// <summary>Advances the state machine. Returns true on the tick the robot first notices the target.</summary>
        public bool Tick(float dt, bool canSeeTarget)
        {
            switch (State)
            {
                case EnemyAwarenessState.Idle:
                case EnemyAwarenessState.Returning:
                    if (canSeeTarget)
                    {
                        EnterAlerted();
                        return true;
                    }
                    break;

                case EnemyAwarenessState.Alerted:
                    _timer -= dt;
                    if (_timer <= 0f) State = EnemyAwarenessState.Chasing;
                    break;

                case EnemyAwarenessState.Chasing:
                    if (!canSeeTarget)
                    {
                        State = EnemyAwarenessState.Searching;
                        _timer = _memoryDuration;
                    }
                    break;

                case EnemyAwarenessState.Searching:
                    if (canSeeTarget)
                    {
                        State = EnemyAwarenessState.Chasing;
                    }
                    else
                    {
                        _timer -= dt;
                        if (_timer <= 0f) State = EnemyAwarenessState.Returning;
                    }
                    break;
            }

            return false;
        }

        /// <summary>Alert from outside sight (took damage, hive call). Returns true if it changed the state.</summary>
        public bool ForceAlert()
        {
            if (State == EnemyAwarenessState.Idle || State == EnemyAwarenessState.Returning)
            {
                EnterAlerted();
                return true;
            }

            // Already engaged: a fresh report just renews the search memory.
            if (State == EnemyAwarenessState.Searching) _timer = _memoryDuration;
            return false;
        }

        /// <summary>The owner arrived back at its post.</summary>
        public void NotifyReturned()
        {
            if (State == EnemyAwarenessState.Returning) State = EnemyAwarenessState.Idle;
        }

        public void Reset()
        {
            State = EnemyAwarenessState.Idle;
            _timer = 0f;
        }

        private void EnterAlerted()
        {
            State = EnemyAwarenessState.Alerted;
            _timer = _reactionTime;
        }
    }
}
