using System.Collections.Generic;
using DG.Tweening;
using Game.Core;
using Game.Foundation;
using Game.Input.Mobile;
using Game.Platform.Android;
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
        [SerializeField] private ZoneLook _zoneLook;
        
        [Header("Game Config & Zones")]
        [SerializeField] private GameStats _gameStats;
        [SerializeField] private List<ZoneDefinition> _zones;
        
        [Header("Ad Boost")]
        [SerializeField] private float _boostMultiplier = 2f;
        [SerializeField] private float _boostDurationSeconds = 120f;
        [SerializeField] private float _adCooldownSeconds = 60f;
        
        [Header("Save")]
        [SerializeField] private float _autoSaveIntervalSeconds = 60f;

        private ZoneService _zoneService;
        private CurrencyAccrualService _currencyAccrualService;
        private BoostService _boostService;
        private GameSaveCoordinator _saveCoordinator;
        private float _autoSaveTimer;

        private void Awake()
        {
            if (_zones == null || _zones.Count == 0 || _zones[0] == null)
            {
                Debug.LogError("[GameBootstrapper] Please assign at least Zone 1 in the _zones list in the Inspector.");
                return;
            }

            DOTween.SetTweensCapacity(800, 200);
            
            var eventBus = new EventBus();
            var mobileInput = gameObject.AddComponent<MobileTouchInputProvider>();
            IInputProvider inputProvider = mobileInput;
            ITimeService timeService = new SystemTimeService();
            IAdsService adsService = new MockAdsService();
            ISaveService saveService = new LocalJsonSaveService();
            CardDefinition[] allCards = Resources.LoadAll<CardDefinition>("Cards");
            
            _boostService = new BoostService(
                adsService,
                timeService,
                eventBus,
                _boostMultiplier,
                _boostDurationSeconds,
                _adCooldownSeconds);

            var globalUpgradeService = new GlobalUpgradeService(eventBus);
            _zoneService = new ZoneService(eventBus, globalUpgradeService);
            var boardEvaluationService = new BoardEvaluationService(eventBus, _zoneService, globalUpgradeService);
            _currencyAccrualService = new CurrencyAccrualService(_zoneService, _boostService, eventBus);
            var packOpeningService = new PackOpeningService();
            var shopService = new ShopService(eventBus, packOpeningService, _zoneService);

            _saveCoordinator = new GameSaveCoordinator(
                saveService,
                timeService,
                _zoneService,
                _boostService,
                _zones,
                allCards);

            bool loadedFromSave = _saveCoordinator.TryLoadGame();

            if (!loadedFromSave)
            {
                ZoneDefinition startingZoneDef = _zones[0];
                _zoneService.CreateAndActivateZone(startingZoneDef);
                Zone startingZone = _zoneService.GetZone(startingZoneDef.Id);

                if (_gameStats != null && _gameStats.InitialMoney > 0)
                {
                    startingZone.Wallet.Add(new CurrencyValue(_gameStats.InitialMoney));
                    eventBus.Publish(new CurrencyChangedEvent(startingZone.ZoneId, startingZone.Wallet.Balance));
                }
            }

            Zone activeZone = _zoneService.GetZone(_zoneService.ActiveZoneId);

            if (_zoneLook != null)
            {
                _zoneLook.Init(eventBus);
                _zoneLook.ShowZone(activeZone.ZoneId, 0f);
            }

            _cameraController.Initialize(inputProvider);
            _boardInputController.Initialize(eventBus, inputProvider, _zoneService, shopService, activeZone.ZoneId);
            _boardView.Initialize(eventBus, _zoneService, shopService, activeZone.ZoneId);
            _inventoryView.Initialize(eventBus, _zoneService, activeZone.ZoneId, inputProvider);
            _hudView.Initialize(eventBus, _zoneService, _boostService, activeZone.ZoneId);
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

            if (loadedFromSave)
            {
                foreach (var zone in _zoneService.GetAllUnlockedZones())
                    eventBus.Publish(new BoardChangedEvent(zone.ZoneId));

                if (_boostService.IsBoostActive)
                {
                    eventBus.Publish(new BoostStateChangedEvent(
                        true,
                        _boostService.CurrentMultiplier,
                        _boostService.RemainingSeconds));
                }
            }

            eventBus.Subscribe<BoardChangedEvent>(_ => _saveCoordinator.SaveGame());
            eventBus.Subscribe<ZoneUnlockedEvent>(_ => _saveCoordinator.SaveGame());
            eventBus.Subscribe<BoostStateChangedEvent>(_ => _saveCoordinator.SaveGame());
        }

        private void Update()
        {
            _boostService?.Tick();
            _currencyAccrualService?.Tick(Time.deltaTime);
            
            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= _autoSaveIntervalSeconds)
            {
                _autoSaveTimer = 0f;
                _saveCoordinator?.SaveGame();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                _saveCoordinator?.SaveGame();
        }

        private void OnApplicationQuit()
        {
            _saveCoordinator?.SaveGame();
        }

        [ContextMenu("Delete Save")]
        private void DeleteSave()
        {
            new LocalJsonSaveService().DeleteSave();
            Debug.Log("[GameBootstrapper] Save file deleted.");
        }
    }
}