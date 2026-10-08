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

        private CardPackDefinition _basicPack;
        private CardPackDefinition _multiplierPack;
        private CardPackDefinition _bufferPack;
        
        public void Initialize(IEventBus eventBus, ZoneService zoneService, ShopService shopService, string activeZoneId, CardPackDefinition basic, CardPackDefinition multiplier, CardPackDefinition buffer)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _shopService = shopService;
            _activeZoneId = activeZoneId;
            
            _basicPack = basic;
            _multiplierPack = multiplier;
            _bufferPack = buffer;

            _basicPriceText.text = $"${_basicPack.Price.ToString()}";
            _multiplierPriceText.text = $"${_multiplierPack.Price.ToString()}";
            _bufferPriceText.text = $"${_bufferPack.Price.ToString()}";

            _buyBasicButton.onClick.AddListener(() => TryBuyPack(_basicPack));
            _buyMultiplierButton.onClick.AddListener(() => TryBuyPack(_multiplierPack));
            _buyBufferButton.onClick.AddListener(() => TryBuyPack(_bufferPack));

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            
            Zone zone = _zoneService.GetZone(_activeZoneId);
            EvaluateButtonStates(zone.Wallet.Balance); 
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
            _buyBasicButton.interactable = currentBalance >= _basicPack.Price;
            _buyMultiplierButton.interactable = currentBalance >= _multiplierPack.Price;
            _buyBufferButton.interactable = currentBalance >= _bufferPack.Price;
        }

        private void TryBuyPack(CardPackDefinition packDef)
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            _shopService.TryOpenPack(zone, packDef);
        }
    }
}