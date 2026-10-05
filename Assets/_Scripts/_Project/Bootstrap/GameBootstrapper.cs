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
        [SerializeField] private MockShopController _mockShopController;
        [SerializeField] private BoardView _boardView;

        [Header("New Phase 2 References")] 
        [SerializeField] private BoardCameraController _cameraController;
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private HUDView _hudView;
        
        [Header("Game Stats")]
        [SerializeField] private GameStats _gameStats;
        
        [Header("Cards")]
        [SerializeField] private CardDefinition _flatDef;
        [SerializeField] private CardDefinition _percDef;
        [SerializeField] private CardDefinition _modDef;

        private ZoneService _zoneService;
        private CurrencyAccrualService _currencyAccrualService;

        private void Awake()
        {
            var eventBus = new EventBus();
            var mobileInput = gameObject.AddComponent<MobileTouchInputProvider>();
            IInputProvider inputProvider = mobileInput;

            var globalUpgradeService = new GlobalUpgradeService(eventBus);
            _zoneService = new ZoneService(eventBus, globalUpgradeService);
            var boardEvaluationService = new BoardEvaluationService(eventBus, _zoneService, globalUpgradeService);
            _currencyAccrualService = new CurrencyAccrualService(_zoneService, eventBus);
            var packOpeningService = new PackOpeningService();

            var startingZoneDef = ScriptableObject.CreateInstance<ZoneDefinition>();
            startingZoneDef.Id = "Test_Zone_01";
            startingZoneDef.Mode = BoardGenerationMode.Fixed;
            startingZoneDef.Layout = ScriptableObject.CreateInstance<BoardLayoutDefinition>();

            startingZoneDef.Layout.UnlockSequence = new List<CellUnlockEntry>
            {
                new CellUnlockEntry { Coordinate = new CellCoord(0, 0), Price = CurrencyValue.Zero },
                new CellUnlockEntry { Coordinate = new CellCoord(1, 0), Price = new CurrencyValue(_gameStats.CellCost[0]) },
                new CellUnlockEntry { Coordinate = new CellCoord(-1, 0), Price = new CurrencyValue(_gameStats.CellCost[1]) },
                new CellUnlockEntry { Coordinate = new CellCoord(0, 1), Price = new CurrencyValue(_gameStats.CellCost[2]) },
                new CellUnlockEntry { Coordinate = new CellCoord(0, -1), Price = new CurrencyValue(_gameStats.CellCost[3]) },
            };
            
            startingZoneDef.StartingCards = new List<CardDefinition> { _flatDef };

            var pricingStrategy = new SequentialPricingStrategy(startingZoneDef.Layout);
            var shopService = new ShopService(eventBus, pricingStrategy, packOpeningService, _zoneService);

            _zoneService.CreateAndActivateZone(startingZoneDef);
            Zone activeZone = _zoneService.GetZone("Test_Zone_01");

            activeZone.Wallet.Add(new CurrencyValue(_gameStats.InitialMoney));
            activeZone.CardInventory.Add(new CardInstance(_flatDef));

            _cameraController.Initialize(inputProvider);
            _boardInputController.Initialize(eventBus, inputProvider, _zoneService, shopService, activeZone.ZoneId);
            _boardView.Initialize(eventBus, _zoneService, shopService, activeZone.ZoneId);
            _inventoryView.Initialize(eventBus, _zoneService, activeZone.ZoneId, inputProvider);
            _hudView.Initialize(eventBus, _zoneService, activeZone.ZoneId);
            
            _mockShopController.Initialize(eventBus, _zoneService, activeZone.ZoneId, _gameStats, _flatDef, _percDef, _modDef);

            eventBus.Subscribe<GenerationRateChangedEvent>(evt =>
            {
                var zone = _zoneService.GetZone(evt.ZoneId);
                if (zone != null) zone.GenerationRate = evt.NewRate;
            });
        }

        private void Update()
        {
            _currencyAccrualService.Tick(Time.deltaTime);
        }
    }
}