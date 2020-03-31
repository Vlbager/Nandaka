using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> IsNullAssert<TSource>(this IEnumerable<TSource> source) where TSource : class
        {
            foreach (TSource element in source)
            {
                if (element == null)
                    throw new NullReferenceException();

                yield return element;
            }
        }

        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }
    }
}
