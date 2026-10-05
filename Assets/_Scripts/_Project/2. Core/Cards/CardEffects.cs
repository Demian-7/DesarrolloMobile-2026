using System;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    [Serializable]

    public abstract class CardEffect
    {
    }

    [Serializable]
    public abstract class GeneratingEffect : CardEffect
    {
        public abstract CurrencyValue Evaluate(BoardEvaluationContext ctx, CellCoord ownerCell);
    }

    [Serializable]
    public abstract class ModifyingEffect : CardEffect
    {
        public abstract CardInteraction Interaction { get; }
        public abstract float SignedMagnitude { get; }
    }

    [Serializable]
    public class BuffEffect : ModifyingEffect
    {
        [Tooltip("Defines which cards receive this buff.")]
        public CardInteraction TargetInteraction;

        [Tooltip(
            "The magnitude of the buff applied to the target's effective value (always positive). E.g. 3.0 = +300%")]
        public float Magnitude;

        public override CardInteraction Interaction => TargetInteraction;
        public override float SignedMagnitude => Mathf.Abs(Magnitude);
    }

    [Serializable]
    public class DebuffEffect : ModifyingEffect
    {
        [Tooltip("Defines which cards are affected by this debuff.")]
        public CardInteraction TargetInteraction;

        [Tooltip(
            "The magnitude of the debuff subtracted from the target's effective value (automatically negative). E.g. 1.0 = -100% (disables)")]
        public float Magnitude;

        public override CardInteraction Interaction => TargetInteraction;
        public override float SignedMagnitude => -Mathf.Abs(Magnitude);
    }
}