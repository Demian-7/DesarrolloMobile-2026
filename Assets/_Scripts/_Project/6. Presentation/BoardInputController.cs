using System;
using UnityEngine;
using Game.Foundation;
using Game.Core;

namespace Game.Presentation
{
    public class BoardInputController : MonoBehaviour
    {
        private enum InputState { Idle, DraggingBoardCard }

        [Header("Drag Visuals")]
        [SerializeField] private CardView _dragGhostPrefab;
        [SerializeField] private BoardView _boardView;

        private IInputProvider _input;
        private ZoneService _zoneService;
        private ShopService _shopService;
        private Camera _mainCamera;
        private string _activeZoneId;

        private InputState _currentState = InputState.Idle;
        private CellCoord _dragOrigin;
        private CellCoord _lastHoveredCoord;
        private CardView _activeGhost;
        private Guid _draggingUICardId;

        private readonly float _cellSpacing = 1.2f;
        private readonly Vector2 _boardOrigin = Vector2.zero;

        public void Initialize(IInputProvider input, ZoneService zoneService, ShopService shopService, string activeZoneId)
        {
            _input = input;
            _zoneService = zoneService;
            _shopService = shopService;
            _activeZoneId = activeZoneId;
            _mainCamera = Camera.main;

            _input.OnCardDropped += HandleUIInventoryDrop;
            
            _input.OnInteractBegan += HandleInteractBegan;
            _input.OnInteractMoved += HandleInteractMoved;
            _input.OnInteractEnded += HandleInteractEnded;
            _input.OnUICardDragBegan += HandleUICardDragBegan;
            _input.OnUICardDragMoved += HandleUICardDragMoved;
            _input.OnUICardDragEnded += HandleUICardDragEnded;
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnCardDropped -= HandleUIInventoryDrop;
                _input.OnInteractBegan -= HandleInteractBegan;
                _input.OnInteractMoved -= HandleInteractMoved;
                _input.OnInteractEnded -= HandleInteractEnded;
                _input.OnUICardDragBegan -= HandleUICardDragBegan;
                _input.OnUICardDragMoved -= HandleUICardDragMoved;
                _input.OnUICardDragEnded -= HandleUICardDragEnded;
            }
        }

        private void HandleInteractBegan(Vector2 screenPosition)
        {
            if (_currentState != InputState.Idle) return;

            CellCoord tapCoord = GetCoordFromScreenPosition(screenPosition);
            Zone zone = _zoneService.GetZone(_activeZoneId);

            if (zone.Board.Cells.TryGetValue(tapCoord, out Cell cell))
            {
                _dragOrigin = tapCoord;

                if (cell.IsUnlocked && cell.OccupiedCard != null)
                {
                    _currentState = InputState.DraggingBoardCard;
                    
                    _activeGhost = Instantiate(_dragGhostPrefab);
                    _activeGhost.Setup(cell.OccupiedCard.Definition);
                    
                    UpdateGhostPosition(screenPosition);
                }
            }
        }

        private void HandleInteractMoved(Vector2 screenPosition)
        {
            if (_currentState == InputState.DraggingBoardCard && _activeGhost != null)
            {
                UpdateGhostPosition(screenPosition);

                CellCoord currentHoverCoord = GetCoordFromScreenPosition(screenPosition);
        
                if (!currentHoverCoord.Equals(_lastHoveredCoord))
                {
                    _lastHoveredCoord = currentHoverCoord;
                    Zone zone = _zoneService.GetZone(_activeZoneId);
            
                    _boardView.ShowDragPreview(zone, zone.Board.Cells[_dragOrigin].OccupiedCard.Definition, currentHoverCoord);
                }
            }
        }

        private void HandleInteractEnded(Vector2 screenPosition)
        {
            CellCoord targetCoord = GetCoordFromScreenPosition(screenPosition);
            Zone zone = _zoneService.GetZone(_activeZoneId);

            if (_currentState == InputState.DraggingBoardCard)
            {
                _boardView.ClearDragPreview();
                
                _currentState = InputState.Idle;
                if (_activeGhost != null) Destroy(_activeGhost.gameObject);

                if (targetCoord.Equals(_dragOrigin))
                {
                    _zoneService.TryPickUpCard(zone, _dragOrigin);
                }
                else if (zone.Board.Cells.TryGetValue(targetCoord, out Cell targetCell) && targetCell.IsUnlocked)
                {
                    _zoneService.TryMoveCardOnBoard(zone, _dragOrigin, targetCoord);
                }
                else
                {
                    _zoneService.TryPickUpCard(zone, _dragOrigin);
                }
            }
            else
            {
                if (targetCoord.Equals(_dragOrigin) && zone.Board.Cells.TryGetValue(targetCoord, out Cell cell) && !cell.IsUnlocked)
                {
                    _shopService.TryPurchaseSlot(zone, targetCoord);
                }
            }
        }

        private void UpdateGhostPosition(Vector2 screenPosition)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _mainCamera.nearClipPlane));
            worldPos.z = -2f;
            _activeGhost.transform.position = worldPos;
        }

        private bool HandleUIInventoryDrop(Guid cardId, Vector2 screenPosition)
        {
            CellCoord targetCoord = GetCoordFromScreenPosition(screenPosition);
            Zone activeZone = _zoneService.GetZone(_activeZoneId);
            return _zoneService.TryPlaceCardFromInventory(activeZone, cardId, targetCoord);
        }

        private CellCoord GetCoordFromScreenPosition(Vector2 screenPosition)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _mainCamera.nearClipPlane));
            int x = Mathf.RoundToInt((worldPos.x - _boardOrigin.x) / _cellSpacing);
            int y = Mathf.RoundToInt((worldPos.y - _boardOrigin.y) / _cellSpacing);
            return new CellCoord(x, y);
        }

        private void HandleUICardDragBegan(Guid cardId, Vector2 screenPosition)
        {
            if (_currentState != InputState.Idle)
                return;

            _draggingUICardId = cardId;
            _lastHoveredCoord = GetCoordFromScreenPosition(screenPosition);
        }

        private void HandleUICardDragMoved(Guid cardId, Vector2 screenPosition)
        {
            if (cardId != _draggingUICardId)
                return;
            
            CellCoord currentHoverCoord = GetCoordFromScreenPosition(screenPosition);

            if (!currentHoverCoord.Equals(_lastHoveredCoord))
            {
                _lastHoveredCoord = currentHoverCoord;
                Zone zone = _zoneService.GetZone(_activeZoneId);

                CardInstance inventoryCard = zone.CardInventory.Find(c => c.InstanceId == cardId);
                if (inventoryCard != null)
                {
                    _boardView.ShowDragPreview(zone, inventoryCard.Definition, currentHoverCoord);
                }
            }
        }

        private void HandleUICardDragEnded(Guid cardId)
        {
            if (cardId == _draggingUICardId)
            {
                _draggingUICardId = Guid.Empty;
                _boardView.ClearDragPreview();
            }
        }
    }
}