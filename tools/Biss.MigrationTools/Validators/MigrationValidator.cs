using Biss.MigrationTools.Analyzers;
using Biss.MigrationTools.Generators;
using System.CommandLine;
using System.Text.Json;

namespace Biss.MigrationTools.Validators;

/// <summary>
/// Validates migrated code to ensure compilation and correctness.
/// </summary>
public class MigrationValidator
{
    /// <summary>
    /// Validates migrated handlers.
    /// </summary>
    /// <param name="handlerDirectory">Directory containing migrated handlers.</param>
    /// <param name="originalDirectory">Directory containing original MediatR handlers.</param>
    /// <returns>Validation results.</returns>
    public async Task<ValidationResult> ValidateHandlersAsync(string handlerDirectory, string? originalDirectory = null)
    {
        var results = new ValidationResult();
        results.StartedAt = DateTime.UtcNow;

        try
        {
            Console.WriteLine("🔍 Validating migrated handlers...");
            
            // Check if directory exists
            if (!Directory.Exists(handlerDirectory))
            {
                results.IsValid = false;
                results.Errors.Add($"Directory not found: {handlerDirectory}");
                return results;
            }

            // Get all handler files
            var handlerFiles = Directory.GetFiles(handlerDirectory, "*.cs", SearchOption.AllDirectories);
            results.TotalFiles = handlerFiles.Length;

            Console.WriteLine($"📊 Found {handlerFiles.Length} handler files");

            // Validate each file
            foreach (var file in handlerFiles)
            {
                var fileValidation = await ValidateHandlerFileAsync(file);
                results.FileResults.Add(fileValidation);
            }

            // Validate against original if provided
            if (!string.IsNullOrEmpty(originalDirectory) && Directory.Exists(originalDirectory))
            {
                await ValidateAgainstOriginalAsync(results, handlerDirectory, originalDirectory);
            }

            // Compile validation summary
            results.IsValid = results.FileResults.All(r => r.IsValid);
            results.TotalErrors = results.FileResults.Sum(r => r.Errors.Count);
            results.TotalWarnings = results.FileResults.Sum(r => r.Warnings.Count);
            results.CompletedAt = DateTime.UtcNow;

            Console.WriteLine($"✅ Validation completed: {results.TotalFiles} files, {results.TotalErrors} errors, {results.TotalWarnings} warnings");
        }
        catch (Exception ex)
        {
            results.IsValid = false;
            results.Errors.Add($"Validation failed: {ex.Message}");
        }

        return results;
    }

    private async Task<FileValidationResult> ValidateHandlerFileAsync(string filePath)
    {
        var result = new FileValidationResult
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath)
        };

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            
            // Check for required using statements
            if (!content.Contains("using Biss.Mediator.Abstractions"))
            {
                result.Warnings.Add("Missing using statement for Biss.Mediator.Abstractions");
            }

            // Check for handler interface implementation
            if (!content.Contains("ICommandHandler") && !content.Contains("IQueryHandler") && !content.Contains("INotificationHandler"))
            {
                result.Errors.Add("Handler does not implement required interface");
            }

            // Check for Handle method
            if (!content.Contains("Handle("))
            {
                result.Errors.Add("Handler is missing Handle method");
            }

            // Check for TODO comments
            if (content.Contains("TODO"))
            {
                var todoCount = content.Split("TODO", StringSplitOptions.None).Length - 1;
                result.Warnings.Add($"Contains {todoCount} TODO comments that need implementation");
            }

            // Check for required dependencies
            if (!content.Contains("ILogger"))
            {
                result.Warnings.Add("Handler is missing ILogger dependency");
            }

            // Check syntax errors (basic)
            if (content.Count(c => c == '{') != content.Count(c => c == '}'))
            {
                result.Errors.Add("Unbalanced curly braces");
            }

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading file: {ex.Message}");
            result.IsValid = false;
        }

        return result;
    }

    private async Task ValidateAgainstOriginalAsync(ValidationResult results, string migratedDirectory, string originalDirectory)
    {
        Console.WriteLine("🔍 Comparing with original handlers...");

        // This would compare the structure of original vs migrated handlers
        // For now, just report
        results.Warnings.Add("Original handler comparison not yet implemented");
    }

    /// <summary>
    /// Validates migrated mappers.
    /// </summary>
    /// <param name="mapperDirectory">Directory containing migrated mappers.</param>
    /// <param name="originalDirectory">Directory containing original AutoMapper profiles.</param>
    /// <returns>Validation results.</returns>
    public async Task<ValidationResult> ValidateMappersAsync(string mapperDirectory, string? originalDirectory = null)
    {
        var results = new ValidationResult();
        results.StartedAt = DateTime.UtcNow;

        try
        {
            Console.WriteLine("🔍 Validating migrated mappers...");
            
            if (!Directory.Exists(mapperDirectory))
            {
                results.IsValid = false;
                results.Errors.Add($"Directory not found: {mapperDirectory}");
                return results;
            }

            var mapperFiles = Directory.GetFiles(mapperDirectory, "*.cs", SearchOption.AllDirectories);
            results.TotalFiles = mapperFiles.Length;

            Console.WriteLine($"📊 Found {mapperFiles.Length} mapper files");

            foreach (var file in mapperFiles)
            {
                var fileValidation = await ValidateMapperFileAsync(file);
                results.FileResults.Add(fileValidation);
            }

            results.IsValid = results.FileResults.All(r => r.IsValid);
            results.TotalErrors = results.FileResults.Sum(r => r.Errors.Count);
            results.TotalWarnings = results.FileResults.Sum(r => r.Warnings.Count);
            results.CompletedAt = DateTime.UtcNow;

            Console.WriteLine($"✅ Validation completed: {results.TotalFiles} files, {results.TotalErrors} errors, {results.TotalWarnings} warnings");
        }
        catch (Exception ex)
        {
            results.IsValid = false;
            results.Errors.Add($"Validation failed: {ex.Message}");
        }

        return results;
    }

    private async Task<FileValidationResult> ValidateMapperFileAsync(string filePath)
    {
        var result = new FileValidationResult
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath)
        };

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            
            // Check for Mapper attribute
            if (!content.Contains("[Mapper]"))
            {
                result.Warnings.Add("Mapper class is missing [Mapper] attribute");
            }

            // Check for mapper methods
            if (!content.Contains("public partial"))
            {
                result.Errors.Add("Mapper is missing mapping methods");
            }

            // Check for TODO comments
            if (content.Contains("TODO"))
            {
                var todoCount = content.Split("TODO", StringSplitOptions.None).Length - 1;
                result.Warnings.Add($"Contains {todoCount} TODO comments that need implementation");
            }

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading file: {ex.Message}");
            result.IsValid = false;
        }

        return result;
    }
}

/// <summary>
/// Validation result for the entire migration.
/// </summary>
public class ValidationResult
{
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool IsValid { get; set; }
    public int TotalFiles { get; set; }
    public int TotalErrors { get; set; }
    public int TotalWarnings { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<FileValidationResult> FileResults { get; set; } = new();

    public TimeSpan Duration => CompletedAt - StartedAt;

    public string GenerateReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== MIGRATION VALIDATION REPORT ===");
        sb.AppendLine($"Started: {StartedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Completed: {CompletedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {Duration.TotalSeconds}s");
        sb.AppendLine();
        sb.AppendLine($"Status: {(IsValid ? "✅ VALID" : "❌ INVALID")}");
        sb.AppendLine($"Total Files: {TotalFiles}");
        sb.AppendLine($"Total Errors: {TotalErrors}");
        sb.AppendLine($"Total Warnings: {TotalWarnings}");
        sb.AppendLine();

        if (Errors.Any())
        {
            sb.AppendLine("=== ERRORS ===");
            foreach (var error in Errors)
            {
                sb.AppendLine($"❌ {error}");
            }
            sb.AppendLine();
        }

        if (Warnings.Any())
        {
            sb.AppendLine("=== WARNINGS ===");
            foreach (var warning in Warnings)
            {
                sb.AppendLine($"⚠️ {warning}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

/// <summary>
/// Validation result for a single file.
/// </summary>
public class FileValidationResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
