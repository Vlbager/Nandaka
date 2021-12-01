using System;
using System.Collections.Generic;
using Nandaka.Model.Device;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class ForeignDeviceSourceCodeBuildStrategy : ISourceBuildStrategy
    {
        public string GetRegisterInterfaceType(RegisterType propertyType)
        {
            return propertyType switch
            {
                RegisterType.ReadRequest => nameof(IReadOnlyRegister<byte>),
                RegisterType.WriteRequest => nameof(IRegister),
                _ => throw new ArgumentException(nameof(propertyType))
            };
        }

        public string GetRegisterFactoryName()
        {
            return "ForeignDeviceRegisterFactory";
        }

        public string GetRegisterFactoryMethodName(RegisterType propertyType)
        {
            return propertyType switch
            {
                RegisterType.ReadRequest => nameof(IRegisterFactory.CreateReadOnly),
                RegisterType.WriteRequest => nameof(IRegisterFactory.Create),
                _ => throw new ArgumentException(nameof(propertyType))
            };
        }

        public IReadOnlyCollection<(string typeName, string varName)> GetConstructorParameters()
        {
            return new[]
            {
                ("RegisterTable", "table"),
                ("int", "address"),
                (nameof(DeviceState), "state")
            };
        }

        public IReadOnlyCollection<(string typeName, string varName)> GetFactoryMethodParameters()
        {
            return new[]
            {
                ("int", "address"),
                (nameof(DeviceState), "state")
            };
        }
    }
}