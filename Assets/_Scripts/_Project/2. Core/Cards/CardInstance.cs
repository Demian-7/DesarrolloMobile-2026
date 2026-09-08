using System.Collections.Generic;
using Game.Foundation;

namespace Game.Core
{
    public class CardInstance
    {
        public string Id;
        public List<ICardBehavior> Behaviors = new();
        public Cell CurrentCell;
    
        //Runtime state calculated by the Evaluation Service
        public CurrencyValue BaseValue;
        public CurrencyValue EffectiveValue;
        public bool IsResolved;

        public CardInstance(string id, params ICardBehavior[] behaviors)
        {
            Id = id;
            Behaviors.AddRange(behaviors);
        }

        public T GetBehavior<T>() where T : class, ICardBehavior
        {
            foreach (var b in Behaviors)
                if (b is T match)
                    return match;

            return null;
        }
    }
}