using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> IsNullAssert<TSource>(this IEnumerable<TSource> source) 
            where TSource : class
        {
            foreach (TSource element in source)
            {
                if (element == null)
                    throw new NullReferenceException();

                yield return element;
            }
        }

        public static IEnumerable<TSource> SkipNull<TSource>(this IEnumerable<TSource?> source)
            where TSource : class
        {
            return source.Where(element => element != null)
                         .Select(element => element!);
        }

        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source) => !source.Any();

        public static IEnumerable<TResult> SafeCast<TSource, TResult>(this IEnumerable<TSource> source)
            where TResult : class
        {
            return source.Select(element => element as TResult)
                .SkipNull();
        }
        

        public static byte[] RevertBytes(this IEnumerable<byte> source)
        { 
            return source.OrderByDescending(b => b).ToArray();
        } 
        
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static string Join(this IEnumerable<string> self, string separator)
        {
            return String.Join(separator, self);
        }
    }
}
