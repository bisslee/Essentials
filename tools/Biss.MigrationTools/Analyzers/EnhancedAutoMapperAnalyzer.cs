using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

namespace Biss.MigrationTools.Analyzers;

/// <summary>
/// Enhanced analyzer for AutoMapper usage with improved type inference.
/// </summary>
public class EnhancedAutoMapperAnalyzer
{
    private readonly List<MappingProfile> _profiles = new();
    private readonly List<MappingUsage> _usages = new();

    public IReadOnlyList<MappingProfile> Profiles => _profiles.AsReadOnly();
    public IReadOnlyList<MappingUsage> Usages => _usages.AsReadOnly();

    /// <summary>
    /// Analyzes a C# file for AutoMapper usage with improved type extraction.
    /// </summary>
    public async Task<AnalysisResult> AnalyzeFileAsync(string filePath, string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = await syntaxTree.GetRootAsync();
        
        var walker = new EnhancedAutoMapperSyntaxWalker();
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
/// Enhanced syntax walker that extracts AutoMapper CreateMap calls with type information.
/// </summary>
internal class EnhancedAutoMapperSyntaxWalker : CSharpSyntaxWalker
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
        // Look for CreateMap<> generic calls
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var memberName = memberAccess.Name.ToString();
            
            // Check for CreateMap<T1, T2>()
            if (memberName == "CreateMap" && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var parent = memberAccess.Parent as InvocationExpressionSyntax;
                if (parent != null)
                {
                    var mapping = ExtractCreateMapTypes(parent);
                    if (mapping != null)
                    {
                        var profile = Profiles.LastOrDefault();
                        if (profile != null)
                        {
                            profile.Mappings.Add(mapping);
                        }
                    }
                }
            }
        }
        
        // Look for Map() usage calls
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

        return profile;
    }

    private MappingDefinition? ExtractCreateMapTypes(InvocationExpressionSyntax node)
    {
        // Look for CreateMap<TSource, TDestination>() pattern
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var parentType = memberAccess.Parent?.Parent as InvocationExpressionSyntax;
            
            // Check if this is a generic method call
            if (node.Expression is MemberAccessExpressionSyntax accessExpression)
            {
                // Try to extract type arguments from GenericNameSyntax
                if (accessExpression.Name is GenericNameSyntax genericName)
                {
                    var typeArguments = genericName.TypeArgumentList.Arguments;
                    if (typeArguments.Count >= 2)
                    {
                        var sourceType = typeArguments[0].ToString();
                        var destinationType = typeArguments[1].ToString();
                        
                        return new MappingDefinition
                        {
                            SourceType = sourceType,
                            DestinationType = destinationType,
                            LineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                        };
                    }
                }
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
