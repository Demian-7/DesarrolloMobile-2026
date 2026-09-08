using System;
using UnityEngine;
using Game.Foundation;
using Game.Core;
using Game.Input.Desktop;
using Game.Presentation;

namespace Game.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private BoardInputController _boardInputController;

        private void Awake()
        {
            var eventBus = new EventBus();

            IInputProvider inputProvider = gameObject.AddComponent<DesktopInputProvider>();
            
            var zoneService = new ZoneService(eventBus);
            var shopService = new ShopService(eventBus);
            
            _boardInputController.Initialize(inputProvider, shopService, zoneService);
            
            Debug.Log("Game Bootstrapped Successfully!");
        }
    }
}