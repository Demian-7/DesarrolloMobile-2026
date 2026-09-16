using System;
using UnityEngine;
using Game.Foundation;
using Game.Core;

namespace Game.Presentation
{
    public class BoardInputController : MonoBehaviour
    {
        private IInputProvider _input;
        private ZoneService _zoneService;
        private ShopService _shopService;
        private Camera _mainCamera;

        private readonly float _cellSpacing = 1.2f;
        private readonly Vector2 _boardOrigin = Vector2.zero;
        private string _activeZoneId;

        public void Initialize(IInputProvider input, ZoneService zoneService, ShopService shopService, string activeZoneId)
        {
            _input = input;
            _zoneService = zoneService;
            _shopService = shopService;
            _activeZoneId = activeZoneId;
            _mainCamera = Camera.main;

            _input.OnCardDropped += HandleCardDropped;
            _input.OnScreenTapped += HandleScreenTapped;
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnCardDropped -= HandleCardDropped;
                _input.OnScreenTapped -= HandleScreenTapped;
            }
        }

        private CellCoord GetCoordFromScreenPosition(Vector2 screenPosition)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _mainCamera.nearClipPlane));
            
            int x = Mathf.RoundToInt((worldPos.x - _boardOrigin.x) / _cellSpacing);
            int y = Mathf.RoundToInt((worldPos.y - _boardOrigin.y) / _cellSpacing);
            
            return new CellCoord(x, y);
        }

        private bool HandleCardDropped(Guid cardId, Vector2 screenPosition)
        {
            CellCoord targetCoord = GetCoordFromScreenPosition(screenPosition);
            Zone activeZone = _zoneService.GetZone(_activeZoneId);
            
            return _zoneService.TryPlaceCardFromInventory(activeZone, cardId, targetCoord);
        }

        private void HandleScreenTapped(Vector2 screenPosition)
        {
            CellCoord targetCoord = GetCoordFromScreenPosition(screenPosition);
            Zone activeZone = _zoneService.GetZone(_activeZoneId);
            
            if (activeZone.Board.Cells.TryGetValue(targetCoord, out Cell cell))
            {
                if (!cell.IsUnlocked)
                {
                    bool purchaseSuccessful = _shopService.TryPurchaseSlot(activeZone, targetCoord);
                    
                    if (purchaseSuccessful)
                    {
                        Debug.Log($"Successfully purchased cell at {targetCoord}!");
                    }
                    else
                    {
                        Debug.Log($"Failed to purchase cell at {targetCoord}. Not enough currency or not next in sequence.");
                    }
                }
            }
        }
    }
}