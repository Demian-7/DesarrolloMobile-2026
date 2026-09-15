using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Foundation;
using Game.Core;
using Game.Input.Desktop;
using Game.Presentation;

namespace Game.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Views & Controllers")]
        [SerializeField] private BoardInputController _boardInputController;
        [SerializeField] private BoardView _boardView;

        private ZoneService _zoneService;
        private CurrencyAccrualService _currencyAccrualService;
        
        private void Awake()
        {
            var eventBus = new EventBus();

            var desktopInput = gameObject.AddComponent<DesktopInputProvider>();
            IInputProvider inputProvider = desktopInput;

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
                new CellUnlockEntry { Coordinate = new CellCoord(0, 0), Price = CurrencyValue.Zero }, // Center (Free)
                new CellUnlockEntry { Coordinate = new CellCoord(1, 0), Price = new CurrencyValue(10) }, // Right
                new CellUnlockEntry { Coordinate = new CellCoord(-1, 0), Price = new CurrencyValue(20) }, // Left
                new CellUnlockEntry { Coordinate = new CellCoord(0, 1), Price = new CurrencyValue(50) }, // Top
                new CellUnlockEntry { Coordinate = new CellCoord(0, -1), Price = new CurrencyValue(100) } // Bottom
            };
            startingZoneDef.StartingCards = new List<CardDefinition>(); 
            
            var pricingStrategy = new SequentialPricingStrategy(startingZoneDef.Layout);
            var shopService = new ShopService(eventBus, pricingStrategy, packOpeningService, _zoneService);

            _zoneService.CreateAndActivateZone(startingZoneDef);
            Zone activeZone = _zoneService.GetZone("Test_Zone_01");

            activeZone.Wallet.Add(new CurrencyValue(200));

            List<CardInstance> starterCards = SeedStarterInventory(activeZone);
            desktopInput.SetMockCardQueue(starterCards);

            _boardInputController.Initialize(inputProvider, _zoneService, shopService);
            _boardView.Initialize(eventBus, _zoneService, activeZone.ZoneId);
            
            eventBus.Subscribe<GenerationRateChangedEvent>(evt =>
            {
                var zone = _zoneService.GetZone(evt.ZoneId);
                if (zone != null)
                {
                    zone.GenerationRate = evt.NewRate;
                    Debug.Log($"Zone {evt.ZoneId} New Generation Rate: {evt.NewRate}");
                }
            });

            eventBus.Subscribe<CurrencyChangedEvent>(evt => 
            {
                // Comment this out if it spams the console too much!
                Debug.Log($"Balance: {evt.NewBalance}"); 
            });
            
            Debug.Log("Game Bootstrapped Successfully! Right-click to place cards, Left-click red cells to buy them.");
        }

        private List<CardInstance> SeedStarterInventory(Zone targetZone)
        {
            var flatDef = ScriptableObject.CreateInstance<CardDefinition>();
            flatDef.Id = "Gen_Flat_1";
            flatDef.Effects.Add(new FlatGenerationEffect { BaseAmount = new CurrencyValue(10) });

            var percDef = ScriptableObject.CreateInstance<CardDefinition>();
            percDef.Id = "Gen_Perc_1";
            percDef.Effects.Add(new PercentageGenerationEffect
            {
                Interaction = new CardInteraction { Type = InteractionType.Adjacent, Range = 1 },
                Percentage = 2.0f 
            });

            var modDef = ScriptableObject.CreateInstance<CardDefinition>();
            modDef.Id = "Mod_Multi_1";
            modDef.Effects.Add(new BuffEffect
            {
                Interaction = new CardInteraction { Type = InteractionType.Adjacent, Range = 1 },
                Magnitude = 2.0f 
            });

            var starterCards = new List<CardInstance> { new CardInstance(flatDef), new CardInstance(percDef), new CardInstance(modDef) };
            targetZone.CardInventory.AddRange(starterCards);
            return starterCards;
        }

        private void Update()
        {
            _currencyAccrualService.Tick(Time.deltaTime);
        }
    }
}