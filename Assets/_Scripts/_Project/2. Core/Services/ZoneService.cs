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
            // InfiniteFrontier (Endless Board) setup is a separate path, not modeled here yet

            foreach (var cardDef in definition.StartingCards)
                zone.CardInventory.Add(new CardInstance(cardDef));

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
            if (card == null || !zone.Board.TryPlaceCard(card, coord))
                return false;

            zone.CardInventory.Remove(card);
            _eventBus.Publish(new CardPlacedEvent(zone.ZoneId, cardInstanceId, coord));
            _eventBus.Publish(new BoardChangedEvent(zone.ZoneId));
            
            return true;
        }
    }
}