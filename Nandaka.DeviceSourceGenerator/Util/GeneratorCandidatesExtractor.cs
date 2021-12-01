using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nandaka.DeviceSourceGenerator.Model;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class GeneratorCandidatesExtractor
    {
        private readonly GeneratorExecutionContext _context;
        private readonly SyntaxReceiver _syntaxReceiver;
        private readonly DefinedTypesProvider _typesProvider;

        public GeneratorCandidatesExtractor(GeneratorExecutionContext context, SyntaxReceiver syntaxReceiver, DefinedTypesProvider typesProvider)
        {
            _context = context;
            _syntaxReceiver = syntaxReceiver;
            _typesProvider = typesProvider;
        }

        public IReadOnlyCollection<DeviceMeta> GetGenerateCandidates()
        {
            var deviceDiscoveredMetas = new HashSet<DeviceMeta>();

            Compilation compilation = _context.Compilation;

            foreach (TypeDeclarationSyntax typeSyntaxNode in _syntaxReceiver.TypeSyntaxNodesWithAttribute)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = compilation.GetSemanticModel(typeSyntaxNode.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(typeSyntaxNode) is not ITypeSymbol typeSemanticModel)
                    continue;

                TableMeta? tableMeta = TryGetDeviceTableMeta(typeSemanticModel);
                if (tableMeta == null)
                    continue;

                var discoveredMeta = new DeviceMeta(typeSemanticModel, typeSyntaxNode.GetLocation(), tableMeta);

                deviceDiscoveredMetas.Add(discoveredMeta);
            }

            return deviceDiscoveredMetas;            
        }

        private TableMeta? TryGetDeviceTableMeta(ITypeSymbol typeSemanticModel)
        {
            AttributeData? deviceAttr = typeSemanticModel.GetAttributes()
                                                         .FindOfType(_typesProvider.GenerateDeviceAttributeSemanticModel);

            if (deviceAttr == null)
                return null;

            return TryGetDeviceTableMetaFromAttributeValue(deviceAttr);
        }

        private TableMeta? TryGetDeviceTableMetaFromAttributeValue(AttributeData deviceAttr)
        {
            var tableSemanticModel = deviceAttr.ConstructorArguments
                                               .FirstOrDefault(arg => arg.Kind == TypedConstantKind.Type)
                                               .Value as ITypeSymbol;

            if (tableSemanticModel != null) 
                return TableMeta.Create(tableSemanticModel, _typesProvider);
            
            _context.ReportInvalidDeviceTableType();
            return null;
        }
    }
}