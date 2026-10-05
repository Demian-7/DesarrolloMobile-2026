using System.Collections.Generic;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "IdleGenerator/Card Definition")]
    public class CardDefinition : ScriptableObject
    {
        [Tooltip("Internal unique identifier used for save data logic.")]
        public string Id;
        [Tooltip("The localized name displayer to the player in-game.")]
        public string DisplayName;
        [Tooltip("Scales the card's numbers/intensity without changing its fundamental Effect or Interaction.")]
        public CardTier Tier;
        [Tooltip("THe parent group this card belongs to. Links Tier 1, 2 and 3 variants of the same card together")]
        public CardFamilyDefinition Family;
        [Tooltip("The list of Main Behaviours (Flat Gen, % Gen, Buff, Debuff) this card executes.")]
        [SerializeReference, SubclassSelector]
        public List<CardEffect> Effects = new();

        public Color IconColor; //manual for now, the behavior-to-color mapping isn't finalized yet
    }
}