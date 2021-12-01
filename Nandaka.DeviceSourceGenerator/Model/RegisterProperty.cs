using Microsoft.CodeAnalysis;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator.Model
{
    internal sealed class RegisterProperty
    {
        public RegisterType RegisterType { get; }
        public int Address { get; }
        public string Name { get; }
        public ITypeSymbol ValueType { get; }

        public RegisterProperty(RegisterType registerType, int address, string name, ITypeSymbol valueType)
        {
            RegisterType = registerType;
            Address = address;
            Name = name;
            ValueType = valueType;
        }
    }
}