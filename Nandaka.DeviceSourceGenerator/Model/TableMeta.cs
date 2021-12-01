using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator.Model
{
    internal sealed class TableMeta
    {
        private readonly ITypeSymbol _tableSemanticModel;

        public TableMeta(ITypeSymbol tableSemanticModel)
        {
            _tableSemanticModel = tableSemanticModel;
        }

        public IReadOnlyList<RegisterProperty> GetAllRegisterProperties(DefinedTypesProvider typesProvider)
        {
            return _tableSemanticModel.GetMembers()
                                      .OfType<IFieldSymbol>()
                                      .Select(memberSymbol => TryGetRegisterProperty(memberSymbol, typesProvider))
                                      .OfType<RegisterProperty>()
                                      .ToArray();
        }

        private static RegisterProperty? TryGetRegisterProperty(IFieldSymbol memberSymbol, DefinedTypesProvider typesProvider)
        {
            ImmutableArray<AttributeData> attributes = memberSymbol.GetAttributes();

            AttributeData? readRequestAttributeData = attributes.FindOfType(typesProvider.ReadRequestRegisterAttribute);
            if (readRequestAttributeData != null)
                return Create(RegisterType.ReadRequest, memberSymbol, readRequestAttributeData);

            AttributeData? writeRequestAttributeData = attributes.FindOfType(typesProvider.WriteRequestRegisterAttribute);
            if (writeRequestAttributeData != null)
                return Create(RegisterType.WriteRequest, memberSymbol, writeRequestAttributeData);

            return null;
        }

        private static RegisterProperty Create(RegisterType type, IFieldSymbol symbol, AttributeData attributeData)
        {
            var registerAddress = (int)attributeData.ConstructorArguments
                                                    .First()
                                                    .Value!;

            return new RegisterProperty(type, registerAddress, symbol.Name, symbol.Type);
        }
    }
}