using System;
using System.Collections.Generic;

namespace LuckyQuest.Systems
{
    public static class WeightedRandom
    {
        public static T Pick<T>(IReadOnlyList<T> items, Func<T, int> weightSelector, Random rng)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items collection is null or empty.");

            if (weightSelector == null)
                throw new ArgumentNullException(nameof(weightSelector));

            if (rng == null)
                throw new ArgumentNullException(nameof(rng));

            int totalWeight = 0;
            for (int i = 0; i < items.Count; i++)
            {
                int weight = weightSelector(items[i]);
                if (weight < 0)
                    throw new InvalidOperationException("Weight cannot be negative.");

                totalWeight += weight;
            }

            if (totalWeight <= 0)
                throw new InvalidOperationException("Total weight must be greater than zero.");

            int roll = rng.Next(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < items.Count; i++)
            {
                cumulative += weightSelector(items[i]);
                if (roll < cumulative)
                    return items[i];
            }

            return items[^1];
        }
    }
}
