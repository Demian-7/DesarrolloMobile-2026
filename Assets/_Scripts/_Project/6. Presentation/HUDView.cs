using System;
using DG.Tweening;
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

        [Header("Display")]
        [Tooltip("Text shown before the money amount. Leave empty when using the coin icon.")]
        [SerializeField] private string _currencyPrefix = "$";
        [Tooltip("If no chain step arrives within this time after a rate change, show the rate directly.")]
        [SerializeField] private float _chainWaitSeconds = 0.4f;

        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private string _activeZoneId;

        private Guid _selectedCardId = Guid.Empty;

        private AnimatedLabel _currencyLabel;
        private AnimatedLabel _rateLabel;

        private CurrencyValue _lastBalance;
        private bool _hasLastBalance;

        private CurrencyValue _targetRate;
        private CurrencyValue _settledRate;
        private bool _rateChanged;
        private bool _rateIncreased;
        private Tween _rateFallback;

        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;

            _currencyLabel = _currencyText != null ? _currencyText.GetComponent<AnimatedLabel>() : null;
            _rateLabel = _rateText != null ? _rateText.GetComponent<AnimatedLabel>() : null;

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);
            _eventBus.Subscribe<ChainStepEvent>(OnChainStep);
            _eventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
            _eventBus.Subscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);

            RefreshUI();
        }

        private void OnDestroy()
        {
            _rateFallback?.Kill();

            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
                _eventBus.Unsubscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);
                _eventBus.Unsubscribe<ChainStepEvent>(OnChainStep);
                _eventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
                _eventBus.Unsubscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
            }
        }

        public void SetBoost(bool active)
        {
            _currencyLabel?.SetBoost(active);
            _rateLabel?.SetBoost(active);
        }

        private void OnActiveZoneChanged(ActiveZoneChangedEvent evt)
        {
            _activeZoneId = evt.ActiveZoneId;
            _selectedCardId = Guid.Empty;
            RefreshUI();
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.ZoneId != _activeZoneId)
                return;

            bool spent = _hasLastBalance && IsGreater(_lastBalance, evt.NewBalance);

            _lastBalance = evt.NewBalance;
            _hasLastBalance = true;

            Show(_currencyText, _currencyLabel, FormatMoney(evt.NewBalance),
                spent ? LabelFlash.Spend : LabelFlash.None);
        }

        private void OnGenerationRateChanged(GenerationRateChangedEvent evt)
        {
            if (evt.ZoneId != _activeZoneId)
                return;

            _targetRate = evt.NewRate;
            _rateChanged = !_targetRate.Equals(_settledRate);
            _rateIncreased = _rateChanged && IsGreater(_targetRate, _settledRate);

            if (_selectedCardId != Guid.Empty) UpdateInfoText();

            if (!_rateChanged)
                return;

            _rateFallback?.Kill();
            _rateFallback = DOVirtual.DelayedCall(_chainWaitSeconds, () =>
            {
                Show(_rateText, _rateLabel, FormatRate(_targetRate.ToString()));
                _settledRate = _targetRate;
                _rateChanged = false;
            }).SetLink(gameObject);
        }

        private void OnChainStep(ChainStepEvent evt)
        {
            if (evt.ZoneId != _activeZoneId)
                return;

            if (!_rateChanged)
                return;

            _rateFallback?.Kill();

            string number = _targetRate.ToString();
            int count = number.Length;

            if (!evt.IsFinal)
            {
                float progress = (evt.Step + 1f) / Mathf.Max(1, evt.StepCount);
                count = Mathf.Clamp(Mathf.CeilToInt(number.Length * progress), 1, number.Length);

                if (count < number.Length && number[count - 1] == '.')
                    count++;
            }

            LabelFlash flash = LabelFlash.None;
            if (_rateIncreased)
                flash = evt.IsFinal ? LabelFlash.GainFinal : LabelFlash.Gain;

            Show(_rateText, _rateLabel, FormatRate(number.Substring(0, count)), flash);

            if (evt.IsFinal)
            {
                _settledRate = _targetRate;
                _rateChanged = false;
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
                _infoText.enabled = true;
                _currencyText.enabled = false;
                _rateText.enabled = false;

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
                _infoText.enabled = false;
                _currencyText.enabled = true;
                _rateText.enabled = true;
            }
        }

        private void RefreshUI()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            _rateFallback?.Kill();

            _lastBalance = zone.Wallet.Balance;
            _hasLastBalance = true;
            _targetRate = zone.GenerationRate;
            _settledRate = zone.GenerationRate;
            _rateChanged = false;
            _rateIncreased = false;

            Show(_currencyText, _currencyLabel, FormatMoney(zone.Wallet.Balance));
            Show(_rateText, _rateLabel, FormatRate(zone.GenerationRate.ToString()));
            UpdateInfoText();
        }

        private string FormatMoney(CurrencyValue value) => $"{_currencyPrefix}{value}";
        private static string FormatRate(string number) => $"+{number}/s";

        private static bool IsGreater(CurrencyValue a, CurrencyValue b)
            => !a.Equals(b) && CurrencyValue.Max(a, b).Equals(a);

        private static void Show(TextMeshProUGUI text, AnimatedLabel label, string value,
            LabelFlash flash = LabelFlash.None)
        {
            if (label != null) label.SetValue(value, flash);
            else if (text != null) text.text = value;
        }
    }
}