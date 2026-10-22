using UnityEngine;

namespace Game.Foundation
{
    [CreateAssetMenu(fileName = "GameStats", menuName = "IdleGenerator/GameStats")]
    public class GameStats : ScriptableObject
    {
        [Header("Initial Stats")]
        [Tooltip("The amount of money that the player starts with")]
        public int InitialMoney = 0;

        [Tooltip("The amount of money that each cell costs progressively")]
        public int[] CellCost;
        
        [Header("Mock Shop Prices")]
        public int BasicCardCost = 10;
        public int MultiplierCardCost = 50;
        public int BufferCardCost = 100;
    }
}