using System;
using System.Collections.Generic;
using System.Linq;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    public class ZoneService
    {
        private readonly IEventBus _eventBus;
        private readonly GlobalUpgradeService _globalUpgrades;
        private readonly Dictionary<string, Zone> _unlockedZones = new();
        private string _activeZoneId;

        public ZoneService(IEventBus eventBus, GlobalUpgradeService globalUpgrades)
        {
            _eventBus = eventBus;
            _globalUpgrades = globalUpgrades;
        }

        public Zone GetZone(string zoneId) => _unlockedZones[zoneId];

        public List<Zone> GetZonesToSimulate() 
        => _globalUpgrades.HasEffect<ConcurrentZoneSimulationEffect>()
        ? _unlockedZones.Values.ToList()
        : new List<Zone> { _unlockedZones[_activeZoneId] };
        
        public void CreateAndActivateZone(ZoneDefinition definition)
        {
            var zone = new Zone { ZoneId = definition.Id };

            if (definition.Mode == BoardGenerationMode.Fixed)
            {
                foreach (var entry in definition.Layout.UnlockSequence)
                    zone.Board.AddCell(entry.Coordinate.X, entry.Coordinate.Y);
                zone.Board.UnlockCell(definition.Layout.UnlockSequence[0].Coordinate);
            }

            if (definition.StartingCards != null && definition.StartingCards.Count > 0)
            {
                foreach (var cardDef in definition.StartingCards)
                    zone.CardInventory.Add(new CardInstance(cardDef));
            }

            _unlockedZones[zone.ZoneId] = zone;
            _activeZoneId = zone.ZoneId;
            _eventBus.Publish(new ZoneUnlockedEvent(zone.ZoneId));
        }

        private void OnGenerationRateChanged(GenerationRateChangedEvent evt)
        {
            //In multi-zone setup, evt should carry the ZoneId.
            //For now, just applied to ActiveZone.
            // ActiveZone.GenerationRate = evt.NewRate;
        }

        public bool TryPlaceCardFromInventory(Zone zone, Guid cardInstanceId, CellCoord coord)
        {
            var card = zone.CardInventory.Find(c => c.InstanceId == cardInstanceId);
            if (card == null || !zone.Board.Cells.TryGetValue(coord, out Cell cell) || !cell.IsUnlocked)
                return false;

            if (cell.OccupiedCard != null)
            {
                CardInstance oldCard = cell.OccupiedCard;
        
                cell.OccupiedCard = null;
                oldCard.CurrentCell = null;
        
                zone.CardInventory.Add(oldCard);
        
                _eventBus.Publish(new CardRemovedFromBoardEvent(zone.ZoneId, oldCard.InstanceId, coord));
            }

            zone.CardInventory.Remove(card);
    
            cell.OccupiedCard = card;
            card.CurrentCell = cell;

            _eventBus.Publish(new CardPlacedEvent(zone.ZoneId, cardInstanceId, coord));
            _eventBus.Publish(new BoardChangedEvent(zone.ZoneId));
    
            return true;
        }
        
        public bool TryMoveCardOnBoard(Zone zone, CellCoord fromCoord, CellCoord toCoord)
        {
            if (!zone.Board.Cells.TryGetValue(fromCoord, out Cell fromCell) || fromCell.OccupiedCard == null) 
                return false;
    
            if (!zone.Board.Cells.TryGetValue(toCoord, out Cell toCell) || !toCell.IsUnlocked) 
                return false;

            CardInstance movingCard = fromCell.OccupiedCard;
            CardInstance targetCard = toCell.OccupiedCard;

            _eventBus.Publish(new CardRemovedFromBoardEvent(zone.ZoneId, movingCard.InstanceId, fromCoord));
            if (targetCard != null)
                _eventBus.Publish(new CardRemovedFromBoardEvent(zone.ZoneId, targetCard.InstanceId, toCoord));

            fromCell.OccupiedCard = targetCard;
            if (targetCard != null) targetCard.CurrentCell = fromCell;

            toCell.OccupiedCard = movingCard;
            movingCard.CurrentCell = toCell;

            _eventBus.Publish(new CardPlacedEvent(zone.ZoneId, movingCard.InstanceId, toCoord));
            if (targetCard != null)
                _eventBus.Publish(new CardPlacedEvent(zone.ZoneId, targetCard.InstanceId, fromCoord));

            _eventBus.Publish(new BoardChangedEvent(zone.ZoneId));
            return true;
        }
        
        public bool TryPickUpCard(Zone zone, CellCoord coord)
        {
            if (!zone.Board.Cells.TryGetValue(coord, out Cell cell) || cell.OccupiedCard == null)
                return false;

            CardInstance card = cell.OccupiedCard;

            cell.OccupiedCard = null;
            card.CurrentCell = null;

            zone.CardInventory.Add(card);

            _eventBus.Publish(new CardRemovedFromBoardEvent(zone.ZoneId, card.InstanceId, coord));
            _eventBus.Publish(new BoardChangedEvent(zone.ZoneId));

            return true;
        }
    }
}