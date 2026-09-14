using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "IdleGenerator/Card Definition")]
    public class CardDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public CardTier Tier;
        
        [SerializeReference]
        public List<ICardEffect> Effects = new();

        public Color IconColor; //manual for now, the behavior-to-color mapping isn't finalized yet
    }
}