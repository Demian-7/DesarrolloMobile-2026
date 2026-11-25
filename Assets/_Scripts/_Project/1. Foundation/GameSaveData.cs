using System;
using System.Collections.Generic;

namespace Game.Foundation
{
    [Serializable]
    public class GameSaveData
    {
        public int Version = 1;
        public string ActiveZoneId;
        public long LastActiveUtcTicks;
        public long BoostExpiresAtUtcTicks; // 0 = no active ticks
        public List<ZoneSaveData> Zones = new();
    }

    [Serializable]
    public class ZoneSaveData
    {
        public string ZoneId;
        public double BalanceMantissa;
        public int BalanceExponent;
        public List<CellCoordSaveData> UnlockedCells = new();
        public List<PlacedCardSaveData> PlacedCards = new();
        public List<InventoryCardSaveData> InventoryCards = new();
    }

    [Serializable]
    public struct CellCoordSaveData
    {
        public int X;
        public int Y;
        
        public CellCoordSaveData(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public struct PlacedCardSaveData
    {
        public string InstanceId;
        public string CardDefinitionId;
        public int X;
        public int Y;
    }

    [Serializable]
    public struct InventoryCardSaveData
    {
        public string InstanceId;
        public string CardDefinitionId;
    }
}