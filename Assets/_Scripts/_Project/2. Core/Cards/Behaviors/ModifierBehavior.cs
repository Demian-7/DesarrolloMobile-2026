namespace Game.Core
{
    public class ModifierBehavior : ICardBehavior
    {
        public readonly double Multiplier;

        public ModifierBehavior(double amount)
        {
            Multiplier = amount;
        }
    }
}