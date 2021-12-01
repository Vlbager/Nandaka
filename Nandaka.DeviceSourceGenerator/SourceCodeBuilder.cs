using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nandaka.DeviceSourceGenerator.Model;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class SourceCodeBuilder
    {
        private readonly ISourceBuildStrategy _strategy;

        private SourceCodeBuilder(ISourceBuildStrategy strategy)
        {
            _strategy = strategy;
        }
        
        public static string BuildSourceForForeignDevice(DeviceMeta meta)
        {
            var buildStrategy = new ForeignDeviceSourceCodeBuildStrategy();
            return new SourceCodeBuilder(buildStrategy).Build(meta);
        }

        public static string BuildSourceForLocalDevice(DeviceMeta meta)
        {
            var buildStrategy = new LocalDeviceSourceCodeBuildStrategy();
            return new SourceCodeBuilder(buildStrategy).Build(meta);
        }

        private string Build(DeviceMeta meta)
        {
            string propertyDeclarations = BuildPropertyDeclarations(meta);

            string constructor = BuildConstructor(meta);

            return BuildResult(meta, propertyDeclarations, constructor);
        }

        private string BuildPropertyDeclarations(DeviceMeta meta)
        {
            var stringBuilder = new StringBuilder();

            foreach (RegisterProperty registerProperty in meta.Table.RegisterProperties)
            {
                string propertyType = _strategy.GetRegisterInterfaceType(registerProperty.RegisterType);
                string valueTypeName = registerProperty.ValueTypeName;
                string propertyName = registerProperty.Name;
                stringBuilder.AppendLine(
      $@"        public {propertyType}<{valueTypeName}> {propertyName} {{ get; }}");
            }

            stringBuilder.AppendLine();
            
            stringBuilder.AppendLine(
       @"        public override RegisterTable Table { get; }");
            
            stringBuilder.AppendLine(
       @"        public override int Address { get; }");
            
            stringBuilder.Append(
      $@"        public override string Name => nameof({meta.ClassName});");

            return stringBuilder.ToString();
        }

        private string BuildConstructor(DeviceMeta meta)
        {
            string registersInitialization = BuildRegistersInitialization(meta.Table.RegisterProperties);
            string tableInitialization = BuildTableInitialization(meta.Table.RegisterProperties);

            return $@"
        public {meta.ClassName}(int address)
        {{
            Address = address;            

            {registersInitialization}
            {tableInitialization}
        }}";
        }

        private string BuildRegistersInitialization(IReadOnlyCollection<RegisterProperty> registerProperties)
        {
            var result = new StringBuilder();

            result.AppendLine($@"var factory = new {_strategy.GetRegisterFactoryName()}();");

            foreach (RegisterProperty registerProperty in registerProperties)
            {
                string propertyName = registerProperty.Name;
                string factoryMethodName = _strategy.GetRegisterFactoryMethodName(registerProperty.RegisterType);
                string valueTypeName = registerProperty.ValueTypeName;
                string registerAddress = registerProperty.Address.ToString();

                result.Append($@"
            {propertyName} = factory.{factoryMethodName}<{valueTypeName}>({registerAddress});");
            }

            string registersInitialization = result.ToString();
            return registersInitialization;
        }

        private static string BuildTableInitialization(IReadOnlyList<RegisterProperty> registerProperties)
        {
            string createTableArgs = registerProperties.Select(prop => prop.Name)
                                                       .JoinStrings("," + Environment.NewLine +
                                                                    "                                                                         ");
            string registerTableCreation = $@"
            Table = RegisterTable.CreateWithValidation(new {nameof(IRegister)}[] {{ {createTableArgs} }});";
            return registerTableCreation;
        }

        private static string BuildResult(DeviceMeta meta, string propertyDeclarations, string constructor)
        {
            return
$@"using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace {meta.Namespace}
{{
    // This part is generated. Don't change manually.
    {meta.Accessibility} partial {meta.TypeKind} {meta.ClassName}
    {{
{propertyDeclarations}
{constructor}
    }}
}}";
        }
    }
}