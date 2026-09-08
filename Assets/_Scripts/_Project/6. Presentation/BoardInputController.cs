using System;
using Game.Core;
using Game.Foundation;
using UnityEngine;

namespace Game.Presentation
{
    public class BoardInputController : MonoBehaviour
    {
        private IInputProvider _input;
        private ShopService _shopService;
        private ZoneService _zoneService;
        private Camera _mainCamera;

        public void Initialize(IInputProvider input, ShopService shopService, ZoneService zoneService)
        {
            _input = input;
            _shopService = shopService;
            _zoneService = zoneService;
            _mainCamera = Camera.main;

            _input.OnScreenTapped += HandleScreenTapped;
            _input.OnCardDropped += HandleCardDropped;
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnScreenTapped -= HandleScreenTapped;
                _input.OnCardDropped -= HandleCardDropped;
            }
        }

        private void HandleScreenTapped(Vector2 screenPos)
        {
            if (TryGetCellCoordFromScreen(screenPos, out CellCoord coord))
            {
                Zone activeZone = _zoneService.GetZonesToSimulate()[0];
                _shopService.TryPurchaseSlot(activeZone, coord, new SequentialPricingStrategy(new CurrencyValue(10)));
            }
        }

        private void HandleCardDropped(Guid cardId, Vector2 screenPos)
        {
            if (TryGetCellCoordFromScreen(screenPos, out CellCoord coord))
            {
                Debug.Log($"Card {cardId} dropped on grid {coord}");
            }
        }

        private bool TryGetCellCoordFromScreen(Vector2 screenPos, out CellCoord coord)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            Plane gridPlane = new Plane(Vector3.back, Vector3.zero);

            if (gridPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                coord = new CellCoord(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
                return true;
            }

            coord = default;
            return false;
        }
    }
}