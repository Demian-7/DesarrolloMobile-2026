using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core
{
    public class PackOpeningService
    {
        private readonly Random _random = new();

        public List<CardDefinition> Draw(CardPackDefinition pack)
        {
            var results = new List<CardDefinition>();
            for (int i = 0; i < pack.CardCount; i++)
                results.Add(WeightedPick(pack.Pool));

            var guaranteedPool = pack.Pool.Where(e => e.Card.Tier >= pack.MinimumGuaranteedTier).ToList();
            int shortfall = pack.MinimumGuaranteedCount - results.Count(c => c.Tier >= pack.MinimumGuaranteedTier);
            if (shortfall > 0 && guaranteedPool.Count > 0)
            {
                foreach (var card in results.OrderBy(c => c.Tier).Take(shortfall).ToList())
                {
                    results.Remove(card);
                    results.Add(WeightedPick(guaranteedPool));
                }
            }

            return results;
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