namespace Game.Core
{
    public class PercentageGenerationBehavior : ICardBehavior
    {
        public readonly double Percentage;

        public PercentageGenerationBehavior(double percentage)
        {
            Percentage = percentage;
        }
    }
}