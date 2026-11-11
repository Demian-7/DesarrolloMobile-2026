using System;
using Game.Foundation;
using UnityEngine;

namespace Game.Presentation
{
    public class ZoneLook : MonoBehaviour
    {
        [Serializable]
        private class ZoneEntry
        {
            public string zoneId;
            public VolumeFader fader;
        }

        [SerializeField] private ZoneEntry[] _zones;
        [SerializeField] private VolumeFader _boon;
        [SerializeField] private ZoneBackground _background;
        [SerializeField] private float _zoneFadeDuration = 1.2f;
        [SerializeField] private float _boonFadeDuration = 0.6f;

        private IEventBus _eventBus;

        public void Init(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
            _eventBus.Subscribe<ChainStepEvent>(OnChainStep);
        }

        private void OnDestroy()
        {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
            _eventBus.Unsubscribe<ChainStepEvent>(OnChainStep);
        }

        private void OnActiveZoneChanged(ActiveZoneChangedEvent evt)
        {
            ShowZone(evt.ActiveZoneId, _zoneFadeDuration);
        }

        private void OnChainStep(ChainStepEvent evt)
        {
            if (_background != null) _background.Kick();
        }

        public void ShowZone(string zoneId)
        {
            ShowZone(zoneId, _zoneFadeDuration);
        }

        public void ShowZone(string zoneId, float duration)
        {
            foreach (var entry in _zones)
            {
                if (entry.fader == null) continue;
                entry.fader.FadeTo(entry.zoneId == zoneId ? 1f : 0f, duration);
            }

            if (_background != null) _background.ShowZone(zoneId, duration);
        }

        public void SetBoon(bool active)
        {
            if (_boon != null)
                _boon.FadeTo(active ? 1f : 0f, _boonFadeDuration);
        }
    }
}