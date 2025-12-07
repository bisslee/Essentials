using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Biss.MigrationTools.Analyzers;

/// <summary>
/// Analyzes MediatR usage in C# code.
/// </summary>
public class MediatRAnalyzer
{
    private readonly List<RequestHandler> _handlers = new();
    private readonly List<NotificationHandler> _notificationHandlers = new();
    private readonly List<MediatRUsage> _usages = new();

    public IReadOnlyList<RequestHandler> Handlers => _handlers.AsReadOnly();
    public IReadOnlyList<NotificationHandler> NotificationHandlers => _notificationHandlers.AsReadOnly();
    public IReadOnlyList<MediatRUsage> Usages => _usages.AsReadOnly();

    /// <summary>
    /// Analyzes a C# file for MediatR usage.
    /// </summary>
    /// <param name="filePath">Path to the C# file.</param>
    /// <param name="sourceCode">Source code content.</param>
    /// <returns>Analysis results.</returns>
    public async Task<MediatRAnalysisResult> AnalyzeFileAsync(string filePath, string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = await syntaxTree.GetRootAsync();
        
        var walker = new MediatRSyntaxWalker();
        walker.Visit(root);
        
        _handlers.AddRange(walker.Handlers);
        _notificationHandlers.AddRange(walker.NotificationHandlers);
        _usages.AddRange(walker.Usages);
        
        return new MediatRAnalysisResult
        {
            FilePath = filePath,
            HandlersFound = walker.Handlers.Count,
            NotificationHandlersFound = walker.NotificationHandlers.Count,
            UsagesFound = walker.Usages.Count,
            HasMediatRReferences = walker.HasMediatRReferences
        };
    }

    /// <summary>
    /// Analyzes multiple files in a directory.
    /// </summary>
    /// <param name="directoryPath">Directory path to analyze.</param>
    /// <param name="searchPattern">File search pattern.</param>
    /// <returns>Analysis results for all files.</returns>
    public async Task<List<MediatRAnalysisResult>> AnalyzeDirectoryAsync(string directoryPath, string searchPattern = "*.cs")
    {
        var results = new List<MediatRAnalysisResult>();
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
                results.Add(new MediatRAnalysisResult
                {
                    FilePath = file,
                    Error = ex.Message
                });
            }
        }
        
        return results;
    }
}

/// <summary>
/// Syntax walker that identifies MediatR usage patterns.
/// </summary>
internal class MediatRSyntaxWalker : CSharpSyntaxWalker
{
    public List<RequestHandler> Handlers { get; } = new();
    public List<NotificationHandler> NotificationHandlers { get; } = new();
    public List<MediatRUsage> Usages { get; } = new();
    public bool HasMediatRReferences { get; private set; }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Name?.ToString().Contains("MediatR") == true)
        {
            HasMediatRReferences = true;
        }
        base.VisitUsingDirective(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // Look for handler classes
        if (node.BaseList != null)
        {
            foreach (var baseType in node.BaseList.Types)
            {
                var baseTypeName = baseType.ToString();
                
                // Check for IRequestHandler<TRequest, TResponse>
                if (baseTypeName.Contains("IRequestHandler"))
                {
                    var handler = ExtractRequestHandler(node, baseTypeName);
                    if (handler != null)
                    {
                        Handlers.Add(handler);
                    }
                }
                
                // Check for INotificationHandler<TNotification>
                if (baseTypeName.Contains("INotificationHandler"))
                {
                    var notificationHandler = ExtractNotificationHandler(node, baseTypeName);
                    if (notificationHandler != null)
                    {
                        NotificationHandlers.Add(notificationHandler);
                    }
                }
            }
        }
        
        base.VisitClassDeclaration(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Look for MediatR Send/Publish calls
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.ValueText;
            if (methodName == "Send" || methodName == "Publish")
            {
                var usage = ExtractMediatRUsage(node, methodName);
                if (usage != null)
                {
                    Usages.Add(usage);
                }
            }
        }
        
        base.VisitInvocationExpression(node);
    }

    private RequestHandler? ExtractRequestHandler(ClassDeclarationSyntax classNode, string baseTypeName)
    {
        var handler = new RequestHandler
        {
            ClassName = classNode.Identifier.ValueText,
            Namespace = GetNamespace(classNode),
            BaseType = baseTypeName,
            HandleMethod = ExtractHandleMethod(classNode)
        };

        // Extract generic type parameters
        var genericTypes = ExtractGenericTypes(baseTypeName);
        if (genericTypes.Count >= 2)
        {
            handler.RequestType = genericTypes[0];
            handler.ResponseType = genericTypes[1];
        }

        return handler;
    }

    private NotificationHandler? ExtractNotificationHandler(ClassDeclarationSyntax classNode, string baseTypeName)
    {
        var handler = new NotificationHandler
        {
            ClassName = classNode.Identifier.ValueText,
            Namespace = GetNamespace(classNode),
            BaseType = baseTypeName,
            HandleMethod = ExtractHandleMethod(classNode)
        };

        // Extract generic type parameter
        var genericTypes = ExtractGenericTypes(baseTypeName);
        if (genericTypes.Count >= 1)
        {
            handler.NotificationType = genericTypes[0];
        }

        return handler;
    }

    private HandleMethod? ExtractHandleMethod(ClassDeclarationSyntax classNode)
    {
        var handleMethod = classNode.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Handle");

        if (handleMethod != null)
        {
            return new HandleMethod
            {
                MethodName = handleMethod.Identifier.ValueText,
                Parameters = handleMethod.ParameterList.Parameters.Select(p => p.ToString()).ToList(),
                ReturnType = handleMethod.ReturnType.ToString(),
                LineNumber = handleMethod.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };
        }

        return null;
    }

    private MediatRUsage? ExtractMediatRUsage(InvocationExpressionSyntax node, string methodName)
    {
        return new MediatRUsage
        {
            MethodName = methodName,
            Arguments = node.ArgumentList.Arguments.Select(a => a.ToString()).ToList(),
            LineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
        };
    }

    private List<string> ExtractGenericTypes(string typeName)
    {
        var types = new List<string>();
        
        var startIndex = typeName.IndexOf('<');
        var endIndex = typeName.LastIndexOf('>');
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            var genericPart = typeName.Substring(startIndex + 1, endIndex - startIndex - 1);
            var typeParts = genericPart.Split(',');
            
            foreach (var part in typeParts)
            {
                types.Add(part.Trim());
            }
        }
        
        return types;
    }

    private string GetNamespace(SyntaxNode node)
    {
        var namespaceNode = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceNode?.Name.ToString() ?? string.Empty;
    }
}

/// <summary>
/// Represents a request handler found in the code.
/// </summary>
public class RequestHandler
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string BaseType { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public HandleMethod? HandleMethod { get; set; }
}

/// <summary>
/// Represents a notification handler found in the code.
/// </summary>
public class NotificationHandler
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string BaseType { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public HandleMethod? HandleMethod { get; set; }
}

/// <summary>
/// Represents a handle method.
/// </summary>
public class HandleMethod
{
    public string MethodName { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new();
    public int LineNumber { get; set; }
}

/// <summary>
/// Represents MediatR usage in the code.
/// </summary>
public class MediatRUsage
{
    public string MethodName { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
    public int LineNumber { get; set; }
}

/// <summary>
/// Analysis result for MediatR usage in a single file.
/// </summary>
public class MediatRAnalysisResult
{
    public string FilePath { get; set; } = string.Empty;
    public int HandlersFound { get; set; }
    public int NotificationHandlersFound { get; set; }
    public int UsagesFound { get; set; }
    public bool HasMediatRReferences { get; set; }
    public string? Error { get; set; }
}
