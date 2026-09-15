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

        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _cardPrefab;

        private Dictionary<CellCoord, GameObject> _activeVisuals = new();
        private readonly float _cellSpacing = 1.2f;
        private readonly Vector2 _boardOrigin = Vector2.zero;
        private string _activeZoneId;

        public void Initialize(IEventBus eventBus, ZoneService zoneService, string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _activeZoneId = activeZoneId;

            _eventBus.Subscribe<BoardChangedEvent>(OnBoardChanged);
            RenderFullBoard();
        }

        private void OnBoardChanged(BoardChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
                RenderFullBoard();
        }

        private void RenderFullBoard()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null)
                return;

            foreach (var go in _activeVisuals.Values)
            {
                Destroy(go);
            }
            _activeVisuals.Clear();

            foreach (var kvp in zone.Board.Cells)
            {
                Cell cell = kvp.Value;

                Vector3 worldPos = new Vector3(
                    _boardOrigin.x + cell.Coordinate.X * _cellSpacing,
                    _boardOrigin.y + cell.Coordinate.Y * _cellSpacing,
                    0);
                
                GameObject instance = Instantiate(cell.OccupiedCard != null ? _cardPrefab : _cellPrefab, worldPos, Quaternion.identity, transform);
                _activeVisuals.Add(cell.Coordinate, instance);
            }
        }
    }
}