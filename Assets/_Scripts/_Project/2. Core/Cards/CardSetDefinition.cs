using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class WeightedCardEntry
    {
        public CardDefinition Card;
        public float Weight = 1f;
    }

    [CreateAssetMenu(menuName = "IdleGenerator/Card Set Definition")]
    public class CardSetDefinition : ScriptableObject
    {
        [Tooltip("A reusable, weighted pool of cards.")]
        public List<WeightedCardEntry> Pool = new();
    }
}