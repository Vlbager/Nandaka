using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Tests.Common
{
    public static class ReadOnlyCollectionExtensions
    {
        public static IEnumerable<T> GetCircular<T>(this IReadOnlyCollection<T> source, int count)
        {
            if (source.IsEmpty())
                yield break;

            while (true)
            {
                foreach (T item in source)
                {
                    if (count-- == 0)
                        yield break;
                    
                    yield return item;
                }
            }
        }
    }
}