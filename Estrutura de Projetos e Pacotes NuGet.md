# Estrutura de Projetos e Pacotes NuGet

## Estrutura de Solução

```
Biss.Essentials/
├── src/
│   ├── Biss.Mediator.Abstractions/
│   │   ├── Biss.Mediator.Abstractions.csproj
│   │   ├── IRequest.cs
│   │   ├── ICommand.cs
│   │   ├── IQuery.cs
│   │   ├── INotification.cs
│   │   ├── IRequestHandler.cs
│   │   ├── ICommandHandler.cs
│   │   ├── IQueryHandler.cs
│   │   ├── INotificationHandler.cs
│   │   ├── IPipelineBehavior.cs
│   │   ├── IMediator.cs
│   │   ├── Result.cs
│   │   ├── Error.cs
│   │   └── Unit.cs
│   │
│   ├── Biss.Mediator/
│   │   ├── Biss.Mediator.csproj
│   │   ├── Mediator.cs
│   │   ├── SourceGenerators/
│   │   │   ├── MediatorSourceGenerator.cs
│   │   │   ├── HandlerDiscovery.cs
│   │   │   └── CodeGeneration.cs
│   │   └── Diagnostics/
│   │       └── MediatorDiagnostics.cs
│   │
│   ├── Biss.Mediator.Behaviors/
│   │   ├── Biss.Mediator.Behaviors.csproj
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   ├── CachingBehavior.cs
│   │   ├── PerformanceBehavior.cs
│   │   └── RetryBehavior.cs
│   │
│   ├── Biss.Mediator.Extensions.DependencyInjection/
│   │   ├── Biss.Mediator.Extensions.DependencyInjection.csproj
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── MediatorServiceCollectionExtensions.cs
│   │
│   ├── Biss.Mediator.Extensions.AspNetCore/
│   │   ├── Biss.Mediator.Extensions.AspNetCore.csproj
│   │   ├── MediatorMiddleware.cs
│   │   ├── MediatorControllerBase.cs
│   │   └── MediatorServiceCollectionExtensions.cs
│   │
│   ├── Biss.Mapper.Abstractions/
│   │   ├── Biss.Mapper.Abstractions.csproj
│   │   ├── MapperAttribute.cs
│   │   ├── MapPropertyAttribute.cs
│   │   ├── IgnoreAttribute.cs
│   │   ├── MapConditionAttribute.cs
│   │   ├── ITypeConverter.cs
│   │   ├── IMappingContext.cs
│   │   ├── IMappingConfiguration.cs
│   │   └── IMappingProfile.cs
│   │
│   ├── Biss.Mapper/
│   │   ├── Biss.Mapper.csproj
│   │   ├── SourceGenerators/
│   │   │   ├── MapperSourceGenerator.cs
│   │   │   ├── MapperDiscovery.cs
│   │   │   └── MappingCodeGeneration.cs
│   │   ├── Diagnostics/
│   │   │   └── MapperDiagnostics.cs
│   │   └── Extensions/
│   │       └── MappingExtensions.cs
│   │
│   └── Biss.Mapper.Extensions.DependencyInjection/
│       ├── Biss.Mapper.Extensions.DependencyInjection.csproj
│       └── ServiceCollectionExtensions.cs
│
├── tools/
│   ├── Biss.MigrationTools/
│   │   ├── Biss.MigrationTools.csproj
│   │   ├── AutoMapperAnalyzer.cs
│   │   ├── MediatRAnalyzer.cs
│   │   ├── MigrationGenerator.cs
│   │   └── Program.cs
│   │
│   └── Biss.PerformanceBenchmarks/
│       ├── Biss.PerformanceBenchmarks.csproj
│       ├── MediatorBenchmarks.cs
│       ├── MapperBenchmarks.cs
│       └── Program.cs
│
├── samples/
│   ├── Biss.Samples.Mediator/
│   │   ├── Biss.Samples.Mediator.csproj
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Handlers/
│   │   ├── Behaviors/
│   │   └── Program.cs
│   │
│   ├── Biss.Samples.Mapper/
│   │   ├── Biss.Samples.Mapper.csproj
│   │   ├── Models/
│   │   ├── Mappers/
│   │   └── Program.cs
│   │
│   └── Biss.Samples.Integration/
│       ├── Biss.Samples.Integration.csproj
│       ├── Controllers/
│       ├── Services/
│       └── Program.cs
│
├── tests/
│   ├── Biss.Mediator.Tests/
│   │   ├── Biss.Mediator.Tests.csproj
│   │   ├── Unit/
│   │   ├── Integration/
│   │   └── Performance/
│   │
│   ├── Biss.Mapper.Tests/
│   │   ├── Biss.Mapper.Tests.csproj
│   │   ├── Unit/
│   │   ├── Integration/
│   │   └── Performance/
│   │
│   └── Biss.Essentials.Tests.Common/
│       ├── Biss.Essentials.Tests.Common.csproj
│       ├── TestFixtures/
│       └── Helpers/
│
├── docs/
│   ├── README.md
│   ├── MEDIATOR.md
│   ├── MAPPER.md
│   ├── MIGRATION.md
│   └── PERFORMANCE.md
│
├── Directory.Build.props
├── Directory.Packages.props
├── Biss.Essentials.sln
└── README.md
```

## Arquivos de Configuração

### Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>NU1701</WarningsNotAsErrors>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
    <Copyright>Copyright © Biss Lee 2025</Copyright>
    <Authors>Biss Lee</Authors>
    <Company>Biss</Company>
    <Product>Biss Essentials</Product>
    <Description>High-performance Mediator and Mapper components for .NET 9</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/biss/biss-essentials</PackageProjectUrl>
    <PackageRepositoryUrl>https://github.com/biss/biss-essentials</PackageRepositoryUrl>
    <PackageTags>mediator;mapper;source-generators;performance;dotnet9</PackageTags>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <RepositoryType>git</RepositoryType>
    <RepositoryUrl>https://github.com/biss/biss-essentials</RepositoryUrl>
    <Version>1.0.0</Version>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

### Directory.Packages.props

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Dependencies -->
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Hosting.Abstractions" Version="9.0.0" />
    
    <!-- ASP.NET Core -->
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Core" Version="2.2.5" />
    <PackageVersion Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
    
    <!-- Testing -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageVersion Include="xunit" Version="2.6.1" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageVersion Include="FluentAssertions" Version="6.12.0" />
    <PackageVersion Include="Moq" Version="4.20.69" />
    <PackageVersion Include="BenchmarkDotNet" Version="0.13.10" />
    
    <!-- Source Generators -->
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
    
    <!-- Validation -->
    <PackageVersion Include="FluentValidation" Version="11.8.1" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.8.1" />
    
    <!-- Caching -->
    <PackageVersion Include="Microsoft.Extensions.Caching.Memory" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Caching.Abstractions" Version="9.0.0" />
  </ItemGroup>
</Project>
```

## Arquivos de Projeto (.csproj)

### Biss.Mediator.Abstractions.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mediator.Abstractions</PackageId>
    <Description>Abstractions for Biss Mediator - High-performance mediator pattern implementation for .NET 9</Description>
    <PackageTags>mediator;cqrs;source-generators;performance;dotnet9</PackageTags>
  </PropertyGroup>
</Project>
```

### Biss.Mediator.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mediator</PackageId>
    <Description>High-performance mediator pattern implementation for .NET 9 using source generators</Description>
    <PackageTags>mediator;cqrs;source-generators;performance;dotnet9</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
```

### Biss.Mediator.Behaviors.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mediator.Behaviors</PackageId>
    <Description>Pre-built pipeline behaviors for Biss Mediator</Description>
    <PackageTags>mediator;behaviors;validation;logging;caching;dotnet9</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
```

### Biss.Mediator.Extensions.DependencyInjection.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mediator.Extensions.DependencyInjection</PackageId>
    <Description>Dependency injection extensions for Biss Mediator</Description>
    <PackageTags>mediator;dependency-injection;extensions;dotnet9</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
    <ProjectReference Include="..\Biss.Mediator\Biss.Mediator.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" />
  </ItemGroup>
</Project>
```

### Biss.Mediator.Extensions.AspNetCore.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mediator.Extensions.AspNetCore</PackageId>
    <Description>ASP.NET Core extensions for Biss Mediator</Description>
    <PackageTags>mediator;aspnetcore;extensions;dotnet9</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
    <ProjectReference Include="..\Biss.Mediator.Extensions.DependencyInjection\Biss.Mediator.Extensions.DependencyInjection.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Core" />
    <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>
</Project>
```

### Biss.Mapper.Abstractions.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mapper.Abstractions</PackageId>
    <Description>Abstractions for Biss Mapper - High-performance object mapping for .NET 9</Description>
    <PackageTags>mapper;object-mapping;source-generators;performance;dotnet9</PackageTags>
  </PropertyGroup>
</Project>
```

### Biss.Mapper.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mapper</PackageId>
    <Description>High-performance object mapping for .NET 9 using source generators</Description>
    <PackageTags>mapper;object-mapping;source-generators;performance;dotnet9</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mapper.Abstractions\Biss.Mapper.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>
</Project>
```

### Biss.Mapper.Extensions.DependencyInjection.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PackageId>Biss.Mapper.Extensions.DependencyInjection</PackageId>
    <Description>Dependency injection extensions for Biss Mapper</Description>
    <PackageTags>mapper;dependency-injection;extensions;dotnet9</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mapper.Abstractions\Biss.Mapper.Abstractions.csproj" />
    <ProjectReference Include="..\Biss.Mapper\Biss.Mapper.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>
</Project>
```

## Scripts de Build e Release

### build.ps1

```powershell
param(
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$SkipPack,
    [string]$Version = "1.0.0"
)

Write-Host "Building Biss Essentials v$Version" -ForegroundColor Green

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
dotnet clean --configuration $Configuration

# Restore
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if (-not $SkipTests) {
    # Test
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --verbosity normal
}

if (-not $SkipPack) {
    # Pack
    Write-Host "Packing..." -ForegroundColor Yellow
    dotnet pack --configuration $Configuration --no-build --output ./artifacts
}

Write-Host "Build completed successfully!" -ForegroundColor Green
```

### release.ps1

```powershell
param(
    [string]$Version = "1.0.0",
    [string]$ApiKey = "",
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

Write-Host "Releasing Biss Essentials v$Version" -ForegroundColor Green

# Build and pack
& .\build.ps1 -Configuration Release -SkipTests -Version $Version

# Push to NuGet
if ($ApiKey) {
    Write-Host "Pushing to NuGet..." -ForegroundColor Yellow
    Get-ChildItem -Path "./artifacts/*.nupkg" | ForEach-Object {
        dotnet nuget push $_.FullName --api-key $ApiKey --source $Source
    }
    Write-Host "Release completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Packages created in ./artifacts. Use 'dotnet nuget push' to publish." -ForegroundColor Yellow
}
```

## GitHub Actions Workflow

### .github/workflows/ci.yml

```yaml
name: CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
      
    - name: Pack
      run: dotnet pack --no-build --configuration Release --output ./artifacts
      
    - name: Upload artifacts
      uses: actions/upload-artifact@v4
      with:
        name: packages
        path: ./artifacts/*.nupkg
```

### .github/workflows/release.yml

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  release:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
      
    - name: Pack
      run: dotnet pack --no-build --configuration Release --output ./artifacts
      
    - name: Publish to NuGet
      run: dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

Esta estrutura de projetos fornece uma base sólida para implementar os componentes Mediator e Mapper com alta qualidade, seguindo as melhores práticas de desenvolvimento .NET e facilitando a manutenção e evolução dos componentes.
