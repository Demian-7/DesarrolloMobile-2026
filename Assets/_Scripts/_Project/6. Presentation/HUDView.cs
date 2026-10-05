using System;
using UnityEngine;
using TMPro;
using Game.Foundation;
using Game.Core;

namespace Game.Presentation
{
    public class HUDView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _rateText;
        [SerializeField] private TextMeshProUGUI _infoText;

        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private string _activeZoneId;
        
        private Guid _selectedCardId = Guid.Empty;

        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);
            _eventBus.Subscribe<CardSelectedEvent>(OnCardSelected);

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
                _eventBus.Unsubscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);
                _eventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
            }
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
                _currencyText.text = $"${evt.NewBalance}";
        }

        private void OnGenerationRateChanged(GenerationRateChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                _rateText.text = $"+{evt.NewRate}/s";
                if (_selectedCardId != Guid.Empty) UpdateInfoText();
            }
        }

        private void OnCardSelected(CardSelectedEvent evt)
        {
            _selectedCardId = evt.CardInstanceId;
            UpdateInfoText();
        }

        private void UpdateInfoText()
        {
            if (_infoText == null) return;

            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            CardInstance selectedCard = zone.CardInventory.Find(c => c.InstanceId == _selectedCardId);
            bool isInInventory = selectedCard != null;

            if (!isInInventory)
            {
                foreach (var cell in zone.Board.Cells.Values)
                {
                    if (cell.OccupiedCard != null && cell.OccupiedCard.InstanceId == _selectedCardId)
                    {
                        selectedCard = cell.OccupiedCard;
                        break;
                    }
                }
            }

            if (selectedCard != null)
            {
                string baseDesc = "";
                foreach (var effect in selectedCard.Definition.Effects)
                {
                    if (effect is FlatGenerationEffect flat)
                        baseDesc += $"{flat.BaseAmount} ";
                    else if (effect is PercentageGenerationEffect perc)
                        baseDesc += $"+{perc.Percentage * 100}% ";
                    else if (effect is ModifyingEffect mod)
                        baseDesc += $"{(mod.SignedMagnitude > 0 ? "+" : "")}{mod.SignedMagnitude} ";
                }
                
                if (string.IsNullOrWhiteSpace(baseDesc)) baseDesc = "0";

                CurrencyValue displayEffective = isInInventory ? CurrencyValue.Zero : selectedCard.EffectiveValue;

                _infoText.text = $"Selected: {selectedCard.Definition.DisplayName}\nBase: {baseDesc.Trim()} | Effective: {displayEffective}";
            }
            else
            {
                _infoText.text = "Select a card...";
            }
        }

        private void RefreshUI()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            _currencyText.text = $"${zone.Wallet.Balance}";
            _rateText.text = $"+{zone.GenerationRate}/s";
            UpdateInfoText();
        }
    }
}