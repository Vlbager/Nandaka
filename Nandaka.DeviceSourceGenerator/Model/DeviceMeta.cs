using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Nandaka.DeviceSourceGenerator.Model
{
    internal sealed class DeviceMeta : IEquatable<DeviceMeta>
    {
        public Location DeviceClassLocation { get; }
        public TableMeta Table { get; }
        
        public ITypeSymbol DeviceSemanticModel { get; }

        public DeviceMeta(ITypeSymbol deviceSemanticModel, Location deviceClassLocation, TableMeta table)
        {
            DeviceSemanticModel = deviceSemanticModel;
            DeviceClassLocation = deviceClassLocation;
            Table = table;
        }
        
        public string ClassName => DeviceSemanticModel.Name;

        public string Namespace => DeviceSemanticModel.ContainingNamespace.ToString();

        public string Accessibility => DeviceSemanticModel.DeclaredAccessibility.ToString().ToLower();

        public string TypeKind => DeviceSemanticModel.TypeKind.ToString().ToLower();


        public bool InheritedFromInterface(ITypeSymbol interfaceSymbol)
        {
            return DeviceSemanticModel.AllInterfaces
                                       .Any(symbol => SymbolEqualityComparer.Default.Equals(interfaceSymbol, symbol));
        }

        public override int GetHashCode()
        {
            return SymbolEqualityComparer.Default.GetHashCode(DeviceSemanticModel);
        }

        public bool Equals(DeviceMeta other)
        {
            return SymbolEqualityComparer.Default.Equals(DeviceSemanticModel, other.DeviceSemanticModel);
        }
    }
}