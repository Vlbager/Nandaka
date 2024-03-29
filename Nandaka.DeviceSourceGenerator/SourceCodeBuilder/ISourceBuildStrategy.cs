﻿using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator
{
    internal interface ISourceBuildStrategy
    {
        public string GetRegisterInterfaceType(RegisterType propertyType);
        public string GetRegisterFactoryName();
        public string GetRegisterFactoryMethodName(RegisterType propertyType);
    }
}