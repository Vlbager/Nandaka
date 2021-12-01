using Microsoft.CodeAnalysis;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator.Model
{
    internal sealed class RegisterProperty
    {
        private readonly ITypeSymbol _valueType;
        
        public RegisterType RegisterType { get; }
        public int Address { get; }
        public string Name { get; }

        public string ValueTypeName => _valueType.ToDisplayString();

        public RegisterProperty(RegisterType registerType, int address, string name, ITypeSymbol valueType)
        {
            RegisterType = registerType;
            Address = address;
            Name = name;
            _valueType = valueType;
        }
    }
}