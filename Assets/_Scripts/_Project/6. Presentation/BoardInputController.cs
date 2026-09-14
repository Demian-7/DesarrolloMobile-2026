using System;
using Game.Core;
using Game.Foundation;
using UnityEngine;

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

        public void Initialize(IInputProvider input, ZoneService zoneService, ShopService shopService)
        {
            _input = input;
            _zoneService = zoneService;
            _shopService = shopService;
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

        private void HandleScreenTapped(Vector2 screenPos)
        {
            if (TryGetCellCoordFromScreen(screenPos, out CellCoord coord))
            {
                Zone activeZone = _zoneService.GetZonesToSimulate()[0];
                
                if (_shopService.TryPurchaseSlot(activeZone, coord))
                {
                    Debug.Log($"Successfully purchased cell at {coord}!");
                }
            }
        }

        private bool HandleCardDropped(Guid cardId, Vector2 screenPos)
        {
            if (TryGetCellCoordFromScreen(screenPos, out CellCoord coord))
            {
                Zone activeZone = _zoneService.GetZonesToSimulate()[0];
                
                bool success = _zoneService.TryPlaceCardFromInventory(activeZone, cardId, coord);
                Debug.Log(success ? $"Placed {cardId} at {coord}" : $"Failed to place {cardId}");
                return success;
            }
            return false;
        }

        private bool TryGetCellCoordFromScreen(Vector2 screenPos, out CellCoord coord)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            Plane gridPlane = new Plane(Vector3.back, Vector3.zero);

            if (gridPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                
                int x = Mathf.RoundToInt((worldPos.x - _boardOrigin.x) / _cellSpacing);
                int y = Mathf.RoundToInt((worldPos.y - _boardOrigin.y) / _cellSpacing);
                
                coord = new CellCoord(x, y);
                return true;
            }

            coord = default;
            return false;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _zoneService == null) return;
            var zones = _zoneService.GetZonesToSimulate();
            if (zones == null || zones.Count == 0) return;
            Zone activeZone = zones[0];
            if (activeZone.Board == null) return;

            foreach (var cell in activeZone.Board.GetAllCells())
            {
                Vector3 worldPos = new Vector3(
                    _boardOrigin.x + cell.Coordinate.X * _cellSpacing,
                    _boardOrigin.y + cell.Coordinate.Y * _cellSpacing, 0);
                    
                Vector3 size = new Vector3(_cellSpacing * 0.9f, _cellSpacing * 0.9f, 0);

                if (cell.OccupiedCard != null)
                {
                    Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.8f); 
                    Gizmos.DrawCube(worldPos, size);
                }
                else if (cell.IsUnlocked)
                {
                    Gizmos.color = Color.green; 
                    Gizmos.DrawWireCube(worldPos, size);
                }
                else
                {
                    Gizmos.color = Color.red; 
                    Gizmos.DrawWireCube(worldPos, size);
                }
            }
        }
    }
}