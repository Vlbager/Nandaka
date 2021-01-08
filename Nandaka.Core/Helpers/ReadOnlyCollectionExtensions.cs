using System.Collections.Generic;

namespace Nandaka.Core.Helpers
{
    public static class ReadOnlyCollectionExtensions
    {
        public static bool IsEmpty<T>(this IReadOnlyCollection<T> self)
        {
            return self.Count == 0;
        }
    }
}