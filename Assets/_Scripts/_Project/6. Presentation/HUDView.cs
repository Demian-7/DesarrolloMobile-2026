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

        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private string _activeZoneId;

        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
                _eventBus.Unsubscribe<GenerationRateChangedEvent>(OnGenerationRateChanged);
            }
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                _currencyText.text = $"${evt.NewBalance}";
            }
        }

        private void OnGenerationRateChanged(GenerationRateChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                _rateText.text = $"+{evt.NewRate}/s";
            }
        }

        private void RefreshUI()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            _currencyText.text = $"${zone.Wallet.Balance}";
            _rateText.text = $"+{zone.GenerationRate}/s";
        }
    }
}