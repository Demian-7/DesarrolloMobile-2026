using System.Collections.Generic;
using Game.Core;
using Game.Foundation;
using Game.Input.Mobile;
using Game.Presentation;
using UnityEngine;

namespace Game.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Views & Controllers")] 
        [SerializeField] private BoardInputController _boardInputController;
        [SerializeField] private ShopController _shopController;
        [SerializeField] private ZoneSelectorController _zoneSelectorController;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardCameraController _cameraController;
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private HUDView _hudView;
        
        [Header("Game Config & Zones")]
        [SerializeField] private GameStats _gameStats;
        [SerializeField] private List<ZoneDefinition> _zones;

        private ZoneService _zoneService;
        private CurrencyAccrualService _currencyAccrualService;

        private void Awake()
        {
            if (_zones == null || _zones.Count == 0 || _zones[0] == null)
            {
                Debug.LogError("[GameBootstrapper] Please assign at least Zone 1 in the _zones list in the Inspector.");
                return;
            }

            var eventBus = new EventBus();
            var mobileInput = gameObject.AddComponent<MobileTouchInputProvider>();
            IInputProvider inputProvider = mobileInput;

            var globalUpgradeService = new GlobalUpgradeService(eventBus);
            _zoneService = new ZoneService(eventBus, globalUpgradeService);
            var boardEvaluationService = new BoardEvaluationService(eventBus, _zoneService, globalUpgradeService);
            _currencyAccrualService = new CurrencyAccrualService(_zoneService, eventBus);
            var packOpeningService = new PackOpeningService();
            var shopService = new ShopService(eventBus, packOpeningService, _zoneService);

            ZoneDefinition startingZoneDef = _zones[0];
            _zoneService.CreateAndActivateZone(startingZoneDef);
            Zone activeZone = _zoneService.GetZone(startingZoneDef.Id);

            if (_gameStats != null && _gameStats.InitialMoney > 0)
            {
                activeZone.Wallet.Add(new CurrencyValue(_gameStats.InitialMoney));
                eventBus.Publish(new CurrencyChangedEvent(activeZone.ZoneId, activeZone.Wallet.Balance));
            }

            _cameraController.Initialize(inputProvider);
            _boardInputController.Initialize(eventBus, inputProvider, _zoneService, shopService, activeZone.ZoneId);
            _boardView.Initialize(eventBus, _zoneService, shopService, activeZone.ZoneId);
            _inventoryView.Initialize(eventBus, _zoneService, activeZone.ZoneId, inputProvider);
            _hudView.Initialize(eventBus, _zoneService, activeZone.ZoneId);
            _shopController.Initialize(eventBus, _zoneService, shopService, activeZone.ZoneId);

            if (_zoneSelectorController != null)
            {
                _zoneSelectorController.Initialize(eventBus, _zoneService, shopService, _zones, activeZone.ZoneId);
            }

            eventBus.Subscribe<GenerationRateChangedEvent>(evt =>
            {
                var zone = _zoneService.GetZone(evt.ZoneId);
                if (zone != null) zone.GenerationRate = evt.NewRate;
            });
        }

        private void Update()
        {
            _currencyAccrualService?.Tick(Time.deltaTime);
        }
    }
}