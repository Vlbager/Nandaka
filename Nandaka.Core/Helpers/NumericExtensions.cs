using System.Runtime.CompilerServices;

namespace Nandaka.Core.Helpers
{
    public static class NumericExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ToFirstNibble(this byte self)
        {
            return (byte)(self << 4);
        }
    }
}