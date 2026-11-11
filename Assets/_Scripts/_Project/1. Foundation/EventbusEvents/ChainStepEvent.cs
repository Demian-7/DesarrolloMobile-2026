namespace Game.Foundation
{
    public readonly struct ChainStepEvent
    {
        public readonly string ZoneId;
        public readonly CurrencyValue RunningTotal;
        public readonly int Step;
        public readonly int StepCount;

        public bool IsFinal => Step >= StepCount - 1;

        public ChainStepEvent(string zoneId, CurrencyValue runningTotal, int step, int stepCount)
        {
            ZoneId = zoneId;
            RunningTotal = runningTotal;
            Step = step;
            StepCount = stepCount;
        }
    }
}