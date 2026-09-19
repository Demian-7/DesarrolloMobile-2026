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

        public void Initialize(IEventBus eventBus, ZoneService zoneService, ShopService shopService,
            string activeZoneId)
        {
            _eventBus = eventBus;
            _zoneService = zoneService;
            _shopService = shopService;
            _activeZoneId = activeZoneId;

            _eventBus.Subscribe<SlotUnlockedEvent>(OnSlotUnlocked);
            _eventBus.Subscribe<CardPlacedEvent>(OnCardPlaced);
            _eventBus.Subscribe<CardRemovedFromBoardEvent>(OnCardRemoved);
            _eventBus.Subscribe<BoardChangedEvent>(OnBoardChanged);

            RenderFullBoard();
        }

        private void OnDestroy()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<SlotUnlockedEvent>(OnSlotUnlocked);
            _eventBus.Unsubscribe<CardPlacedEvent>(OnCardPlaced);
            _eventBus.Unsubscribe<CardRemovedFromBoardEvent>(OnCardRemoved);
            _eventBus.Unsubscribe<BoardChangedEvent>(OnBoardChanged);
        }

        public void ShowDragPreview(Zone zone, CardDefinition cardDef, CellCoord hoverCoord)
        {
            ClearDragPreview();
            List<Cell> targets = GetTargetCellsForCard(zone, cardDef, hoverCoord);

            if (targets == null)
                return;

            foreach (var target in targets)
            {
                if (_activeCells.TryGetValue(target.Coordinate, out CellView cellView))
                {
                    cellView.SetHighlight(true);
                }
            }
        }

        public void ClearDragPreview()
        {
            foreach (var cellView in _activeCells.Values)
            {
                cellView.SetHighlight(false);
            }
        }

        private List<Cell> GetTargetCellsForCard(Zone zone, CardDefinition cardDef, CellCoord originCoord)
        {
            var targets = new List<Cell>();
            bool hasInteraction = false;

            foreach (var effect in cardDef.Effects)
            {
                CardInteraction interaction = null;

                if (effect is PercentageGenerationEffect perc) interaction = perc.Interaction;
                else if (effect is BuffEffect buff) interaction = buff.Interaction;
                else if (effect is DebuffEffect debuff) interaction = debuff.Interaction;

                if (interaction != null)
                {
                    hasInteraction = true;
                    targets.AddRange(zone.Board.ResolveInteraction(interaction, originCoord));
                }
            }

            return hasInteraction ? targets : null;
        }

        private void RenderFullBoard()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null)
                return;

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
            if (evt.ZoneId != _activeZoneId)
                return;

            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null)
                return;

            if (zone.Board.Cells.TryGetValue(evt.Coord, out Cell cell) && cell.OccupiedCard != null)
            {
                SpawnCardVisual(cell.OccupiedCard, evt.Coord);
            }
        }

        private void OnCardRemoved(CardRemovedFromBoardEvent evt)
        {
            if (evt.ZoneId != _activeZoneId)
                return;

            if (_activeCards.TryGetValue(evt.Coord, out CardView cardView))
            {
                Destroy(cardView.gameObject);
                _activeCards.Remove(evt.Coord);
            }
        }

        private void SpawnCardVisual(CardInstance card, CellCoord coord)
        {
            Vector3 worldPos = GetWorldPosition(coord);
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

        private void OnBoardChanged(BoardChangedEvent evt)
        {
            if (evt.ZoneId == _activeZoneId)
            {
                RefreshAllCardVisualizers();
            }
        }

        private void RefreshAllCardVisualizers()
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            foreach (var kvp in _activeCards)
            {
                CellCoord coord = kvp.Key;
                CardView cardView = kvp.Value;

                if (zone.Board.Cells.TryGetValue(coord, out Cell cell) && cell.OccupiedCard != null)
                {
                    UpdateSingleCardVisualizer(zone, cell.OccupiedCard, coord, cardView);
                }
            }
        }

        private void UpdateSingleCardVisualizer(Zone zone, CardInstance card, CellCoord coord, CardView cardView)
        {
            List<Cell> targets = GetTargetCellsForCard(zone, card.Definition, coord);

            if (targets == null)
            {
                cardView.ShowWarning(false);
                cardView.DrawLinesToTargets(Vector3.zero, new List<Vector3>());
                return;
            }

            Vector3 cardWorldPos = GetWorldPosition(coord);
            cardWorldPos.z -= 0.1f; // Keep lines at the card's Z-depth

            List<Vector3> validTargetPositions = new List<Vector3>();
            foreach (var targetCell in targets)
            {
                if (targetCell.OccupiedCard != null)
                {
                    validTargetPositions.Add(GetWorldPosition(targetCell.Coordinate));
                }
            }

            bool isPercentageGenerator = false;
            foreach (var effect in card.Definition.Effects)
            {
                if (effect is PercentageGenerationEffect)
                {
                    isPercentageGenerator = true;
                    break;
                }
            }

            bool showWarning = false;

            if (validTargetPositions.Count == 0)
            {
                showWarning = true;
            }
            else if (isPercentageGenerator && card.EffectiveValue.Equals(CurrencyValue.Zero))
            {
                showWarning = true;
            }

            cardView.ShowWarning(showWarning);

            if (validTargetPositions.Count == 0)
            {
                cardView.DrawLinesToTargets(cardWorldPos, new List<Vector3>());
            }
            else
            {
                cardView.DrawLinesToTargets(cardWorldPos, validTargetPositions);
            }
        }
    }
}