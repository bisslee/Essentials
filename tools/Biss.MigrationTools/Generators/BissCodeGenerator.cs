using System.Text;

namespace Biss.MigrationTools.Generators;

/// <summary>
/// Generates Biss Mapper code from AutoMapper profiles.
/// </summary>
public class BissMapperGenerator
{
    /// <summary>
    /// Generates Biss Mapper code from AutoMapper profile.
    /// </summary>
    /// <param name="profile">AutoMapper profile to convert.</param>
    /// <returns>Generated Biss Mapper code.</returns>
    public string GenerateMapperCode(Analyzers.MappingProfile profile)
    {
        var sb = new StringBuilder();
        
        // Add using statements
        sb.AppendLine("using Biss.Mapper.Abstractions;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        
        // Add namespace
        if (!string.IsNullOrEmpty(profile.Namespace))
        {
            sb.AppendLine($"namespace {profile.Namespace};");
            sb.AppendLine();
        }
        
        // Add mapper class
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Auto-generated mapper for {profile.ClassName}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[Mapper]");
        sb.AppendLine($"public partial class {profile.ClassName.Replace("Profile", "Mapper")}");
        sb.AppendLine("{");
        
        // Generate mapping methods
        foreach (var mapping in profile.Mappings)
        {
            GenerateMappingMethod(sb, mapping);
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private void GenerateMappingMethod(StringBuilder sb, Analyzers.MappingDefinition mapping)
    {
        var sourceType = CleanTypeName(mapping.SourceType);
        var destinationType = CleanTypeName(mapping.DestinationType);
        
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Maps {sourceType} to {destinationType}");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public partial {destinationType} To{destinationType}({sourceType} source)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (source == null)");
        sb.AppendLine("            return null!;");
        sb.AppendLine();
        sb.AppendLine($"        return new {destinationType}");
        sb.AppendLine("        {");
        
        // Generate property mappings (simplified)
        GeneratePropertyMappings(sb, sourceType, destinationType);
        
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private void GeneratePropertyMappings(StringBuilder sb, string sourceType, string destinationType)
    {
        // This is a simplified implementation
        // In a real scenario, you would analyze the actual types to generate proper mappings
        sb.AppendLine("            // TODO: Implement property mappings");
        sb.AppendLine("            // This is a placeholder - actual mappings should be generated based on type analysis");
    }

    private string CleanTypeName(string typeName)
    {
        // Remove generic parameters and clean up the type name
        var cleanName = typeName.Split('<')[0].Trim();
        return cleanName;
    }
}

/// <summary>
/// Generates Biss Mediator code from MediatR handlers.
/// </summary>
public class BissMediatorGenerator
{
    /// <summary>
    /// Generates Biss Mediator handler code from MediatR handler.
    /// </summary>
    /// <param name="handler">MediatR handler to convert.</param>
    /// <returns>Generated Biss Mediator handler code.</returns>
    public string GenerateHandlerCode(Analyzers.RequestHandler handler)
    {
        var sb = new StringBuilder();
        
        // Add using statements
        sb.AppendLine("using Biss.Mediator.Abstractions;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        
        // Add namespace
        if (!string.IsNullOrEmpty(handler.Namespace))
        {
            sb.AppendLine($"namespace {handler.Namespace};");
            sb.AppendLine();
        }
        
        // Generate request/command/query classes
        GenerateRequestClasses(sb, handler);
        
        // Generate handler class
        GenerateHandlerClass(sb, handler);
        
        return sb.ToString();
    }

    /// <summary>
    /// Generates Biss Mediator notification handler code from MediatR handler.
    /// </summary>
    /// <param name="handler">MediatR notification handler to convert.</param>
    /// <returns>Generated Biss Mediator notification handler code.</returns>
    public string GenerateNotificationHandlerCode(Analyzers.NotificationHandler handler)
    {
        var sb = new StringBuilder();
        
        // Add using statements
        sb.AppendLine("using Biss.Mediator.Abstractions;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        
        // Add namespace
        if (!string.IsNullOrEmpty(handler.Namespace))
        {
            sb.AppendLine($"namespace {handler.Namespace};");
            sb.AppendLine();
        }
        
        // Generate notification class
        GenerateNotificationClass(sb, handler);
        
        // Generate handler class
        GenerateNotificationHandlerClass(sb, handler);
        
        return sb.ToString();
    }

    private void GenerateRequestClasses(StringBuilder sb, Analyzers.RequestHandler handler)
    {
        var requestType = CleanTypeName(handler.RequestType);
        var responseType = CleanTypeName(handler.ResponseType);
        
        // Determine if it's a command or query based on naming convention
        var isCommand = requestType.Contains("Command") || requestType.EndsWith("Command");
        var isQuery = requestType.Contains("Query") || requestType.EndsWith("Query");
        
        if (isCommand)
        {
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// Command: {requestType}");
            sb.AppendLine($"/// </summary>");
            sb.AppendLine($"public record {requestType} : ICommand");
            sb.AppendLine("{");
            sb.AppendLine("    // TODO: Add command properties");
            sb.AppendLine("}");
            sb.AppendLine();
        }
        else if (isQuery)
        {
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// Query: {requestType}");
            sb.AppendLine($"/// </summary>");
            sb.AppendLine($"public record {requestType} : IQuery<{responseType}>");
            sb.AppendLine("{");
            sb.AppendLine("    // TODO: Add query properties");
            sb.AppendLine("}");
            sb.AppendLine();
        }
        else
        {
            // Generic request
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// Request: {requestType}");
            sb.AppendLine($"/// </summary>");
            sb.AppendLine($"public record {requestType} : IRequest<{responseType}>");
            sb.AppendLine("{");
            sb.AppendLine("    // TODO: Add request properties");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private void GenerateNotificationClass(StringBuilder sb, Analyzers.NotificationHandler handler)
    {
        var notificationType = CleanTypeName(handler.NotificationType);
        
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Notification: {notificationType}");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public record {notificationType} : INotification");
        sb.AppendLine("{");
        sb.AppendLine("    // TODO: Add notification properties");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private void GenerateHandlerClass(StringBuilder sb, Analyzers.RequestHandler handler)
    {
        var handlerName = handler.ClassName;
        var requestType = CleanTypeName(handler.RequestType);
        var responseType = CleanTypeName(handler.ResponseType);
        
        // Better detection: check handler name, not request type
        var isQuery = handlerName.StartsWith("Get", StringComparison.Ordinal);
        var isCommand = !isQuery && (handlerName.Contains("Add") || handlerName.Contains("Change") || 
                       handlerName.Contains("Remove") || handlerName.Contains("Update") ||
                       handlerName.Contains("Register") || handlerName.Contains("Logout") ||
                       handlerName.Contains("Reset") || handlerName.Contains("Revoke"));
        
        string interfaceName;
        if (isCommand)
        {
            interfaceName = $"ICommandHandler<{requestType}>";
        }
        else if (isQuery)
        {
            interfaceName = $"IQueryHandler<{requestType}, {responseType}>";
        }
        else
        {
            // Default to command handler for backward compatibility
            interfaceName = $"ICommandHandler<{requestType}>";
        }
        
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Handler for {requestType}");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class {handlerName} : {interfaceName}");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly ILogger<" + handlerName + "> _logger;");
        sb.AppendLine();
        sb.AppendLine($"    public {handlerName}(ILogger<{handlerName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _logger = logger;");
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // Generate Handle method  
        GenerateHandleMethod(sb, handler, requestType, responseType, isCommand, isQuery);
        
        sb.AppendLine("}");
    }

    private void GenerateNotificationHandlerClass(StringBuilder sb, Analyzers.NotificationHandler handler)
    {
        var handlerName = handler.ClassName;
        var notificationType = CleanTypeName(handler.NotificationType);
        
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Handler for {notificationType}");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class {handlerName} : INotificationHandler<{notificationType}>");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly ILogger<" + handlerName + "> _logger;");
        sb.AppendLine();
        sb.AppendLine($"    public {handlerName}(ILogger<{handlerName}> logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _logger = logger;");
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // Generate Handle method
        sb.AppendLine($"    public async Task Handle({notificationType} notification, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        _logger.LogInformation(\"Processing notification {NotificationType}\", typeof(" + notificationType + ").Name);");
        sb.AppendLine();
        sb.AppendLine("        // TODO: Implement notification handling logic");
        sb.AppendLine("        await Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
    }

    private void GenerateHandleMethod(StringBuilder sb, Analyzers.RequestHandler handler, string requestType, string responseType, bool isCommand, bool isQuery)
    {
        if (isCommand)
        {
            sb.AppendLine($"    public async Task<Result<Unit>> Handle({requestType} request, CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        _logger.LogInformation(\"Handling command {CommandType}\", typeof(" + requestType + ").Name);");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Implement command handling logic");
            sb.AppendLine("        await Task.CompletedTask;");
            sb.AppendLine();
            sb.AppendLine("        return Result<Unit>.Success(Unit.Value);");
        }
        else if (isQuery)
        {
            sb.AppendLine($"    public async Task<Result<{responseType}>> Handle({requestType} request, CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        _logger.LogInformation(\"Handling query {QueryType}\", typeof(" + requestType + ").Name);");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Implement query handling logic");
            sb.AppendLine("        await Task.CompletedTask;");
            sb.AppendLine();
            sb.AppendLine($"        // TODO: Return actual result");
            sb.AppendLine($"        return Result<{responseType}>.Success(default!);");
        }
        else
        {
            sb.AppendLine($"    public async Task<Result<{responseType}>> Handle({requestType} request, CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        _logger.LogInformation(\"Handling request {RequestType}\", typeof(" + requestType + ").Name);");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Implement request handling logic");
            sb.AppendLine("        await Task.CompletedTask;");
            sb.AppendLine();
            sb.AppendLine($"        // TODO: Return actual result");
            sb.AppendLine($"        return Result<{responseType}>.Success(default!);");
        }
        
        sb.AppendLine("    }");
    }

    private string CleanTypeName(string typeName)
    {
        // Remove generic parameters and clean up the type name
        var cleanName = typeName.Split('<')[0].Trim();
        return cleanName;
    }
}
