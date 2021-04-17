using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> SkipNull<TSource>(this IEnumerable<TSource?> source)
            where TSource : class
        {
            return source.Where(element => element != null)
                         .Select(element => element!);
        }

        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
                action(item);
        }

        public static IEnumerable<TSource> With<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
                yield return item;
            }
        }

        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source) => !source.Any();

        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static string JoinString<T>(this IEnumerable<T> self, string separator)
        {
            return String.Join(separator, self);
        }
    }
}
