using System.Collections.Generic;
using DG.Tweening;
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
        [SerializeField] private ZoneBackground _background;

        [Header("Layout")]
        [Tooltip("20px tile + 2px gap = 22px = 1.375 units at PPU 16.")]
        [SerializeField] private float _cellSpacing = 1.375f;

        [Header("Chain pulse")]
        [SerializeField] private float _pulseStepSeconds = 0.2f;
        [SerializeField] private int _maxPulseSteps = 8;

        [Header("Card transitions")]
        [SerializeField] private float _removeSeconds = 0.3f;
        [SerializeField] private float _zoneOutSeconds = 0.25f;
        [SerializeField] private float _zoneInSeconds = 0.35f;
        [Tooltip("Delay between neighbouring cells in the zone-switch wave.")]
        [SerializeField] private float _zoneStaggerSeconds = 0.03f;
        [Tooltip("New cards start appearing this long after the old ones start fading.")]
        [SerializeField] private float _zoneInStartDelay = 0.2f;

        private Dictionary<CellCoord, CellView> _activeCells = new();
        private Dictionary<CellCoord, CardView> _activeCards = new();
        private Sequence _pulseSequence;

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
            _eventBus.Subscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);

            RenderFullBoard(false);
        }

        private void OnDestroy()
        {
            KillPulse();

            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<SlotUnlockedEvent>(OnSlotUnlocked);
            _eventBus.Unsubscribe<CardPlacedEvent>(OnCardPlaced);
            _eventBus.Unsubscribe<CardRemovedFromBoardEvent>(OnCardRemoved);
            _eventBus.Unsubscribe<BoardChangedEvent>(OnBoardChanged);
            _eventBus.Unsubscribe<ActiveZoneChangedEvent>(OnActiveZoneChanged);
        }

        private Color CurrentCellTint()
            => _background != null ? _background.GetMidColor(_activeZoneId) : Color.white;

        private float WaveDelay(CellCoord coord)
            => Mathf.Clamp(coord.X + coord.Y, 0, 20) * _zoneStaggerSeconds;

        private void OnActiveZoneChanged(ActiveZoneChangedEvent evt)
        {
            _activeZoneId = evt.ActiveZoneId;
            ClearDragPreview();
            RenderFullBoard(true);
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

        private void RenderFullBoard(bool animated)
        {
            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null)
                return;

            KillPulse();

            foreach (var cell in _activeCells.Values)
                Destroy(cell.gameObject);

            foreach (var kvp in _activeCards)
            {
                if (animated)
                    kvp.Value.PlayRemoval(_zoneOutSeconds, WaveDelay(kvp.Key));
                else
                    Destroy(kvp.Value.gameObject);
            }

            _activeCells.Clear();
            _activeCards.Clear();

            Color tint = CurrentCellTint();

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

                cellInstance.Setup(cell.IsUnlocked, priceStr, tint);
                _activeCells.Add(cell.Coordinate, cellInstance);

                if (cell.OccupiedCard != null)
                {
                    SpawnCardVisual(cell.OccupiedCard, cell.Coordinate, animated,
                        _zoneInStartDelay + WaveDelay(cell.Coordinate), _zoneInSeconds);
                }
            }

            RefreshAllCardVisualizers();
        }

        private void OnSlotUnlocked(SlotUnlockedEvent evt)
        {
            if (evt.ZoneId != _activeZoneId)
                return;

            if (_activeCells.TryGetValue(evt.Coord, out CellView cellView))
            {
                cellView.Setup(true, "", CurrentCellTint());
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
                SpawnCardVisual(cell.OccupiedCard, evt.Coord, true, 0f, 0.5f);
            }
        }

        private void OnCardRemoved(CardRemovedFromBoardEvent evt)
        {
            if (evt.ZoneId != _activeZoneId)
                return;

            if (_activeCards.TryGetValue(evt.Coord, out CardView cardView))
            {
                _activeCards.Remove(evt.Coord);
                cardView.PlayRemoval(_removeSeconds);
            }
        }

        private void SpawnCardVisual(CardInstance card, CellCoord coord, bool animateIn,
            float delay, float duration)
        {
            Vector3 worldPos = GetWorldPosition(coord);
            worldPos.z -= 0.1f;

            CardView cardInstance = Instantiate(_cardPrefab, worldPos, Quaternion.identity, transform);
            cardInstance.Setup(card, _eventBus);
            _activeCards[coord] = cardInstance;

            if (animateIn)
                cardInstance.Fx?.DissolveIn(duration, delay);
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
            if (evt.ZoneId != _activeZoneId)
                return;

            RefreshAllCardVisualizers();
            PlayChainPulse();
        }

        private void PlayChainPulse()
        {
            KillPulse();

            Zone zone = _zoneService.GetZone(_activeZoneId);
            if (zone == null) return;

            var pulses = new List<(int step, CardView view)>();
            var valuePerStep = new SortedDictionary<int, CurrencyValue>();

            foreach (var kvp in _activeCards)
            {
                if (!zone.Board.Cells.TryGetValue(kvp.Key, out Cell cell) || cell.OccupiedCard == null)
                    continue;

                int pass = cell.OccupiedCard.ResolvedPass;
                if (pass < 0) continue;

                int step = Mathf.Min(pass, _maxPulseSteps);
                pulses.Add((step, kvp.Value));

                CurrencyValue value = cell.OccupiedCard.EffectiveValue;
                valuePerStep[step] = valuePerStep.TryGetValue(step, out var existing) ? existing + value : value;
            }

            if (pulses.Count == 0) return;

            _pulseSequence = DOTween.Sequence().SetLink(gameObject);

            foreach (var p in pulses)
            {
                CardView view = p.view;
                _pulseSequence.InsertCallback(p.step * _pulseStepSeconds, () =>
                {
                    if (view != null) view.Fx?.Pulse();
                });
            }

            string zoneId = _activeZoneId;
            int stepCount = valuePerStep.Count;
            int index = 0;
            CurrencyValue running = CurrencyValue.Zero;

            foreach (var kvp in valuePerStep)
            {
                running += kvp.Value;
                CurrencyValue total = running;
                int stepIndex = index++;

                _pulseSequence.InsertCallback(kvp.Key * _pulseStepSeconds,
                    () => _eventBus.Publish(new ChainStepEvent(zoneId, total, stepIndex, stepCount)));
            }
        }

        private void KillPulse()
        {
            _pulseSequence?.Kill();
            _pulseSequence = null;
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
                cardView.SetUnpowered(false);
                cardView.DrawLinesToTargets(Vector3.zero, new List<Vector3>());
                return;
            }

            Vector3 cardWorldPos = GetWorldPosition(coord);
            cardWorldPos.z -= 0.1f;

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

            bool unpowered = false;

            if (validTargetPositions.Count == 0)
            {
                unpowered = true;
            }
            else if (isPercentageGenerator && card.EffectiveValue.Equals(CurrencyValue.Zero))
            {
                unpowered = true;
            }

            cardView.SetUnpowered(unpowered);

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