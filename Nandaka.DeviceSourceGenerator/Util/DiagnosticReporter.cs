using Microsoft.CodeAnalysis;

namespace Nandaka.DeviceSourceGenerator
{
    internal static class DiagnosticReporter
    {
        private const string DiagnosticCategory = "Nandaka device code generator";
        
        public static void ReportInvalidMetadataDiagnostic(this GeneratorExecutionContext context)
        {
            ReportInternal(context: context,
                           id: "NAN01",
                           message: "[NAN] Invalid Nandaka.Model metadata structure. Ensure that correct assembly was referenced", 
                           severity: DiagnosticSeverity.Error);
        }
        
        public static void ReportWrongTypeWasMarkedWithGenerateAttributeCriteria(this GeneratorExecutionContext context, 
                                                                                 string deviceClassName, Location location)
        {
            ReportInternal(context: context,
                           id: "NAN02",
                           message: $"[NAN] Non-inheritor of NandakaDevice was marked with [GenerateDevice] attribute: {deviceClassName}", 
                           severity: DiagnosticSeverity.Warning,
                           location: location);
        }
        
        public static void ReportInvalidDeviceTableType(this GeneratorExecutionContext context)
        { 
            ReportInternal(context: context,
                           id: "NAN03",
                           message: "[NAN] GenerateDevice attribute doesn't contain any type of device table. " +
                                    "Ensure that attribute was initialized correctly", 
                           severity: DiagnosticSeverity.Error);
        }

        private static void ReportInternal(GeneratorExecutionContext context, string id, string message, 
                                           DiagnosticSeverity severity)
        {
            var diagnostic = Diagnostic.Create(id: id,
                                               category: DiagnosticCategory,
                                               message: message,
                                               severity: severity,
                                               defaultSeverity: severity,
                                               isEnabledByDefault: true,
                                               warningLevel: GetWarningLevel(severity));
            
            context.ReportDiagnostic(diagnostic);
        }

        private static void ReportInternal(GeneratorExecutionContext context, string id, string message, 
                                           DiagnosticSeverity severity, Location location)
        {
            var diagnostic = Diagnostic.Create(id: id,
                                               category: DiagnosticCategory,
                                               message: message,
                                               severity: severity,
                                               defaultSeverity: severity,
                                               isEnabledByDefault: true,
                                               warningLevel: GetWarningLevel(severity),
                                               location: location);
            
            context.ReportDiagnostic(diagnostic);
        }
        
        private static int GetWarningLevel(DiagnosticSeverity severity)
        {
            return severity == DiagnosticSeverity.Error ? 0 : 1;
        }
    }
}