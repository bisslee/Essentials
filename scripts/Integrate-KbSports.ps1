# Script de Integração - Biss Essentials no KbSports
# Executar como Administrador

param(
    [string]$KbSportsPath = "P:\proj\KbSports",
    [string]$BissEssentialsPath = "P:\proj\Libs\Biss.Essentials",
    [string]$HandlersMigratedPath = "C:\Migrado\Handlers",
    [switch]$DryRun
)

Write-Host "🚀 Iniciando integração do Biss Essentials no projeto KbSports..." -ForegroundColor Cyan
Write-Host ""

# Verificar caminhos
if (-not (Test-Path $KbSportsPath)) {
    Write-Host "❌ Caminho do KbSports não encontrado: $KbSportsPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $BissEssentialsPath)) {
    Write-Host "❌ Caminho do Biss.Essentials não encontrado: $BissEssentialsPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $HandlersMigratedPath)) {
    Write-Host "❌ Caminho dos handlers migrados não encontrado: $HandlersMigratedPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Caminhos validados" -ForegroundColor Green
Write-Host ""

# Criar backup do projeto
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "$KbSportsPath\_backup_$timestamp"

if ($DryRun) {
    Write-Host "🔍 DRY RUN: Seria criado backup em: $backupPath" -ForegroundColor Yellow
} else {
    Write-Host "📦 Criando backup do projeto..." -ForegroundColor Cyan
    Copy-Item -Path $KbSportsPath -Destination $backupPath -Recurse -Force
    Write-Host "✅ Backup criado em: $backupPath" -ForegroundColor Green
}

Write-Host ""

# Construir pacotes Biss em Release
Write-Host "🔨 Construindo pacotes Biss em Release..." -ForegroundColor Cyan

$projectsToBuild = @(
    "src\Biss.Mediator.Abstractions",
    "src\Biss.Mediator",
    "src\Biss.Mediator.Extensions.DependencyInjection",
    "src\Biss.Mediator.Behaviors",
    "src\Biss.Mapper.Abstractions",
    "src\Biss.Mapper",
    "src\Biss.Mapper.Extensions.DependencyInjection"
)

foreach ($project in $projectsToBuild) {
    $projectPath = Join-Path $BissEssentialsPath "$project\*.csproj"
    $projectFiles = Get-ChildItem $projectPath
    
    foreach ($projFile in $projectFiles) {
        Write-Host "  Construindo: $($projFile.Name)..." -ForegroundColor Gray
        
        if (-not $DryRun) {
            dotnet build $projFile.FullName -c Release | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✅ $($projFile.Name) construído com sucesso" -ForegroundColor Green
            } else {
                Write-Host "  ❌ Erro ao construir $($projFile.Name)" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""

# Buscar arquivo de configuração do KbSports (Startup.cs ou Program.cs)
Write-Host "🔍 Procurando arquivo de configuração do KbSports..." -ForegroundColor Cyan

$configFiles = Get-ChildItem -Path $KbSportsPath -Recurse -Include "Startup.cs","Program.cs","*.csproj" | 
    Where-Object { $_.FullName -notmatch "bin|obj" }

Write-Host "Encontrados $($configFiles.Count) arquivos de configuração" -ForegroundColor Gray
Write-Host ""

# Criar relatório de integração
$reportPath = Join-Path $BissEssentialsPath "RELATORIO_INTEGRACAO.md"

$report = @"
# Relatório de Integração - Biss Essentials no KbSports

Gerado em: $(Get-Date -Format "dd/MM/yyyy HH:mm:ss")

## Configuração
- KbSports Path: $KbSportsPath
- Biss Essentials Path: $BissEssentialsPath
- Handlers Migrados: $HandlersMigratedPath
- Backup Criado: $(if ($DryRun) {"N/A (Dry Run)"} else {$backupPath})

## Projetos Biss Construídos
$(if ($DryRun) {"(Dry Run - nenhum projeto construído)"} else {
    $projectsToBuild -join "`n"
})

## Arquivos de Configuração Encontrados
$($configFiles | ForEach-Object { "- $($_.FullName)" } | Out-String)

## Próximos Passos

1. **Adicionar Referências de Projeto** no KbSports.Core.csproj:
   ````xml
   <ItemGroup>
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator\Biss.Mediator.csproj" />
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator.Extensions.DependencyInjection\Biss.Mediator.Extensions.DependencyInjection.csproj" />
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator.Behaviors\Biss.Mediator.Behaviors.csproj" />
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mapper.Abstractions\Biss.Mapper.Abstractions.csproj" />
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mapper\Biss.Mapper.csproj" />
     <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mapper.Extensions.DependencyInjection\Biss.Mapper.Extensions.DependencyInjection.csproj" />
   </ItemGroup>
   ````

2. **Atualizar DI** no Startup.cs/Program.cs:
   ````csharp
   using Biss.Mediator.Extensions.DependencyInjection;
   
   // Substituir:
   // services.AddMediatR(...)
   // Por:
   services.AddMediator(typeof(YourAssembly).Assembly);
   ````

3. **Copiar Handlers Migrados**:
   ````powershell
   Copy-Item "$HandlersMigratedPath\*" -Destination "$KbSportsPath\src\...\Handlers\" -Force
   ````

4. **Atualizar Imports**: Substituir todas as referências a `MediatR` por `Biss.Mediator.Abstractions`

5. **Testar**: Executar testes unitários e de integração

## Status da Integração
- [ ] Referências adicionadas
- [ ] DI atualizado
- [ ] Handlers copiados
- [ ] Imports atualizados
- [ ] Testes passando
- [ ] Performance validada

---
**Gerado por**: Script de Integração Biss Essentials
**Versão**: 1.0
"@

if (-not $DryRun) {
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "📄 Relatório criado em: $reportPath" -ForegroundColor Green
} else {
    Write-Host "📄 Relatório seria criado em: $reportPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Integração preparada com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
Write-Host "  1. Revisar o relatório em: $reportPath"
Write-Host "  2. Seguir as instruções do GUIA_INTEGRACAO_KbSports.md"
Write-Host "  3. Testar em ambiente de desenvolvimento"
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  Este foi um DRY RUN - nenhuma alteração foi feita" -ForegroundColor Yellow
}
