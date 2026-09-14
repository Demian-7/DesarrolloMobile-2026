using System;
using Game.Core;
using Game.Foundation;
using UnityEngine;

public interface ICardEffect { }  // marker

public interface IGeneratingEffect : ICardEffect
{
    CurrencyValue Evaluate(BoardEvaluationContext ctx, CellCoord ownerCell);
}

public interface IModifyingEffect : ICardEffect
{
    CardInteraction Interaction { get; }
    float SignedMagnitude { get; } // positive for Buff, negative for Debuff. e.g. +3.0 = "+300%", -1.0 = "-100%" (disables)
}

[Serializable]
public class BuffEffect : IModifyingEffect
{
    [field: SerializeField] public CardInteraction Interaction { get; set; } // [field:SerializeField] so the auto-property still serializes like a normal field
    public float Magnitude; // e.g. 3.0 = "+300%"
    public float SignedMagnitude => Magnitude;
}

[Serializable]
public class DebuffEffect : IModifyingEffect
{
    [field: SerializeField] public CardInteraction Interaction { get; set; }
    public float Magnitude; // e.g. 1.0 = "-100%", i.e. disables
    public float SignedMagnitude => -Magnitude;
}