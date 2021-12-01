using System.Linq;
using Microsoft.CodeAnalysis;
using Nandaka.Model.Attributes;
using Nandaka.Model.Device;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class DefinedTypesProvider
    {
        public ITypeSymbol GenerateDeviceAttributeSemanticModel { get; }
        public ITypeSymbol NandakaDeviceSemanticModel { get; }
        public ITypeSymbol ForeignDeviceSemanticModel { get; }
        public ITypeSymbol ReadRequestRegisterAttribute { get; }
        public ITypeSymbol WriteRequestRegisterAttribute { get; }

        private DefinedTypesProvider(ITypeSymbol generateDeviceAttributeSemanticModel,
                                     ITypeSymbol nandakaDeviceSemanticModel,
                                     ITypeSymbol foreignDeviceSemanticModel,
                                     ITypeSymbol readRequestRegisterAttribute,
                                     ITypeSymbol writeRequestRegisterAttribute)
        {
            GenerateDeviceAttributeSemanticModel = generateDeviceAttributeSemanticModel;
            NandakaDeviceSemanticModel = nandakaDeviceSemanticModel;
            ForeignDeviceSemanticModel = foreignDeviceSemanticModel;
            ReadRequestRegisterAttribute = readRequestRegisterAttribute;
            WriteRequestRegisterAttribute = writeRequestRegisterAttribute;
        }

        public static DefinedTypesProvider? TryCreate(Compilation compilation)
        {
            string asmName = typeof(GenerateDeviceAttribute).Assembly
                                                            .GetName().Name;
            
            IAssemblySymbol? modelAsmSymbol = compilation.SourceModule
                                                         .ReferencedAssemblySymbols
                                                         .FirstOrDefault(asmSymbol => asmSymbol.Name == asmName);

            if (modelAsmSymbol == null)
                return null;

            ITypeSymbol? generateDeviceAttribute = modelAsmSymbol.GetTypeByMetadataName(typeof(GenerateDeviceAttribute).FullName);
            ITypeSymbol? foreignDeviceInterface = modelAsmSymbol.GetTypeByMetadataName(typeof(IForeignDevice).FullName);
            ITypeSymbol? nandakaDeviceInterface = modelAsmSymbol.GetTypeByMetadataName(typeof(INandakaDevice).FullName);
            ITypeSymbol? readRequestRegisterAttribute = modelAsmSymbol.GetTypeByMetadataName(typeof(ReadRequestRegisterAttribute).FullName);
            ITypeSymbol? writeRequestRegisterAttribute = modelAsmSymbol.GetTypeByMetadataName(typeof(WriteRequestRegisterAttribute).FullName);
            
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