using System;

namespace Nandaka.Core.Helpers
{
    public static class RangeExtensions
    {
        public static bool IfIncludes(this Range range, int value)
        {
            return range.Start.Value <= value &&
                   range.End.Value >= value;
        }
    }
}