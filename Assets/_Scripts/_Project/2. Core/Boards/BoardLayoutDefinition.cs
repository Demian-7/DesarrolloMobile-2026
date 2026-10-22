using System;
using System.Collections.Generic;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "IdleGenerator/Board Layout Definition")]
    public class BoardLayoutDefinition : ScriptableObject
    {
        public List<CellUnlockEntry> UnlockSequence;
    }

    [Serializable]
    public class CellUnlockEntry
    {
        public CellCoord Coordinate;
        public CurrencyValue Price; //Ignored for the first entry
    }
}