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

        /// <summary>0 = flat generators, 1+ = later passes of the chain, -1 = never resolved.</summary>
        public int ResolvedPass = -1;

        public CardInstance(CardDefinition definition)
        {
            Definition = definition;
        }

        public IEnumerable<T> GetEffects<T>() where T : CardEffect
        {
            foreach (var effect in Definition.Effects)
            {
                if (effect is T match)
                    yield return match;
            }
        }
    }
}