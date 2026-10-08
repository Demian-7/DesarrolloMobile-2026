using System.Collections.Generic;
using System.Linq;
using Game.Foundation;
using UnityEngine;
using Random = System.Random;

namespace Game.Core
{
    public class PackOpeningService
    {
        private readonly Random _random = new();

        public List<CardDefinition> Draw(CardPackDefinition pack)
        {
            var results = new List<CardDefinition>();
            
            int drawCount = _random.Next(pack.MinCards, pack.MaxCards + 1);
            
            List<WeightedCardEntry> pool = BuildPool(pack);
            
            if (pool == null || pool.Count == 0)
            {
                Debug.LogError($"[PackOpeningService] Pack '{pack.Id}' generated an empty pool. Check your filters or Card Set.");
                return results;
            }

            for (int i = 0; i < drawCount; i++)
            {
                results.Add(WeightedPick(pool));
            }

            var guaranteedPool = pool.Where(e => e.Card.Tier >= pack.MinimumGuaranteedTier).ToList();
            int shortfall = pack.MinimumGuaranteedCount - results.Count(c => c.Tier >= pack.MinimumGuaranteedTier);
            
            if (shortfall > 0 && guaranteedPool.Count > 0)
            {
                foreach (var card in results.OrderBy(c => c.Tier).Take(shortfall).ToList())
                {
                    results.Remove(card);
                    results.Add(WeightedPick(guaranteedPool));
                }
            }
            else if (shortfall > 0 && guaranteedPool.Count == 0)
            {
                Debug.LogWarning($"[PackOpeningService] Pack '{pack.Id}' requires a guarantee, but no cards in its pool meet the criteria.");
            }

            return results;
        }

        private List<WeightedCardEntry> BuildPool(CardPackDefinition pack)
        {
            IEnumerable<WeightedCardEntry> basePool;

            if (pack.CardSet != null && pack.CardSet.Pool.Count > 0)
            {
                basePool = pack.CardSet.Pool;
            }
            else
            {
                basePool = Resources.LoadAll<CardDefinition>("Cards")
                    .Select(card => new WeightedCardEntry { Card = card, Weight = 1f });
            }

            var filteredPool = basePool
                .Where(entry => MatchesFilter(entry.Card, pack))
                .ToList();

            return filteredPool;
        }

        private bool MatchesFilter(CardDefinition candidate, CardPackDefinition pack)
        {
            if (candidate == null) return false;

            return pack.FilterType switch
            {
                InteractionFilterType.None => true,
                InteractionFilterType.ExactType => candidate == pack.FilterByType,
                InteractionFilterType.Tier => candidate.Tier == pack.FilterByTier,
                InteractionFilterType.Family => candidate.Family == pack.FilterByFamily,
                _ => true
            };
        }

        private CardDefinition WeightedPick(List<WeightedCardEntry> pool)
        {
            float totalWeight = pool.Sum(e => e.Weight);
            float roll = (float)_random.NextDouble() * totalWeight;
            float cumulative = 0f;
            
            foreach (var entry in pool)
            {
                cumulative += entry.Weight;
                if (roll <= cumulative) return entry.Card;
            }
            
            return pool[^1].Card;
        }
    }
}