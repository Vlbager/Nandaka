using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nandaka.DeviceSourceGenerator.Model;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class SourceCodeBuilder
    {
        private readonly ISourceBuildStrategy _strategy;
        private readonly DefinedTypesProvider _typesProvider;

        private SourceCodeBuilder(ISourceBuildStrategy strategy, DefinedTypesProvider typesProvider)
        {
            _strategy = strategy;
            _typesProvider = typesProvider;
        }
        
        public static string BuildSourceForForeignDevice(DeviceMeta meta, DefinedTypesProvider typesProvider)
        {
            var buildStrategy = new ForeignDeviceSourceCodeBuildStrategy();
            return new SourceCodeBuilder(buildStrategy, typesProvider).Build(meta);
        }

        public static string BuildSourceForLocalDevice(DeviceMeta meta, DefinedTypesProvider typesProvider)
        {
            var buildStrategy = new LocalDeviceSourceCodeBuildStrategy();
            return new SourceCodeBuilder(buildStrategy, typesProvider).Build(meta);
        }

        private string Build(DeviceMeta meta)
        {
            IReadOnlyList<RegisterProperty> registerProperties = meta.Table.GetAllRegisterProperties(_typesProvider);

            string propertyDeclarations = BuildPropertyDeclarations(registerProperties);

            string constructor = BuildConstructor(meta);

            string factoryMethod = BuildFactoryMethod(meta, registerProperties);

            return BuildResult(meta, propertyDeclarations, constructor, factoryMethod);
        }

        private string BuildPropertyDeclarations(IReadOnlyCollection<RegisterProperty> properties)
        {
            var stringBuilder = new StringBuilder();

            foreach (RegisterProperty registerProperty in properties)
            {
                string propertyType = _strategy.GetRegisterInterfaceType(registerProperty.RegisterType);
                string valueTypeName = registerProperty.ValueType.Name;
                string propertyName = registerProperty.Name;
                stringBuilder.AppendLine(
      $@"
public {propertyType}<{valueTypeName}> {propertyName} {{ get; }}");
            }

            return stringBuilder.ToString();
        }

        private string BuildConstructor(DeviceMeta meta)
        {
            string constructorInParams = _strategy.GetConstructorParameters()
                                                  .Select((typeName, varName) => $"{typeName} {varName}")
                                                  .JoinStrings(", ");

            string baseConstructorInParams = _strategy.GetConstructorParameters()
                                                      .Select((_, varName) => varName)
                                                      .JoinStrings(", ");


            return $@"
        private {meta.ClassName}({constructorInParams})
            : base({baseConstructorInParams}) {{ }}";
        }
        
        private string BuildFactoryMethod(DeviceMeta meta, IReadOnlyList<RegisterProperty> registerProperties)
        {
            string registersInitialization = BuildRegistersInitialization(registerProperties);

            string registerTableCreation = BuildRegisterTableCreationCode(registerProperties);

            string inParams = _strategy.GetFactoryMethodParameters()
                                       .Select((typeName, varName) => $"{typeName} {varName}")
                                       .JoinStrings(", ");

            string constructorExternalArgs = _strategy.GetFactoryMethodParameters()
                                              .Select((_, varName) => varName)
                                              .JoinStrings(", ");

            return $@"
        public static {meta.ClassName} Create({inParams})
        {{
            {registersInitialization}
            {registerTableCreation}
            return new {meta.ClassName}(table, {constructorExternalArgs})
        }}";
        }
        
        private string BuildRegistersInitialization(IReadOnlyCollection<RegisterProperty> registerProperties)
        {
            var result = new StringBuilder();

            result.AppendLine($@"
            var factory = new {_strategy.GetRegisterFactoryName()}();");

            foreach (RegisterProperty registerProperty in registerProperties)
            {
                string propertyName = registerProperty.Name;
                string factoryMethodName = _strategy.GetRegisterFactoryMethodName(registerProperty.RegisterType);
                string valueTypeName = registerProperty.ValueType.Name;
                string registerAddress = registerProperty.Address.ToString();

                result.Append($@"
            var {propertyName} = factory.{factoryMethodName}<{valueTypeName}>({registerAddress});");
            }

            string registersInitialization = result.ToString();
            return registersInitialization;
        }

        private static string BuildRegisterTableCreationCode(IReadOnlyList<RegisterProperty> registerProperties)
        {
            string createTableArgs = registerProperties.Select(prop => prop.Name)
                                                       .JoinStrings("," + Environment.NewLine +
                                                                    "                                                                          ");
            string registerTableCreation = $@"
            var table = RegisterTable.CreateWithValidation(new string[] {{ {createTableArgs} }});";
            return registerTableCreation;
        }

        private static string BuildResult(DeviceMeta meta, string propertyDeclarations, string constructor, string factoryMethod)
        {
            return
$@"

namespace {meta.Namespace}
{{
    {meta.Accessibility} partial {meta.TypeKind} {meta.ClassName}
    {{
{propertyDeclarations}
{constructor}
{factoryMethod}
    }}
}}";
        }
    }
}