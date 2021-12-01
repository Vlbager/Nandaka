using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Nandaka.DeviceSourceGenerator
{
    internal static class ArrayOfAttributesExtensions
    {
        public static AttributeData? FindOfType(this ImmutableArray<AttributeData> source, ISymbol attributeType)
        {
            return source.FirstOrDefault(attrData => SymbolEqualityComparer.Default.Equals(attrData.AttributeClass, attributeType));
        }
    }
}