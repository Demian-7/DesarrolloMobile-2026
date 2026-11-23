namespace Game.Foundation
{
    public readonly struct BoostStateChangedEvent
    {
        public readonly bool IsActive;
        public readonly float Multiplier;
        public readonly float RemainingSeconds;

        public BoostStateChangedEvent(bool isActive, float multiplier, float remainingSeconds)
        {
            IsActive = isActive;
            Multiplier = multiplier;
            RemainingSeconds = remainingSeconds;
        }
    }
}