using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Nandaka.DeviceSourceGenerator.Model
{
    internal sealed class DeviceMeta : IEquatable<DeviceMeta>
    {
        private readonly ITypeSymbol _deviceSemanticModel;
        
        public Location DeviceClassLocation { get; }
        public TableMeta Table { get; }

        public DeviceMeta(ITypeSymbol deviceSemanticModel, Location deviceClassLocation, TableMeta table)
        {
            _deviceSemanticModel = deviceSemanticModel;
            DeviceClassLocation = deviceClassLocation;
            Table = table;
        }
        
        public string ClassName => _deviceSemanticModel.Name;

        public string Namespace => _deviceSemanticModel.ContainingNamespace.ToString();

        public string Accessibility => _deviceSemanticModel.DeclaredAccessibility.ToString().ToLower();

        public string TypeKind => _deviceSemanticModel.TypeKind.ToString().ToLower();


        public bool IsInheritedFromInterface(ITypeSymbol interfaceSymbol)
        {
            return _deviceSemanticModel.AllInterfaces
                                       .Any(symbol => SymbolEqualityComparer.Default.Equals(interfaceSymbol, symbol));
        }

        public override int GetHashCode()
        {
            return SymbolEqualityComparer.Default.GetHashCode(_deviceSemanticModel);
        }

        public bool Equals(DeviceMeta other)
        {
            return SymbolEqualityComparer.Default.Equals(_deviceSemanticModel, other._deviceSemanticModel);
        }
    }
}