using System.Collections.Generic;

namespace Nandaka.Tests.Util
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SkipEvery<T>(this IEnumerable<T> source, int everyCount)
        {
            var counter = 1;
            var divider = everyCount + 1;

            foreach (T item in source)
            {
                if (counter++ % divider != 0)
                    yield return item;
            }
        }
    }
}