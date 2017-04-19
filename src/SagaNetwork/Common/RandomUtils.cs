using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SagaNetwork
{
    /// <summary>
    /// Thread-safe static random numbers generator.
    /// </summary>
    public static class StaticRandom
    {
        private static int seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next (int min = int.MinValue, int max = int.MaxValue) => random.Value.Next(min, max);
        public static bool Bool => random.Value.NextDouble() >= 0.5;
        public static bool RollTheDice (float probability) => Next(0, 101) <= probability * 100;

        public static T Random<T> (this IEnumerable<T> enumerable)
        {
            if (enumerable == null || !enumerable.Any()) return default(T);

            var index = random.Value.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static T WeightedRandom<T> (this IEnumerable<T> enumerable) where T: IWeighted
        {
            if (enumerable == null || !enumerable.Any()) return default(T);

            var totalWeight = enumerable.Sum(c => c.ProbabilityWeight);
            var choice = random.Value.Next(totalWeight);
            int sum = 0;

            foreach (var element in enumerable)
            {
                for (int i = sum; i < element.ProbabilityWeight + sum; i++)
                    if (i >= choice) return element;
                sum += element.ProbabilityWeight;
            }

            return enumerable.First();
        }
    }

    /// <summary>
    /// The object has a probability weight property exposed.
    /// </summary>
    public interface IWeighted
    {
        /// <summary>
        /// Describes the likeness probability checks will pick the object. 
        /// </summary>
        int ProbabilityWeight { get; set; }
    }
}
