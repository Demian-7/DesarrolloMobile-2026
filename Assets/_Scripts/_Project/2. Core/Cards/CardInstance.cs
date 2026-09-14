using System;
using System.Collections.Generic;
using Game.Foundation;

namespace Game.Core
{
    public class CardInstance
    {
        public Guid InstanceId = Guid.NewGuid();
        public CardDefinition Definition;
        public Cell CurrentCell;
    
        public CurrencyValue BaseValue;
        public CurrencyValue EffectiveValue;
        public bool IsResolved;

        public CardInstance(CardDefinition definition)
        {
            Definition = definition;
        }

        public IEnumerable<T> GetEffects<T>() where T : class, ICardEffect
        {
            foreach (var effect in Definition.Effects)
            {
                if (effect is T match)
                    yield return match;
            }
        }
    }
}