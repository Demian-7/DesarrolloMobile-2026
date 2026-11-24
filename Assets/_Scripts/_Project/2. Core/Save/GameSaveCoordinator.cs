using System;
using System.Collections.Generic;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    public class GameSaveCoordinator
    {
        private readonly ISaveService _saveService;
        private readonly ITimeService _timeService;
        private readonly ZoneService _zoneService;
        private readonly BoostService _boostService;
        private readonly Dictionary<string, ZoneDefinition> _zoneDefs = new();
        private readonly Dictionary<string, CardDefinition> _cardDefs = new();

        public GameSaveCoordinator(
            ISaveService saveService,
            ITimeService timeService,
            ZoneService zoneService,
            BoostService boostService,
            IEnumerable<ZoneDefinition> zoneDefinitions,
            IEnumerable<CardDefinition> cardDefinitions)
        {
            _saveService = saveService;
            _timeService = timeService;
            _zoneService = zoneService;
            _boostService = boostService;

            foreach (var def in zoneDefinitions)
            {
                if (def != null && !string.IsNullOrEmpty(def.Id))
                    _zoneDefs[def.Id] = def;
            }

            foreach (var def in cardDefinitions)
            {
                if (def != null && !string.IsNullOrEmpty(def.Id))
                    _cardDefs[def.Id] = def;
            }
        }

        public bool TryLoadGame()
        {
            if (!_saveService.HasSave())
                return false;

            GameSaveData data = _saveService.Load();
            if (data == null || data.Zones == null || data.Zones.Count == 0)
                return false;

            foreach (var zoneData in data.Zones)
            {
                if (!_zoneDefs.TryGetValue(zoneData.ZoneId, out var zoneDef))
                {
                    Debug.LogWarning($"[Save] Unknown ZoneId '{zoneData.ZoneId}', skipping.");
                    continue;
                }

                Zone zone = _zoneService.RestoreZone(zoneDef);
                zone.Wallet.SetBalance(new CurrencyValue(zoneData.BalanceMantissa, zoneData.BalanceExponent));

                foreach (var coordData in zoneData.UnlockedCells)
                {
                    var coord = new CellCoord(coordData.X, coordData.Y);
                    if (!zone.Board.Cells.ContainsKey(coord))
                        zone.Board.AddCell(coord.X, coord.Y);

                    zone.Board.UnlockCell(coord);
                }

                foreach (var placed in zoneData.PlacedCards)
                {
                    if (!_cardDefs.TryGetValue(placed.CardDefinitionId, out var cardDef))
                    {
                        Debug.LogWarning($"[Save] Unknown CardDefinitionId '{placed.CardDefinitionId}', skipping.");
                        continue;
                    }

                    Guid id = Guid.TryParse(placed.InstanceId, out var parsed) ? parsed : Guid.NewGuid();
                    var card = new CardInstance(cardDef, id);
                    zone.Board.TryPlaceCard(card, new CellCoord(placed.X, placed.Y));
                }

                foreach (var inv in zoneData.InventoryCards)
                {
                    if (!_cardDefs.TryGetValue(inv.CardDefinitionId, out var cardDef))
                    {
                        Debug.LogWarning($"[Save] Unknown CardDefinitionId '{inv.CardDefinitionId}', skipping.");
                        continue;
                    }

                    Guid id = Guid.TryParse(inv.InstanceId, out var parsed) ? parsed : Guid.NewGuid();
                    zone.CardInventory.Add(new CardInstance(cardDef, id));
                }
            }

            string targetActiveZone = !string.IsNullOrEmpty(data.ActiveZoneId) && _zoneService.IsZoneUnlocked(data.ActiveZoneId)
                ? data.ActiveZoneId
                : data.Zones[0].ZoneId;

            _zoneService.TrySetActiveZone(targetActiveZone);

            if (data.BoostExpiresAtUtcTicks > 0)
            {
                var expiresAt = new DateTime(data.BoostExpiresAtUtcTicks, DateTimeKind.Utc);
                _boostService.RestoreExpiryUtc(expiresAt);
            }

            return true;
        }
        
        public void SaveGame()
        {
            var data = new GameSaveData
            {
                ActiveZoneId = _zoneService.ActiveZoneId,
                LastActiveUtcTicks = _timeService.UtcNow.Ticks,
                BoostExpiresAtUtcTicks = _boostService.BoostExpiresAtUtc?.Ticks ?? 0
            };

            foreach (var zone in _zoneService.GetAllUnlockedZones())
            {
                var zoneData = new ZoneSaveData
                {
                    ZoneId = zone.ZoneId,
                    BalanceMantissa = zone.Wallet.Balance.Mantissa,
                    BalanceExponent = zone.Wallet.Balance.Exponent
                };

                foreach (var cell in zone.Board.Cells.Values)
                {
                    if (cell.IsUnlocked)
                        zoneData.UnlockedCells.Add(new CellCoordSaveData(cell.Coordinate.X, cell.Coordinate.Y));

                    if (cell.OccupiedCard != null)
                    {
                        zoneData.PlacedCards.Add(new PlacedCardSaveData
                        {
                            InstanceId = cell.OccupiedCard.InstanceId.ToString(),
                            CardDefinitionId = cell.OccupiedCard.Definition.Id,
                            X = cell.Coordinate.X,
                            Y = cell.Coordinate.Y
                        });
                    }
                }

                foreach (var card in zone.CardInventory)
                {
                    zoneData.InventoryCards.Add(new InventoryCardSaveData
                    {
                        InstanceId = card.InstanceId.ToString(),
                        CardDefinitionId = card.Definition.Id
                    });
                }

                data.Zones.Add(zoneData);
            }

            _saveService.Save(data);
        }
    }
}