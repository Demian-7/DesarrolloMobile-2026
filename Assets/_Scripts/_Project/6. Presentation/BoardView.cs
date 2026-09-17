using System.Collections.Generic;
using Game.Core;
using Game.Foundation;
using UnityEngine;

namespace Game.Presentation
{
    public class BoardView : MonoBehaviour
    {
        private IEventBus _eventBus;
        private ZoneService _zoneService;
        private ShopService _shopService;
        private string _activeZoneId;

        [SerializeField] private CellView _cellPrefab;
        [SerializeField] private CardView _cardPrefab;

        private Dictionary<CellCoord, CellView> _activeCells = new();
        private Dictionary<CellCoord, CardView> _activeCards = new();
        
        private readonly float _cellSpacing = 1.2f;
        private readonly Vector2 _boardOrigin = Vector2.zero;

        public void Initialize(IEventBus eventBus, ZoneService zoneService, ShopService shopService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _shopService = shopService;
            _activeZoneId = activeZoneId;

            _eventBus.Subscribe<SlotUnlockedEvent>(OnSlotUnlocked);
            _eventBus.Subscribe<CardPlacedEvent>(OnCardPlaced);
            
            RenderFullBoard();
        }

        private void OnDestroy()
        {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<SlotUnlockedEvent>(OnSlotUnlocked);
            _eventBus.Unsubscribe<CardPlacedEvent>(OnCardPlaced);
        }

        private void RenderFullBoard()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            foreach (var cell in _activeCells.Values) 
                Destroy(cell.gameObject);
            
            foreach (var card in _activeCards.Values) 
                Destroy(card.gameObject);
            
            _activeCells.Clear();
            _activeCards.Clear();

            foreach (var kvp in zone.Board.Cells)
            {
                Cell cell = kvp.Value;
                Vector3 worldPos = GetWorldPosition(cell.Coordinate);
                
                CellView cellInstance = Instantiate(_cellPrefab, worldPos, Quaternion.identity, transform);

                string priceStr = "";
                if (!cell.IsUnlocked)
                {
                    CurrencyValue price = _shopService.GetSlotPrice(zone, cell.Coordinate);
                    priceStr = $"${price}";
                }
                
                cellInstance.Setup(cell.IsUnlocked, priceStr);
                _activeCells.Add(cell.Coordinate, cellInstance);

                if (cell.OccupiedCard != null)
                {
                    SpawnCardVisual(cell.OccupiedCard, cell.Coordinate);
                }
            }
        }

        private void OnSlotUnlocked(SlotUnlockedEvent evt)
        {
            if (evt.ZoneId != _activeZoneId) 
                return;
            
            if (_activeCells.TryGetValue(evt.Coord, out CellView cellView))
            {
                cellView.Setup(true, "");
            }
        }

        private void OnCardPlaced(CardPlacedEvent evt)
        {
            if (evt.ZoneId != _activeZoneId) return;
            
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            if (zone.Board.Cells.TryGetValue(evt.Coord, out Cell cell) && cell.OccupiedCard != null)
            {
                SpawnCardVisual(cell.OccupiedCard, evt.Coord);
            }
        }

        private void SpawnCardVisual(CardInstance card, CellCoord coord)
        {
            Vector3 worldPos = GetWorldPosition(coord);
            
            // Slight Z-offset so cards render in front of cells
            worldPos.z -= 0.1f; 

            CardView cardInstance = Instantiate(_cardPrefab, worldPos, Quaternion.identity, transform);
            cardInstance.Setup(card.Definition);
            _activeCards[coord] = cardInstance;
        }

        private Vector3 GetWorldPosition(CellCoord coord)
        {
            return new Vector3(
                _boardOrigin.x + coord.X * _cellSpacing,
                _boardOrigin.y + coord.Y * _cellSpacing,
                0);
        }
    }
}