using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

namespace Biss.MigrationTools.Analyzers;

/// <summary>
/// Analyzes AutoMapper usage in C# code.
/// </summary>
public class AutoMapperAnalyzer
{
    private readonly List<MappingProfile> _profiles = new();
    private readonly List<MappingUsage> _usages = new();

    public IReadOnlyList<MappingProfile> Profiles => _profiles.AsReadOnly();
    public IReadOnlyList<MappingUsage> Usages => _usages.AsReadOnly();

    /// <summary>
    /// Analyzes a C# file for AutoMapper usage.
    /// </summary>
    /// <param name="filePath">Path to the C# file.</param>
    /// <param name="sourceCode">Source code content.</param>
    /// <returns>Analysis results.</returns>
    public async Task<AnalysisResult> AnalyzeFileAsync(string filePath, string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = await syntaxTree.GetRootAsync();
        
        var walker = new AutoMapperSyntaxWalker();
        walker.Visit(root);
        
        _profiles.AddRange(walker.Profiles);
        _usages.AddRange(walker.Usages);
        
        return new AnalysisResult
        {
            FilePath = filePath,
            ProfilesFound = walker.Profiles.Count,
            UsagesFound = walker.Usages.Count,
            HasAutoMapperReferences = walker.HasAutoMapperReferences
        };
    }

    /// <summary>
    /// Analyzes multiple files in a directory.
    /// </summary>
    /// <param name="directoryPath">Directory path to analyze.</param>
    /// <param name="searchPattern">File search pattern.</param>
    /// <returns>Analysis results for all files.</returns>
    public async Task<List<AnalysisResult>> AnalyzeDirectoryAsync(string directoryPath, string searchPattern = "*.cs")
    {
        var results = new List<AnalysisResult>();
        var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            try
            {
                var sourceCode = await File.ReadAllTextAsync(file);
                var result = await AnalyzeFileAsync(file, sourceCode);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new AnalysisResult
                {
                    FilePath = file,
                    Error = ex.Message
                });
            }
        }
        
        return results;
    }

    /// <summary>
    /// Generates migration report.
    /// </summary>
    /// <returns>Migration report as JSON.</returns>
    public string GenerateMigrationReport()
    {
        var report = new MigrationReport
        {
            TotalProfiles = _profiles.Count,
            TotalUsages = _usages.Count,
            Profiles = _profiles.ToList(),
            Usages = _usages.ToList(),
            GeneratedAt = DateTime.UtcNow
        };
        
        return JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}

/// <summary>
/// Syntax walker that identifies AutoMapper usage patterns.
/// </summary>
internal class AutoMapperSyntaxWalker : CSharpSyntaxWalker
{
    public List<MappingProfile> Profiles { get; } = new();
    public List<MappingUsage> Usages { get; } = new();
    public bool HasAutoMapperReferences { get; private set; }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Name?.ToString().Contains("AutoMapper") == true)
        {
            HasAutoMapperReferences = true;
        }
        base.VisitUsingDirective(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // Look for Profile classes
        if (node.BaseList?.Types.Any(t => t.ToString().Contains("Profile")) == true)
        {
            var profile = ExtractMappingProfile(node);
            if (profile != null)
            {
                Profiles.Add(profile);
            }
        }
        
        base.VisitClassDeclaration(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Look for Map() calls
        if (node.Expression.ToString().Contains(".Map"))
        {
            var usage = ExtractMappingUsage(node);
            if (usage != null)
            {
                Usages.Add(usage);
            }
        }
        
        base.VisitInvocationExpression(node);
    }

    private MappingProfile? ExtractMappingProfile(ClassDeclarationSyntax classNode)
    {
        var profile = new MappingProfile
        {
            ClassName = classNode.Identifier.ValueText,
            Namespace = GetNamespace(classNode),
            Mappings = new List<MappingDefinition>()
        };

        // Extract CreateMap calls from constructor
        var constructor = classNode.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault();

        if (constructor != null)
        {
            var createMapCalls = ExtractCreateMapCalls(constructor);
            profile.Mappings.AddRange(createMapCalls);
        }

        return profile.Mappings.Any() ? profile : null;
    }

    private List<MappingDefinition> ExtractCreateMapCalls(ConstructorDeclarationSyntax constructor)
    {
        var mappings = new List<MappingDefinition>();
        
        var walker = new CreateMapWalker();
        walker.Visit(constructor);
        
        foreach (var call in walker.CreateMapCalls)
        {
            var mapping = ParseCreateMapCall(call);
            if (mapping != null)
            {
                mappings.Add(mapping);
            }
        }
        
        return mappings;
    }

    private MappingDefinition? ParseCreateMapCall(InvocationExpressionSyntax call)
    {
        if (call.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var arguments = call.ArgumentList.Arguments;
            if (arguments.Count >= 2)
            {
                return new MappingDefinition
                {
                    SourceType = arguments[0].ToString().Trim(),
                    DestinationType = arguments[1].ToString().Trim(),
                    LineNumber = call.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                };
            }
        }
        
        return null;
    }

    private MappingUsage? ExtractMappingUsage(InvocationExpressionSyntax node)
    {
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return new MappingUsage
            {
                MethodName = memberAccess.Name.Identifier.ValueText,
                Arguments = node.ArgumentList.Arguments.Select(a => a.ToString()).ToList(),
                LineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };
        }
        
        return null;
    }

    private string GetNamespace(SyntaxNode node)
    {
        var namespaceNode = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceNode?.Name.ToString() ?? string.Empty;
    }
}

/// <summary>
/// Walker that finds CreateMap calls.
/// </summary>
internal class CreateMapWalker : CSharpSyntaxWalker
{
    public List<InvocationExpressionSyntax> CreateMapCalls { get; } = new();

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression.ToString().Contains("CreateMap"))
        {
            CreateMapCalls.Add(node);
        }
        base.VisitInvocationExpression(node);
    }
}

/// <summary>
/// Represents a mapping profile found in the code.
/// </summary>
public class MappingProfile
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<MappingDefinition> Mappings { get; set; } = new();
}

/// <summary>
/// Represents a mapping definition.
/// </summary>
public class MappingDefinition
{
    public string SourceType { get; set; } = string.Empty;
    public string DestinationType { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

/// <summary>
/// Represents a mapping usage in the code.
/// </summary>
public class MappingUsage
{
    public string MethodName { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
    public int LineNumber { get; set; }
}

/// <summary>
/// Analysis result for a single file.
/// </summary>
public class AnalysisResult
{
    public string FilePath { get; set; } = string.Empty;
    public int ProfilesFound { get; set; }
    public int UsagesFound { get; set; }
    public bool HasAutoMapperReferences { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Complete migration report.
/// </summary>
public class MigrationReport
{
    public int TotalProfiles { get; set; }
    public int TotalUsages { get; set; }
    public List<MappingProfile> Profiles { get; set; } = new();
    public List<MappingUsage> Usages { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
