using System.Collections.Generic;
using Game.Core;
using Game.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class ShopController : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button _buyBasicButton;
        [SerializeField] private Button _buyMultiplierButton;
        [SerializeField] private Button _buyBufferButton;
        
        [Header("UI Price Texts")]
        [SerializeField] private TextMeshProUGUI _basicPriceText;
        [SerializeField] private TextMeshProUGUI _multiplierPriceText;
        [SerializeField] private TextMeshProUGUI _bufferPriceText;

        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private ShopService _shopService;
        private string _activeZoneId;

        private Button[] _buttons;
        private TextMeshProUGUI[] _priceTexts;
        
        public void Initialize(IEventBus eventBus, ZoneService zoneService, ShopService shopService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _shopService = shopService;
            _activeZoneId = activeZoneId;

            _buttons = new[] { _buyBasicButton, _buyMultiplierButton, _buyBufferButton };
            _priceTexts = new[] { _basicPriceText, _multiplierPriceText, _bufferPriceText };

            for (int i = 0; i < _buttons.Length; i++)
            {
                int packIndex = i;
                _buttons[i].onClick.AddListener(() => TryBuyPackAtIndex(packIndex));
            }

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
            
            RefreshShopForActiveZone();
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
                _eventBus.Unsubscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
            }
        }

        private void OnActiveZoneChanged(ActiveZoneChangedEvent evt)
        {
            _activeZoneId = evt.ActiveZoneId;
            RefreshShopForActiveZone();
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                EvaluateButtonStates(evt.NewBalance);
            }
        }

        private void RefreshShopForActiveZone()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            List<CardPackDefinition> packs = zone.Definition.AvailablePacks;

            for (int i = 0; i < _buttons.Length; i++)
            {
                bool hasPack = packs != null && i < packs.Count && packs[i] != null;
                _buttons[i].gameObject.SetActive(hasPack);

                if (hasPack)
                {
                    _priceTexts[i].text = $"${packs[i].Price}";
                }
            }

            EvaluateButtonStates(zone.Wallet.Balance);
        }

        private void EvaluateButtonStates(CurrencyValue currentBalance)
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null || zone.Definition.AvailablePacks == null) return;

            List<CardPackDefinition> packs = zone.Definition.AvailablePacks;

            for (int i = 0; i < _buttons.Length; i++)
            {
                if (i < packs.Count && packs[i] != null)
                {
                    _buttons[i].interactable = currentBalance >= packs[i].Price;
                }
            }
        }

        private void TryBuyPackAtIndex(int index)
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null || zone.Definition.AvailablePacks == null) return;

            if (index >= 0 && index < zone.Definition.AvailablePacks.Count)
            {
                CardPackDefinition packDef = zone.Definition.AvailablePacks[index];
                _shopService.TryOpenPack(zone, packDef);
            }
        }
    }
}