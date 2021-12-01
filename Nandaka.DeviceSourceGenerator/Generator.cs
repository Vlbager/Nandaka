using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nandaka.DeviceSourceGenerator.Model;

namespace Nandaka.DeviceSourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
                return;

            Compilation compilation = context.Compilation;

            var typeProvider = DefinedTypesProvider.TryCreate(compilation);

            if (typeProvider == null)
            {
                context.ReportInvalidMetadataDiagnostic();
                return;
            }

            IReadOnlyCollection<DeviceMeta> generateCandidates = GetGenerateCandidates(context,
                syntaxReceiver,
                compilation,
                typeProvider.GenerateDeviceAttributeSemanticModel);

            foreach (DeviceMeta generateCandidate in generateCandidates)
            {
                if (generateCandidate.InheritedFromInterface(typeProvider.ForeignDeviceSemanticModel))
                {
                    string source = SourceCodeBuilder.BuildSourceForForeignDevice(generateCandidate, typeProvider);
                    context.AddSource($"{generateCandidate.ClassName}.Generated.cs", source);
                    continue;
                }

                if (generateCandidate.InheritedFromInterface(typeProvider.NandakaDeviceSemanticModel))
                {
                    string source = SourceCodeBuilder.BuildSourceForLocalDevice(generateCandidate, typeProvider);
                    context.AddSource($"{generateCandidate.ClassName}.Generated.cs", source);
                    continue;
                }
                
                context.ReportWrongTypeWasMarkedWithGenerateAttributeCriteria(generateCandidate.ClassName, 
                                                                              generateCandidate.DeviceClassLocation);
            }
        }
        
        private static IReadOnlyCollection<DeviceMeta> GetGenerateCandidates(GeneratorExecutionContext context, 
                                                                                             SyntaxReceiver syntaxReceiver, 
                                                                                             Compilation compilation, 
                                                                                             ISymbol generateDeviceAttr)
        {
            var deviceDiscoveredMetas = new HashSet<DeviceMeta>();

            foreach (TypeDeclarationSyntax typeSyntaxNode in syntaxReceiver.TypeSyntaxNodesWithAttribute)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = compilation.GetSemanticModel(typeSyntaxNode.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(typeSyntaxNode) is not ITypeSymbol typeSemanticModel)
                    continue;

                TableMeta? tableMeta = TryGetDeviceTableMeta(context, generateDeviceAttr, typeSemanticModel);
                if (tableMeta == null)
                    continue;

                var discoveredMeta = new DeviceMeta(typeSemanticModel, typeSyntaxNode.GetLocation(), tableMeta);

                deviceDiscoveredMetas.Add(discoveredMeta);
            }

            return deviceDiscoveredMetas;
        }

        private static TableMeta? TryGetDeviceTableMeta(GeneratorExecutionContext context, ISymbol generateDeviceAttr,
                                                        ITypeSymbol typeSemanticModel)
        {
            AttributeData? deviceAttr = typeSemanticModel.GetAttributes()
                                                         .FindOfType(generateDeviceAttr);

            if (deviceAttr == null)
                return null;

            return TryGetDeviceTableMetaFromAttributeValue(context, deviceAttr);
        }

        private static TableMeta? TryGetDeviceTableMetaFromAttributeValue(GeneratorExecutionContext context, AttributeData deviceAttr)
        {
            var tableSemanticModel = deviceAttr.ConstructorArguments
                                               .FirstOrDefault(arg => arg.Kind == TypedConstantKind.Type)
                                               .Value as ITypeSymbol;

            if (tableSemanticModel != null) 
                return new TableMeta(tableSemanticModel);
            
            context.ReportInvalidDeviceTableType();
            return null;
        }
    }
}