using System.Collections.Generic;
using Game.Core;
using Game.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class ZoneSelectorController : MonoBehaviour
    {
        [Header("Zone Buttons (Ordered: Zone 1, Zone 2, Zone 3...)")]
        [SerializeField] private List<Button> _zoneButtons;
        [SerializeField] private List<TextMeshProUGUI> _zoneButtonTexts;

        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private ShopService _shopService;
        private List<ZoneDefinition> _zoneDefinitions;
        private string _activeZoneId;

        public void Initialize(IEventBus eventBus, ZoneService zoneService, ShopService shopService,
            List<ZoneDefinition> zoneDefinitions, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _shopService = shopService;
            _zoneDefinitions = zoneDefinitions;
            _activeZoneId = activeZoneId;

            for (int i = 0; i < _zoneButtons.Count; i++)
            {
                int zoneIndex = i;
                _zoneButtons[i].onClick.AddListener(() => OnZoneButtonClicked(zoneIndex));
            }

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<ZoneUnlockedEvent>(OnZoneUnlocked);
            _eventBus.Subscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);

            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Unsubscribe<ZoneUnlockedEvent>(OnZoneUnlocked);
            _eventBus.Unsubscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                RefreshButtons();
            }
        }

        private void OnZoneUnlocked(ZoneUnlockedEvent evt) => RefreshButtons();

        private void OnActiveZoneChanged(ActiveZoneChangedEvent evt)
        {
            _activeZoneId = evt.ActiveZoneId;
            RefreshButtons();
        }

        private void OnZoneButtonClicked(int index)
        {
            if (_zoneDefinitions == null || index < 0 || index >= _zoneDefinitions.Count)
                return;

            ZoneDefinition targetDef = _zoneDefinitions[index];

            if (_zoneService.IsZoneUnlocked(targetDef.Id))
            {
                _zoneService.TrySetActiveZone(targetDef.Id);
                return;
            }

            if (index > 0)
            {
                ZoneDefinition previousZoneDef = _zoneDefinitions[index - 1];
                if (_activeZoneId == previousZoneDef.Id)
                {
                    Zone currentZone = _zoneService.GetZone(_activeZoneId);
                    _shopService.TryUnlockZone(currentZone, targetDef);
                }
            }
        }

        private void RefreshButtons()
        {
            if (_zoneDefinitions == null) return;

            for (int i = 0; i < _zoneButtons.Count; i++)
            {
                bool hasZoneDef = i < _zoneDefinitions.Count && _zoneDefinitions[i] != null;
                _zoneButtons[i].gameObject.SetActive(hasZoneDef);
                if (!hasZoneDef) continue;

                ZoneDefinition def = _zoneDefinitions[i];
                bool isUnlocked = _zoneService.IsZoneUnlocked(def.Id);
                bool isCurrentZone = _activeZoneId == def.Id;
                string displayName = string.IsNullOrEmpty(def.DisplayName) ? $"Zone {i + 1}" : def.DisplayName;

                if (isUnlocked)
                {
                    if (i < _zoneButtonTexts.Count && _zoneButtonTexts[i] != null)
                        _zoneButtonTexts[i].text = isCurrentZone ? $"[{displayName}]" : displayName;

                    _zoneButtons[i].interactable = !isCurrentZone;
                }
                else
                {
                    if (i < _zoneButtonTexts.Count && _zoneButtonTexts[i] != null)
                        _zoneButtonTexts[i].text = $"Buy {displayName}\n(${def.UnlockPrice})";

                    bool isInPreviousZone = i > 0 && _activeZoneId == _zoneDefinitions[i - 1].Id;
                    Zone activeZone = _zoneService.GetZone(_activeZoneId);
                    bool canAfford = activeZone != null && activeZone.Wallet.Balance >= def.UnlockPrice;

                    _zoneButtons[i].interactable = isInPreviousZone && canAfford;
                }
            }
        }
    }
}