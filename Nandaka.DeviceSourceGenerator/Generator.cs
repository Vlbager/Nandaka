using System.Collections.Generic;
using Microsoft.CodeAnalysis;
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

            var extractor = new GeneratorCandidatesExtractor(context, syntaxReceiver, typeProvider);
            IReadOnlyCollection<DeviceMeta> generateCandidates = extractor.GetGenerateCandidates();

            foreach (DeviceMeta generateCandidate in generateCandidates)
            {
                if (generateCandidate.IsInheritedFromInterface(typeProvider.ForeignDeviceSemanticModel))
                {
                    string source = SourceCodeBuilder.BuildSourceForForeignDevice(generateCandidate);
                    context.AddSource($"{generateCandidate.ClassName}.Generated.cs", source);
                    continue;
                }

                if (generateCandidate.IsInheritedFromInterface(typeProvider.NandakaDeviceSemanticModel))
                {
                    string source = SourceCodeBuilder.BuildSourceForLocalDevice(generateCandidate);
                    context.AddSource($"{generateCandidate.ClassName}.Generated.cs", source);
                    continue;
                }
                
                context.ReportWrongTypeWasMarkedWithGenerateAttributeCriteria(generateCandidate.ClassName, 
                                                                              generateCandidate.DeviceClassLocation);
            }
        }
    }
}