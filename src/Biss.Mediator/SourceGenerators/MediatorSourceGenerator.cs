using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Biss.Mediator.SourceGenerators;

/// <summary>
/// Source generator that creates optimized mediator dispatch code.
/// </summary>
[Generator]
public class MediatorSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new MediatorSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not MediatorSyntaxReceiver receiver)
            return;

        var compilation = context.Compilation;
        var mediatorInterface = compilation.GetTypeByMetadataName("Biss.Mediator.Abstractions.IMediator");
        var requestInterface = compilation.GetTypeByMetadataName("Biss.Mediator.Abstractions.IRequest`1");
        var commandInterface = compilation.GetTypeByMetadataName("Biss.Mediator.Abstractions.ICommand");
        var queryInterface = compilation.GetTypeByMetadataName("Biss.Mediator.Abstractions.IQuery`1");
        var notificationInterface = compilation.GetTypeByMetadataName("Biss.Mediator.Abstractions.INotification");

        if (mediatorInterface == null || requestInterface == null || commandInterface == null || 
            queryInterface == null || notificationInterface == null)
            return;

        var handlers = new List<HandlerInfo>();
        var notifications = new List<NotificationInfo>();

        foreach (var classDeclaration in receiver.CandidateClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null) continue;

            // Find request handlers
            foreach (var interfaceSymbol in classSymbol.AllInterfaces)
            {
                if (IsRequestHandler(interfaceSymbol, requestInterface, commandInterface, queryInterface))
                {
                    var handlerInfo = ExtractHandlerInfo(interfaceSymbol, classSymbol);
                    if (handlerInfo != null)
                        handlers.Add(handlerInfo);
                }
                else if (IsNotificationHandler(interfaceSymbol, notificationInterface))
                {
                    var notificationInfo = ExtractNotificationInfo(interfaceSymbol, classSymbol);
                    if (notificationInfo != null)
                        notifications.Add(notificationInfo);
                }
            }
        }

        if (handlers.Any() || notifications.Any())
        {
            var source = GenerateMediatorClass(handlers, notifications);
            context.AddSource("Mediator.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static bool IsRequestHandler(INamedTypeSymbol interfaceSymbol, INamedTypeSymbol requestInterface, 
        INamedTypeSymbol commandInterface, INamedTypeSymbol queryInterface)
    {
        if (!interfaceSymbol.IsGenericType) return false;

        var genericDefinition = interfaceSymbol.ConstructedFrom;
        var name = genericDefinition.Name;

        return name == "IRequestHandler" || name == "ICommandHandler" || name == "IQueryHandler";
    }

    private static bool IsNotificationHandler(INamedTypeSymbol interfaceSymbol, INamedTypeSymbol notificationInterface)
    {
        if (!interfaceSymbol.IsGenericType) return false;

        var genericDefinition = interfaceSymbol.ConstructedFrom;
        return genericDefinition.Name == "INotificationHandler";
    }

    private static HandlerInfo? ExtractHandlerInfo(INamedTypeSymbol interfaceSymbol, INamedTypeSymbol classSymbol)
    {
        if (!interfaceSymbol.IsGenericType) return null;

        var typeArguments = interfaceSymbol.TypeArguments;
        if (typeArguments.Length < 1) return null;

        var requestType = typeArguments[0];
        var responseType = typeArguments.Length > 1 ? typeArguments[1] : null;

        return new HandlerInfo
        {
            RequestType = requestType,
            ResponseType = responseType,
            HandlerType = classSymbol,
            InterfaceType = interfaceSymbol
        };
    }

    private static NotificationInfo? ExtractNotificationInfo(INamedTypeSymbol interfaceSymbol, INamedTypeSymbol classSymbol)
    {
        if (!interfaceSymbol.IsGenericType) return null;

        var typeArguments = interfaceSymbol.TypeArguments;
        if (typeArguments.Length < 1) return null;

        var notificationType = typeArguments[0];

        return new NotificationInfo
        {
            NotificationType = notificationType,
            HandlerType = classSymbol,
            InterfaceType = interfaceSymbol
        };
    }

    private static string GenerateMediatorClass(List<HandlerInfo> handlers, List<NotificationInfo> notifications)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using Biss.Mediator.Abstractions;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("namespace Biss.Mediator;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated mediator implementation with optimized dispatch.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public partial class Mediator : IMediator");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly IServiceProvider _serviceProvider;");
        sb.AppendLine("    private readonly ILogger<Mediator> _logger;");
        sb.AppendLine();
        sb.AppendLine("    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _serviceProvider = serviceProvider;");
        sb.AppendLine("        _logger = logger;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate Send method
        GenerateSendMethod(sb, handlers);

        // Generate Publish method
        GeneratePublishMethod(sb, notifications);

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateSendMethod(StringBuilder sb, List<HandlerInfo> handlers)
    {
        sb.AppendLine("    public Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        return request switch");
        sb.AppendLine("        {");

        foreach (var handler in handlers)
        {
            var requestTypeName = handler.RequestType.ToDisplayString();
            var responseTypeName = handler.ResponseType?.ToDisplayString() ?? "Unit";
            var handlerTypeName = handler.HandlerType.ToDisplayString();
            var interfaceTypeName = handler.InterfaceType.ToDisplayString();

            sb.AppendLine($"            {requestTypeName} cmd => HandleRequest<{requestTypeName}, {responseTypeName}>(cmd, cancellationToken),");
        }

        sb.AppendLine("            _ => Task.FromResult(Result<TResponse>.Failure(Error.Generic(\"HANDLER_NOT_FOUND\", $\"No handler found for request type {{typeof(TResponse).Name}}\")))");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate HandleRequest method
        sb.AppendLine("    private async Task<Result<TResponse>> HandleRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)");
        sb.AppendLine("        where TRequest : IRequest<TResponse>");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();");
        sb.AppendLine("            return await handler.Handle(request, cancellationToken);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            _logger.LogError(ex, \"Error handling request of type {RequestType}\", typeof(TRequest).Name);");
        sb.AppendLine("            return Result<TResponse>.Failure(Error.Generic(\"HANDLER_ERROR\", ex.Message));");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GeneratePublishMethod(StringBuilder sb, List<NotificationInfo> notifications)
    {
        sb.AppendLine("    public Task Publish(INotification notification, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        return notification switch");
        sb.AppendLine("        {");

        foreach (var notification in notifications)
        {
            var notificationTypeName = notification.NotificationType.ToDisplayString();
            sb.AppendLine($"            {notificationTypeName} notif => HandleNotification<{notificationTypeName}>(notif, cancellationToken),");
        }

        sb.AppendLine("            _ => Task.CompletedTask");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate HandleNotification method
        sb.AppendLine("    private async Task HandleNotification<TNotification>(TNotification notification, CancellationToken cancellationToken)");
        sb.AppendLine("        where TNotification : INotification");
        sb.AppendLine("    {");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();");
        sb.AppendLine("            var tasks = handlers.Select(handler => handler.Handle(notification, cancellationToken));");
        sb.AppendLine("            await Task.WhenAll(tasks);");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception ex)");
        sb.AppendLine("        {");
        sb.AppendLine("            _logger.LogError(ex, \"Error handling notification of type {NotificationType}\", typeof(TNotification).Name);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }
}

/// <summary>
/// Syntax receiver that finds candidate classes for the mediator.
/// </summary>
public class MediatorSyntaxReceiver : ISyntaxReceiver
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

/// <summary>
/// Information about a request handler.
/// </summary>
public class HandlerInfo
{
    public ITypeSymbol RequestType { get; set; } = null!;
    public ITypeSymbol? ResponseType { get; set; }
    public INamedTypeSymbol HandlerType { get; set; } = null!;
    public INamedTypeSymbol InterfaceType { get; set; } = null!;
}

/// <summary>
/// Information about a notification handler.
/// </summary>
public class NotificationInfo
{
    public ITypeSymbol NotificationType { get; set; } = null!;
    public INamedTypeSymbol HandlerType { get; set; } = null!;
    public INamedTypeSymbol InterfaceType { get; set; } = null!;
}
