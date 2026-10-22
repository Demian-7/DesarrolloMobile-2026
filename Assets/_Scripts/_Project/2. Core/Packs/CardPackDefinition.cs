using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "IdleGenerator/Card Pack Definition")]
    public class CardPackDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public CurrencyValue Price;

        [Header("Yield Range")]
        [Tooltip("The minimum number of cards this pack will drop.")]
        public int MinCards = 3;
        [Tooltip("The maximum number of cards this pack will drop.")]
        public int MaxCards = 5;

        [Header("Pool Source")]
        [Tooltip("If assigned, the pack draws exclusively from this weighted set. If left None, it searches Resources/Cards/ using the filters below.")]
        public CardSetDefinition CardSet;

        [Header("Dynamic Filter")]
        public InteractionFilterType FilterType = InteractionFilterType.None;
        public CardDefinition FilterByType;
        public CardTier FilterByTier;
        public CardFamilyDefinition FilterByFamily;

        [Header("Guarantees")]
        public CardTier MinimumGuaranteedTier;
        public int MinimumGuaranteedCount = 1;

        private void OnValidate()
        {
            if (MaxCards < MinCards) 
                MaxCards = MinCards;
                
            if (MinCards < MinimumGuaranteedCount)
            {
                Debug.LogWarning($"[CardPackDefinition] '{name}': MinCards ({MinCards}) cannot be less than the MinimumGuaranteedCount ({MinimumGuaranteedCount}). Adjusting MinCards automatically.");
                MinCards = MinimumGuaranteedCount;
                
                if (MaxCards < MinCards) 
                    MaxCards = MinCards;
            }
        }
    }
}