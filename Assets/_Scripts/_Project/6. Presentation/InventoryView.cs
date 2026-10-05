using UnityEngine;
using Game.Foundation;
using Game.Core;
using System.Collections.Generic;

namespace Game.Presentation
{
    public class InventoryView : MonoBehaviour
    {
        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private string _activeZoneId;

        [SerializeField] private InventoryCardView _inventoryItemPrefab; 
        [SerializeField] private Transform _scrollRectContent;    

        private List<InventoryCardView> _activeUIItems = new();
        private IInputProvider _inputProvider;
        
        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId, IInputProvider inputProvider)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;
            _inputProvider = inputProvider;

            _eventBus.Subscribe<CardPlacedEvent>(OnCardPlaced);
            _eventBus.Subscribe<PackOpenedEvent>(OnPackOpened);
            _eventBus.Subscribe<CardRemovedFromBoardEvent>(OnCardRemovedFromBoard);
            
            RenderInventory();
        }

        private void OnDestroy()
        {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<CardPlacedEvent>(OnCardPlaced);
            _eventBus.Unsubscribe<PackOpenedEvent>(OnPackOpened);
            _eventBus.Unsubscribe<CardRemovedFromBoardEvent>(OnCardRemovedFromBoard);
        }

        private void OnCardPlaced(CardPlacedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId) RenderInventory();
        }
        
        private void OnCardRemovedFromBoard(CardRemovedFromBoardEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                RenderInventory();
            }
        }
        
        private void OnPackOpened(PackOpenedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId) RenderInventory();
        }

        private void RenderInventory()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            foreach (var item in _activeUIItems) 
            {
                item.gameObject.SetActive(false); 
                Destroy(item.gameObject);
            }
            _activeUIItems.Clear();

            foreach (var card in zone.CardInventory)
            {
                InventoryCardView uiInstance = Instantiate(_inventoryItemPrefab, _scrollRectContent);
        
                uiInstance.gameObject.SetActive(true);
        
                uiInstance.transform.localScale = Vector3.one; 
        
                uiInstance.Setup(card, _inputProvider, _eventBus);
                _activeUIItems.Add(uiInstance);
            }
        }
    }
}