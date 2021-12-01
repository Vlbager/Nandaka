using System;
using System.Collections.Generic;

namespace Nandaka.DeviceSourceGenerator
{
    internal static class EnumerableExtensions
    {
        public static string JoinStrings<T>(this IEnumerable<T> source, string separator)
        {
            return String.Join(separator, source);
        }
    }
}