using Microsoft.CodeAnalysis;
using Nandaka.Model.Attributes;
using Nandaka.Model.Device;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class DefinedTypesProvider
    {
        public ITypeSymbol GenerateDeviceAttributeSemanticModel { get; }
        public ITypeSymbol INandakaDeviceSemanticModel { get; }
        public ITypeSymbol IForeignDeviceSemanticModel { get; }
        public ITypeSymbol ReadRequestRegisterAttribute { get; }
        public ITypeSymbol WriteRequestRegisterAttribute { get; }

        private DefinedTypesProvider(ITypeSymbol generateDeviceAttributeSemanticModel,
                                     ITypeSymbol nandakaDeviceSemanticModel,
                                     ITypeSymbol foreignDeviceSemanticModel,
                                     ITypeSymbol readRequestRegisterAttribute,
                                     ITypeSymbol writeRequestRegisterAttribute)
        {
            GenerateDeviceAttributeSemanticModel = generateDeviceAttributeSemanticModel;
            INandakaDeviceSemanticModel = nandakaDeviceSemanticModel;
            IForeignDeviceSemanticModel = foreignDeviceSemanticModel;
            ReadRequestRegisterAttribute = readRequestRegisterAttribute;
            WriteRequestRegisterAttribute = writeRequestRegisterAttribute;
        }

        public static DefinedTypesProvider? TryCreate(Compilation compilation)
        {
            ITypeSymbol? generateDeviceAttribute = compilation.GetTypeByMetadataName(typeof(GenerateDeviceAttribute).FullName);
            ITypeSymbol? foreignDeviceInterface = compilation.GetTypeByMetadataName(typeof(IForeignDevice).FullName);
            ITypeSymbol? nandakaDeviceInterface = compilation.GetTypeByMetadataName(typeof(INandakaDevice).FullName);
            ITypeSymbol? readRequestRegisterAttribute = compilation.GetTypeByMetadataName(typeof(ReadRequestRegisterAttribute).FullName);
            ITypeSymbol? writeRequestRegisterAttribute = compilation.GetTypeByMetadataName(typeof(WriteRequestRegisterAttribute).FullName);
            
            if (generateDeviceAttribute is null || 
                foreignDeviceInterface is null || 
                nandakaDeviceInterface is null ||
                readRequestRegisterAttribute is null ||
                writeRequestRegisterAttribute is null)
            {
                return null;
            }

            return new DefinedTypesProvider(generateDeviceAttribute, 
                                            nandakaDeviceInterface, 
                                            foreignDeviceInterface, 
                                            readRequestRegisterAttribute,
                                            writeRequestRegisterAttribute);
        }
    }
}