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

        /// <summary> Constructor for restored cards that need to keep their original InstanceId </summary>
        public CardInstance(CardDefinition definition, Guid instanceId)
        {
            Definition = definition;
            InstanceId = instanceId;
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