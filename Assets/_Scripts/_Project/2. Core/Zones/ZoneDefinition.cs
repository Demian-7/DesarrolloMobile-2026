using System;
using System.Collections.Generic;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    public enum BoardGenerationMode { Fixed, InfiniteFrontier }
    
    [CreateAssetMenu(menuName = "IdleGenerator/Zone Definition")]
    public class ZoneDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public BoardGenerationMode Mode;

        [Tooltip("Used when Mode == Fixed")]
        public BoardLayoutDefinition Layout;
        [Tooltip("Used when Mode == InfiniteFrontier")]
        public CurrencyValue EndlessBasePrice;
        [Tooltip("Used when Mode == InfiniteFrontier")]
        public float EndlessPriceDoublingFactor;

        public CurrencyValue UnlockPrice;
        public List<CardDefinition> StartingCards;
        public List<CardPackDefinition> AvailablePacks;
    }
}