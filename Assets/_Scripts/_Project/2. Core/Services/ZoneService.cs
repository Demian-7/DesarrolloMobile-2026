using System.Collections.Generic;
using System.Linq;
using Game.Foundation;
using UnityEngine;

namespace Game.Core
{
    public class ZoneService
    {
        private readonly IEventBus _eventBus;
        public Zone ActiveZone { get; private set; }
        private readonly List<Zone> _unlockedZones = new();

        public ZoneService(IEventBus eventBus)
        {
            _eventBus = eventBus;

            ActiveZone = new Zone { ZoneId = "Zone_1" };
            _unlockedZones.Add(ActiveZone);
            
            _eventBus.Subscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);
        }

        public List<Zone> GetZonesToSimulate()
        {
            //No lategame global upgrade logic yet.
            //TODO: decide which zones should generate based on the global upgrades unlocked.
            return new List<Zone> { ActiveZone };
        }

        public Zone GetZone(string zoneId)
        {
            foreach (var zone in _unlockedZones)
            {
                if (zone.ZoneId == zoneId)
                    return zone;
            }
            
            Debug.LogWarning("Zone not found: " + zoneId);
            return null;
        }

        private void OnGenerationRateChanged(GenerationRateChangedEvent evt)
        {
            //In multi-zone setup, evt should carry the ZoneId.
            //For now, just applied to ActiveZone.
            ActiveZone.GenerationRate = evt.NewRate;
        }
    }
}