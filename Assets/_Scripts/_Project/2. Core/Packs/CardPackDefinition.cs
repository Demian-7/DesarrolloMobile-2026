using System;
using System.Collections.Generic;
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
        public int CardCount;
        public List<WeightedCardEntry> Pool;
        public CardTier MinimumGuaranteedTier;
        public int MinimumGuaranteedCount = 1;
    }

    [Serializable]
    public class WeightedCardEntry
    {
        public CardDefinition Card;
        public float Weight;
    }
}