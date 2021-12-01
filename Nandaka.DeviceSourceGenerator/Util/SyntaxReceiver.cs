using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nandaka.DeviceSourceGenerator
{
    public sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public HashSet<TypeDeclarationSyntax> TypeSyntaxNodesWithAttribute { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not TypeDeclarationSyntax typeSyntaxNode)
                return;

            if (typeSyntaxNode.AttributeLists.Count > 0)
                TypeSyntaxNodesWithAttribute.Add(typeSyntaxNode);
        }
    }
}