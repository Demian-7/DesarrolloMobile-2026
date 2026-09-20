using System.Collections.Generic;
using Game.Core;
using Game.Foundation;
using Game.Input.Desktop;
using Game.Input.Mobile;
using Game.Presentation;
using UnityEngine;

namespace Game.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Views & Controllers")] [SerializeField]
        private BoardInputController _boardInputController;

        [SerializeField] private BoardView _boardView;

        [Header("New Phase 2 References")] [SerializeField]
        private BoardCameraController _cameraController;

        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private HUDView _hudView;

        private ZoneService _zoneService;
        private CurrencyAccrualService _currencyAccrualService;

        private void Awake()
        {
            var eventBus = new EventBus();

            // var desktopInput = gameObject.AddComponent<DesktopInputProvider>();
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
                new CellUnlockEntry { Coordinate = new CellCoord(1, 0), Price = new CurrencyValue(10) },
                new CellUnlockEntry { Coordinate = new CellCoord(-1, 0), Price = new CurrencyValue(20) },
                new CellUnlockEntry { Coordinate = new CellCoord(0, 1), Price = new CurrencyValue(50) },
                new CellUnlockEntry { Coordinate = new CellCoord(0, -1), Price = new CurrencyValue(100) }
            };
            startingZoneDef.StartingCards = new List<CardDefinition>();

            var pricingStrategy = new SequentialPricingStrategy(startingZoneDef.Layout);
            var shopService = new ShopService(eventBus, pricingStrategy, packOpeningService, _zoneService);

            _zoneService.CreateAndActivateZone(startingZoneDef);
            Zone activeZone = _zoneService.GetZone("Test_Zone_01");

            activeZone.Wallet.Add(new CurrencyValue(200));

            List<CardInstance> starterCards = SeedStarterInventory(activeZone);

            _cameraController.Initialize(inputProvider);
            _boardInputController.Initialize(eventBus, inputProvider, _zoneService, shopService, activeZone.ZoneId);
            _boardView.Initialize(eventBus, _zoneService, shopService, activeZone.ZoneId);
            _inventoryView.Initialize(eventBus, _zoneService, activeZone.ZoneId, inputProvider);

            _hudView.Initialize(eventBus, _zoneService, activeZone.ZoneId);

            eventBus.Subscribe<GenerationRateChangedEvent>(evt =>
            {
                var zone = _zoneService.GetZone(evt.ZoneId);
                if (zone != null)
                {
                    zone.GenerationRate = evt.NewRate;
                }
            });
        }

        private List<CardInstance> SeedStarterInventory(Zone targetZone)
        {
            var flatDef = ScriptableObject.CreateInstance<CardDefinition>();
            flatDef.Id = "Gen_Flat_1";
            flatDef.DisplayName = "Basic";
            flatDef.IconColor = new Color(0.2f, 0.6f, 1f);
            flatDef.Effects.Add(new FlatGenerationEffect { BaseAmount = new CurrencyValue(10) });

            var percDef = ScriptableObject.CreateInstance<CardDefinition>();
            percDef.Id = "Gen_Perc_1";
            percDef.DisplayName = "Multiplier";

            percDef.IconColor = new Color(0.6f, 0.2f, 0.8f);

            percDef.Effects.Add(new PercentageGenerationEffect
            {
                Interaction = new CardInteraction { Type = InteractionType.Adjacent, Range = 1 },
                Percentage = 2.0f
            });

            var modDef = ScriptableObject.CreateInstance<CardDefinition>();
            modDef.Id = "Mod_Multi_1";
            modDef.DisplayName = "Buffer";
            modDef.IconColor = new Color(0.4f, 0.8f, 0.4f);
            modDef.Effects.Add(new BuffEffect
            {
                Interaction = new CardInteraction { Type = InteractionType.Adjacent, Range = 1 },
                Magnitude = 2.0f
            });

            var starterCards = new List<CardInstance>
                { new CardInstance(flatDef), new CardInstance(percDef), new CardInstance(modDef) };
            targetZone.CardInventory.AddRange(starterCards);
            return starterCards;
        }

        private void Update()
        {
            _currencyAccrualService.Tick(Time.deltaTime);
        }
    }
}