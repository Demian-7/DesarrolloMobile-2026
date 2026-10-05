using System;
using Game.Core;
using UnityEngine;

namespace Game.Foundation
{
    [Serializable]
    public class CardInteraction
    {
        [Tooltip("Defined the spatial relationship required to interact")]
        public InteractionType Type;
        [Tooltip("Used ONLY if Type is 'Directional'. Select one or multiple specific directions to target.")]
        public Directions Directions; //Only used if Type == Directional
        [Tooltip("Extended targeting range. Currently deferred; keep at 1 for default adjacency.")]
        public int Range = 1;
        
        [Header("Filtering")]
        [Tooltip("Narrows down the targeted cards by an exact condition. 'None' affects all cards in the interaction area.")]
        public InteractionFilterType FilterType = InteractionFilterType.None;
        [Tooltip("Used if Filter Type is 'ExactType'. The targeted card must be this exact asset.")]
        public CardDefinition FilterByType;
        [Tooltip("Used if Filter Type is 'Tier'. The targeted card must match this strength level.")]
        public CardTier FilterByTier;
        [Tooltip("Used if Filter Type is 'Family'. The targeted card must belong to this behavior group regardless of tier.")]
        public CardFamilyDefinition FilterByFamily;

        public bool Matches(CardDefinition candidate)
        {
            if (candidate == null)
                return false;

            return FilterType switch
            {
                InteractionFilterType.None => true,
                InteractionFilterType.ExactType => candidate == FilterByType,
                InteractionFilterType.Tier => candidate.Tier == FilterByTier,
                InteractionFilterType.Family => candidate.Family == FilterByFamily,
                _ => true
            };
        }
    }
}