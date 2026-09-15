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

        [SerializeField] private GameObject _inventoryItemPrefab; // A UI Prefab with an Image/Text
        [SerializeField] private Transform _scrollRectContent;    // The Content transform of the ScrollRect

        private List<GameObject> _activeUIItems = new List<GameObject>();

        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;

            // The inventory changes when a card is placed, or (later) when a pack is opened
            _eventBus.Subscribe<CardPlacedEvent>(OnCardPlaced);
            
            RenderInventory();
        }

        private void OnCardPlaced(CardPlacedEvent evt)
        {
            // Re-render the inventory to reflect the removed card
            RenderInventory();
        }

        private void RenderInventory()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            // Clear old UI items
            foreach (var item in _activeUIItems) Destroy(item);
            _activeUIItems.Clear();

            // Populate from the logical CardInventory
            foreach (var card in zone.CardInventory)
            {
                GameObject uiInstance = Instantiate(_inventoryItemPrefab, _scrollRectContent);
                // TODO: Get InventoryCardView component and call Setup(card.Id, card.InstanceId)
                _activeUIItems.Add(uiInstance);
            }
        }
    }
}