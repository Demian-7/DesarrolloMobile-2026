using System;
using System.Collections.Generic;
using Game.Core;
using Game.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class MockShopController : MonoBehaviour
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
        private string _activeZoneId;
        private GameStats _stats;

        private CardDefinition _basicDef;
        private CardDefinition _multiplierDef;
        private CardDefinition _bufferDef;
        
        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId, GameStats stats, CardDefinition basic, CardDefinition multiplier, CardDefinition buffer)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;
            _stats = stats;
            
            _basicDef = basic;
            _multiplierDef = multiplier;
            _bufferDef = buffer;

            _basicPriceText.text = $"${_stats.BasicCardCost}";
            _multiplierPriceText.text = $"${_stats.MultiplierCardCost}";
            _bufferPriceText.text = $"${_stats.BufferCardCost}";

            _buyBasicButton.onClick.AddListener(() => TryBuyCard(_basicDef, _stats.BasicCardCost));
            _buyMultiplierButton.onClick.AddListener(() => TryBuyCard(_multiplierDef, _stats.MultiplierCardCost));
            _buyBufferButton.onClick.AddListener(() => TryBuyCard(_bufferDef, _stats.BufferCardCost));

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            
            EvaluateButtonStates(new CurrencyValue(_stats.InitialMoney)); 
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            }
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                EvaluateButtonStates(evt.NewBalance);
            }
        }

        private void EvaluateButtonStates(CurrencyValue currentBalance)
        {
            _buyBasicButton.interactable = currentBalance >= new CurrencyValue(_stats.BasicCardCost);
            _buyMultiplierButton.interactable = currentBalance >= new CurrencyValue(_stats.MultiplierCardCost);
            _buyBufferButton.interactable = currentBalance >= new CurrencyValue(_stats.BufferCardCost);
        }

        private void TryBuyCard(CardDefinition def, int cost)
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            CurrencyValue price = new CurrencyValue(cost);

            if (zone.Wallet.TrySpend(price))
            {
                zone.CardInventory.Add(new CardInstance(def));
                
                _eventBus.Publish(new PackOpenedEvent(_activeZoneId, new List<CardDefinition> { def }));
            }
        }
    }
}