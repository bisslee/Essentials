using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Biss.Mapper.SourceGenerators;

/// <summary>
/// Source generator that creates optimized object mapping code.
/// </summary>
[Generator]
public class MapperSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new MapperSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not MapperSyntaxReceiver receiver)
            return;

        var compilation = context.Compilation;
        var mapperAttribute = compilation.GetTypeByMetadataName("Biss.Mapper.Abstractions.MapperAttribute");

        if (mapperAttribute == null)
            return;

        foreach (var classDeclaration in receiver.CandidateClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null) continue;

            var mapperAttr = classSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Equals(mapperAttribute, SymbolEqualityComparer.Default) == true);

            if (mapperAttr == null) continue;

            var methods = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(SyntaxKind.PartialKeyword))
                .ToList();

            if (methods.Any())
            {
                var source = GenerateMapperImplementation(classSymbol, methods, semanticModel);
                var fileName = $"{classSymbol.Name}.g.cs";
                context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    private static string GenerateMapperImplementation(INamedTypeSymbol classSymbol, 
        List<MethodDeclarationSyntax> methods, SemanticModel semanticModel)
    {
        var sb = new StringBuilder();
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated mapper implementation.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial class {classSymbol.Name}");
        sb.AppendLine("{");

        foreach (var method in methods)
        {
            GenerateMethodImplementation(sb, method, semanticModel);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateMethodImplementation(StringBuilder sb, MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
        if (methodSymbol == null) return;

        var returnType = methodSymbol.ReturnType.ToDisplayString();
        var parameters = methodSymbol.Parameters;
        
        if (parameters.Length != 1) return;

        var sourceType = parameters[0].Type.ToDisplayString();
        var sourceParamName = parameters[0].Name;

        sb.AppendLine($"    public partial {returnType} {method.Identifier.ValueText}({sourceType} {sourceParamName})");
        sb.AppendLine("    {");
        sb.AppendLine($"        if ({sourceParamName} == null)");
        sb.AppendLine($"            throw new ArgumentNullException(nameof({sourceParamName}));");
        sb.AppendLine();
        sb.AppendLine($"        return new {returnType}");
        sb.AppendLine("        {");

        // Generate property mappings
        GeneratePropertyMappings(sb, sourceType, returnType, sourceParamName, semanticModel);

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GeneratePropertyMappings(StringBuilder sb, string sourceType, string returnType, 
        string sourceParamName, SemanticModel semanticModel)
    {
        // This is a simplified implementation
        // In a real implementation, you would analyze the types and generate appropriate mappings
        
        sb.AppendLine("            // Auto-generated property mappings");
        sb.AppendLine("            // TODO: Implement actual property mapping logic");
        sb.AppendLine("            // This would analyze source and target types to generate mappings");
    }
}

/// <summary>
/// Syntax receiver that finds candidate mapper classes.
/// </summary>
public class MapperSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration)
        {
            CandidateClasses.Add(classDeclaration);
        }
    }
}
