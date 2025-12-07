using Biss.MigrationTools.Analyzers;
using Biss.MigrationTools.Generators;
using Biss.MigrationTools.Validators;
using System.CommandLine;
using System.Text.Json;

namespace Biss.MigrationTools;

/// <summary>
/// Migration tool for converting AutoMapper and MediatR code to Biss components.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Biss Migration Tools - Convert AutoMapper and MediatR to Biss components");

        // AutoMapper migration command
        var autoMapperCommand = new Command("automapper", "Analyze and migrate AutoMapper code");
        var autoMapperPathOption = new Option<string>("--path", "Path to analyze") { IsRequired = true };
        var autoMapperOutputOption = new Option<string>("--output", "Output directory for generated code") { IsRequired = true };
        var autoMapperPatternOption = new Option<string>("--pattern", () => "*.cs", "File search pattern");
        
        autoMapperCommand.AddOption(autoMapperPathOption);
        autoMapperCommand.AddOption(autoMapperOutputOption);
        autoMapperCommand.AddOption(autoMapperPatternOption);
        
        autoMapperCommand.SetHandler(async (path, output, pattern) =>
        {
            await MigrateAutoMapper(path, output, pattern);
        }, autoMapperPathOption, autoMapperOutputOption, autoMapperPatternOption);

        // MediatR migration command
        var mediatRCommand = new Command("mediatr", "Analyze and migrate MediatR code");
        var mediatRPathOption = new Option<string>("--path", "Path to analyze") { IsRequired = true };
        var mediatROutputOption = new Option<string>("--output", "Output directory for generated code") { IsRequired = true };
        var mediatRPatternOption = new Option<string>("--pattern", () => "*.cs", "File search pattern");
        
        mediatRCommand.AddOption(mediatRPathOption);
        mediatRCommand.AddOption(mediatROutputOption);
        mediatRCommand.AddOption(mediatRPatternOption);
        
        mediatRCommand.SetHandler(async (path, output, pattern) =>
        {
            await MigrateMediatR(path, output, pattern);
        }, mediatRPathOption, mediatROutputOption, mediatRPatternOption);

        // Analysis command
        var analyzeCommand = new Command("analyze", "Analyze codebase for AutoMapper and MediatR usage");
        var analyzePathOption = new Option<string>("--path", "Path to analyze") { IsRequired = true };
        var analyzeOutputOption = new Option<string>("--output", "Output file for analysis report");
        
        analyzeCommand.AddOption(analyzePathOption);
        analyzeCommand.AddOption(analyzeOutputOption);
        
        analyzeCommand.SetHandler(async (path, output) =>
        {
            await AnalyzeCodebase(path, output);
        }, analyzePathOption, analyzeOutputOption);

        // Validation command for handlers
        var validateHandlersCommand = new Command("validate-handlers", "Validate migrated handlers");
        var validateHandlersPathOption = new Option<string>("--path", "Path to migrated handlers") { IsRequired = true };
        var validateHandlersOriginalOption = new Option<string>("--original", "Path to original handlers for comparison");
        
        validateHandlersCommand.AddOption(validateHandlersPathOption);
        validateHandlersCommand.AddOption(validateHandlersOriginalOption);
        
        validateHandlersCommand.SetHandler(async (path, original) =>
        {
            await ValidateHandlers(path, original);
        }, validateHandlersPathOption, validateHandlersOriginalOption);

        // Validation command for mappers
        var validateMappersCommand = new Command("validate-mappers", "Validate migrated mappers");
        var validateMappersPathOption = new Option<string>("--path", "Path to migrated mappers") { IsRequired = true };
        var validateMappersOriginalOption = new Option<string>("--original", "Path to original profiles for comparison");
        
        validateMappersCommand.AddOption(validateMappersPathOption);
        validateMappersCommand.AddOption(validateMappersOriginalOption);
        
        validateMappersCommand.SetHandler(async (path, original) =>
        {
            await ValidateMappers(path, original);
        }, validateMappersPathOption, validateMappersOriginalOption);

        rootCommand.AddCommand(autoMapperCommand);
        rootCommand.AddCommand(mediatRCommand);
        rootCommand.AddCommand(analyzeCommand);
        rootCommand.AddCommand(validateHandlersCommand);
        rootCommand.AddCommand(validateMappersCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task MigrateAutoMapper(string path, string output, string pattern)
    {
        Console.WriteLine($"🔍 Analyzing AutoMapper code in: {path}");
        
        var analyzer = new AutoMapperAnalyzer();
        var results = await analyzer.AnalyzeDirectoryAsync(path, pattern);
        
        var totalProfiles = results.Sum(r => r.ProfilesFound);
        var totalUsages = results.Sum(r => r.UsagesFound);
        
        Console.WriteLine($"📊 Found {totalProfiles} profiles and {totalUsages} usages");
        
        if (totalProfiles == 0)
        {
            Console.WriteLine("❌ No AutoMapper profiles found to migrate");
            return;
        }
        
        Directory.CreateDirectory(output);
        
        var generator = new BissMapperGenerator();
        
        foreach (var profile in analyzer.Profiles)
        {
            Console.WriteLine($"🔄 Migrating profile: {profile.ClassName}");
            
            var generatedCode = generator.GenerateMapperCode(profile);
            var fileName = $"{profile.ClassName.Replace("Profile", "Mapper")}.cs";
            var filePath = Path.Combine(output, fileName);
            
            await File.WriteAllTextAsync(filePath, generatedCode);
            Console.WriteLine($"✅ Generated: {filePath}");
        }
        
        // Generate migration report
        var report = analyzer.GenerateMigrationReport();
        var reportPath = Path.Combine(output, "migration-report.json");
        await File.WriteAllTextAsync(reportPath, report);
        
        Console.WriteLine($"📋 Migration report saved to: {reportPath}");
        Console.WriteLine("🎉 AutoMapper migration completed!");
    }

    private static async Task MigrateMediatR(string path, string output, string pattern)
    {
        Console.WriteLine($"🔍 Analyzing MediatR code in: {path}");
        
        var analyzer = new MediatRAnalyzer();
        var results = await analyzer.AnalyzeDirectoryAsync(path, pattern);
        
        var totalHandlers = results.Sum(r => r.HandlersFound);
        var totalNotificationHandlers = results.Sum(r => r.NotificationHandlersFound);
        var totalUsages = results.Sum(r => r.UsagesFound);
        
        Console.WriteLine($"📊 Found {totalHandlers} handlers, {totalNotificationHandlers} notification handlers, and {totalUsages} usages");
        
        if (totalHandlers == 0 && totalNotificationHandlers == 0)
        {
            Console.WriteLine("❌ No MediatR handlers found to migrate");
            return;
        }
        
        Directory.CreateDirectory(output);
        
        var generator = new BissMediatorGenerator();
        
        // Migrate request handlers
        foreach (var handler in analyzer.Handlers)
        {
            Console.WriteLine($"🔄 Migrating handler: {handler.ClassName}");
            
            var generatedCode = generator.GenerateHandlerCode(handler);
            var fileName = $"{handler.ClassName}.cs";
            var filePath = Path.Combine(output, fileName);
            
            await File.WriteAllTextAsync(filePath, generatedCode);
            Console.WriteLine($"✅ Generated: {filePath}");
        }
        
        // Migrate notification handlers
        foreach (var handler in analyzer.NotificationHandlers)
        {
            Console.WriteLine($"🔄 Migrating notification handler: {handler.ClassName}");
            
            var generatedCode = generator.GenerateNotificationHandlerCode(handler);
            var fileName = $"{handler.ClassName}.cs";
            var filePath = Path.Combine(output, fileName);
            
            await File.WriteAllTextAsync(filePath, generatedCode);
            Console.WriteLine($"✅ Generated: {filePath}");
        }
        
        Console.WriteLine("🎉 MediatR migration completed!");
    }

    private static async Task AnalyzeCodebase(string path, string? output)
    {
        Console.WriteLine($"🔍 Analyzing codebase: {path}");
        
        var autoMapperAnalyzer = new AutoMapperAnalyzer();
        var mediatRAnalyzer = new MediatRAnalyzer();
        
        var autoMapperResults = await autoMapperAnalyzer.AnalyzeDirectoryAsync(path);
        var mediatRResults = await mediatRAnalyzer.AnalyzeDirectoryAsync(path);
        
        var analysis = new
        {
            AnalyzedAt = DateTime.UtcNow,
            Path = path,
            AutoMapper = new
            {
                TotalFiles = autoMapperResults.Count,
                FilesWithReferences = autoMapperResults.Count(r => r.HasAutoMapperReferences),
                TotalProfiles = autoMapperResults.Sum(r => r.ProfilesFound),
                TotalUsages = autoMapperResults.Sum(r => r.UsagesFound),
                Profiles = autoMapperAnalyzer.Profiles.Select(p => new
                {
                    p.ClassName,
                    p.Namespace,
                    MappingCount = p.Mappings.Count
                }).ToList()
            },
            MediatR = new
            {
                TotalFiles = mediatRResults.Count,
                FilesWithReferences = mediatRResults.Count(r => r.HasMediatRReferences),
                TotalHandlers = mediatRResults.Sum(r => r.HandlersFound),
                TotalNotificationHandlers = mediatRResults.Sum(r => r.NotificationHandlersFound),
                TotalUsages = mediatRResults.Sum(r => r.UsagesFound),
                Handlers = mediatRAnalyzer.Handlers.Select(h => new
                {
                    h.ClassName,
                    h.Namespace,
                    h.RequestType,
                    h.ResponseType
                }).ToList(),
                NotificationHandlers = mediatRAnalyzer.NotificationHandlers.Select(h => new
                {
                    h.ClassName,
                    h.Namespace,
                    h.NotificationType
                }).ToList()
            }
        };
        
        var json = JsonSerializer.Serialize(analysis, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        if (!string.IsNullOrEmpty(output))
        {
            await File.WriteAllTextAsync(output, json);
            Console.WriteLine($"📋 Analysis report saved to: {output}");
        }
        else
        {
            Console.WriteLine(json);
        }
        
        Console.WriteLine("🎉 Analysis completed!");
    }

    private static async Task ValidateHandlers(string handlersPath, string? originalPath)
    {
        Console.WriteLine($"🔍 Validating handlers in: {handlersPath}");
        
        var validator = new MigrationValidator();
        var result = await validator.ValidateHandlersAsync(handlersPath, originalPath);
        
        Console.WriteLine();
        Console.WriteLine(result.GenerateReport());
        
        if (result.IsValid)
        {
            Console.WriteLine("✅ Validation passed!");
        }
        else
        {
            Console.WriteLine("❌ Validation failed!");
            Environment.ExitCode = 1;
        }
    }

    private static async Task ValidateMappers(string mappersPath, string? originalPath)
    {
        Console.WriteLine($"🔍 Validating mappers in: {mappersPath}");
        
        var validator = new MigrationValidator();
        var result = await validator.ValidateMappersAsync(mappersPath, originalPath);
        
        Console.WriteLine();
        Console.WriteLine(result.GenerateReport());
        
        if (result.IsValid)
        {
            Console.WriteLine("✅ Validation passed!");
        }
        else
        {
            Console.WriteLine("❌ Validation failed!");
            Environment.ExitCode = 1;
        }
    }
}
