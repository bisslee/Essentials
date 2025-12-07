# Guia de Integração - Biss Essentials no KbSports

## 📋 Pré-requisitos

1. Ter a migração dos handlers concluída (96 handlers gerados em `C:\Migrado\Handlers`)
2. Ter acesso ao projeto KbSports em `P:\proj\KbSports`
3. Backups atualizados do projeto

## 🚀 Passo 1: Preparar Pacotes Locais Biss

### 1.1 Build dos Pacotes em Release

```powershell
# Na pasta do Biss.Essentials
dotnet build -c Release src/Biss.Mediator.Abstractions/Biss.Mediator.Abstractions.csproj
dotnet build -c Release src/Biss.Mediator/Biss.Mediator.csproj
dotnet build -c Release src/Biss.Mediator.Extensions.DependencyInjection/Biss.Mediator.Extensions.DependencyInjection.csproj
dotnet build -c Release src/Biss.Mediator.Behaviors/Biss.Mediator.Behaviors.csproj
dotnet build -c Release src/Biss.Mapper.Abstractions/Biss.Mapper.Abstractions.csproj
dotnet build -c Release src/Biss.Mapper/Biss.Mapper.csproj
dotnet build -c Release src/Biss.Mapper.Extensions.DependencyInjection/Biss.Mapper.Extensions.DependencyInjection.csproj
```

### 1.2 Publicar Pacotes Localmente (Opcional)

```powershell
# Criar pasta para pacotes locais
New-Item -ItemType Directory -Force -Path "$HOME\.nuget\local"

# Publicar pacotes
dotnet pack src/Biss.Mediator.Abstractions/Biss.Mediator.Abstractions.csproj -c Release -o "$HOME\.nuget\local"
dotnet pack src/Biss.Mediator/Biss.Mediator.csproj -c Release -o "$HOME\.nuget\local"
dotnet pack src/Biss.Mediator.Extensions.DependencyInjection/Biss.Mediator.Extensions.DependencyInjection.csproj -c Release -o "$HOME\.nuget\local"
dotnet pack src/Biss.Mediator.Behaviors/Biss.Mediator.Behaviors.csproj -c Release -o "$HOME\.nuget\local"
dotnet pack src/Biss.Mapper.Abstractions/Biss.Mapper.Abstractions.csproj -c Release -o "$HOME\.nuget\local"
dotnet pack src/Biss.Mapper/Biss.Mapper.csproj -c Release -o "$HOME\.nuget\local"
dotnet pack src/Biss.Mapper.Extensions.DependencyInjection/Biss.Mapper.Extensions.DependencyInjection.csproj -c Release -o "$HOME\.nuget\local"
```

## 🏗️ Passo 2: Configurar Referências no KbSports

### 2.1 Adicionar Referências de Projeto (Recomendado para Testes)

No projeto KbSports.Core (ou onde os handlers estão):

```xml
<!-- No .csproj do KbSports.Core -->
<ItemGroup>
  <!-- Biss Mediator -->
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator\Biss.Mediator.csproj" />
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator.Extensions.DependencyInjection\Biss.Mediator.Extensions.DependencyInjection.csproj" />
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mediator.Behaviors\Biss.Mediator.Behaviors.csproj" />
  
  <!-- Biss Mapper -->
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mapper.Abstractions\Biss.Mapper.Abstractions.csproj" />
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mapper\Biss.Mapper.csproj" />
  <ProjectReference Include="..\..\..\Libs\Biss.Essentials\src\Biss.Mapper.Extensions.DependencyInjection\Biss.Mapper.Extensions.DependencyInjection.csproj" />
</ItemGroup>
```

## 🔄 Passo 3: Substituir MediatR pelo Biss

### 3.1 Copiar Handlers Migrados

Copiar os handlers migrados para o projeto KbSports:

```powershell
# Substituir handlers existentes (fazer backup antes!)
Copy-Item "C:\Migrado\Handlers\*" -Destination "P:\proj\KbSports\src\Back\Core\KbSports.Core\Application\Handlers\" -Force
```

### 3.2 Atualizar DI no Startup/Program.cs

**ANTES (MediatR):**
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
```

**DEPOIS (Biss):**
```csharp
using Biss.Mediator.Extensions.DependencyInjection;
using Biss.Mediator.Behaviors;
using KbSports.Core.Application;

// Registrar Biss Mediator
services.AddMediator(typeof(ApplicationAssemblyMarker).Assembly);

// Registrar Behaviors (opcional)
services.AddMediatorBehaviors(typeof(ApplicationAssemblyMarker).Assembly);
```

### 3.3 Atualizar Injeções de Dependência

Nos controllers/services que usam `IMediator` do MediatR:

```csharp
using Biss.Mediator.Abstractions; // Nova importação
// ... código existente
```

## 🧪 Passo 4: Testes Graduais

### 4.1 Teste Unitário por Handler

Criar testes unitários para cada handler migrado:

```csharp
[Fact]
public async Task Handle_CreateUserCommand_ReturnsSuccess()
{
    // Arrange
    var command = new CreateUserCommand("John", "Doe", "john@example.com");
    var handler = new CreateUserHandler(_mockRepository, _mockLogger);
    
    // Act
    var result = await handler.Handle(command);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(Unit.Value, result.Value);
}
```

### 4.2 Teste de Integração

```csharp
[Fact]
public async Task Mediator_Should_Handle_Command()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddMediator(typeof(ApplicationAssemblyMarker).Assembly);
    services.AddSingleton<IMediator, Mediator>();
    
    var serviceProvider = services.BuildServiceProvider();
    var mediator = serviceProvider.GetRequiredService<IMediator>();
    
    var command = new CreateUserCommand("Test", "User", "test@example.com");
    
    // Act
    var result = await mediator.Send(command);
    
    // Assert
    Assert.True(result.IsSuccess);
}
```

## 🔍 Passo 5: Validação e Ajustes

### 5.1 Verificar Logs

Monitorar logs durante a execução para garantir que os handlers estão sendo chamados corretamente:

```csharp
// Adicionar logging detalhado
services.AddLogging(builder => 
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

### 5.2 Verificar Pipeline Behaviors

Testar se os behaviors estão funcionando corretamente:

```csharp
// Adicionar validação no handler
public async Task<Result<UserDto>> Handle(GetUserQuery request)
{
    // Validação é feita automaticamente pelo ValidationBehavior
    // ... código do handler
}
```

## 📊 Passo 6: Benchmark de Performance

Comparar performance antes e depois:

```powershell
# Executar benchmarks
dotnet run --configuration Release --project tools/Biss.PerformanceBenchmarks
```

## ⚠️ Possíveis Problemas e Soluções

### Problema 1: Handlers não estão sendo registrados

**Solução:**
- Verificar se os handlers implementam as interfaces corretas do Biss
- Verificar se o `AddMediator` está configurado corretamente
- Verificar se há erros de build

### Problema 2: Falta referência ao Biss

**Solução:**
```powershell
dotnet add package Biss.Mediator.Abstractions --source "$HOME\.nuget\local"
dotnet add package Biss.Mediator --source "$HOME\.nuget\local"
# ... adicionar outros pacotes necessários
```

### Problema 3: Erro de compilação

**Solução:**
- Verificar se todos os `using` foram atualizados
- Verificar se há conflitos de namespace
- Limpar e reconstruir a solução:
  ```powershell
  dotnet clean
  dotnet build
  ```

## ✅ Checklist de Integração

- [ ] Pacotes Biss construídos em Release
- [ ] Referências adicionadas ao projeto KbSports
- [ ] Handlers migrados copiados
- [ ] DI atualizado no Startup/Program.cs
- [ ] Imports atualizados (MediatR → Biss)
- [ ] Testes unitários ajustados
- [ ] Testes de integração funcionando
- [ ] Logs de debug adicionados
- [ ] Benchmarks executados
- [ ] Documentação atualizada

## 📞 Próximos Passos

Após validação bem-sucedida:

1. Implementar Source Generators para otimização
2. Criar documentação de uso para a equipe
3. Preparar pacotes NuGet para publicação
4. Treinar equipe no uso do Biss

## 🎯 Objetivo Final

Migrar completamente do MediatR/AutoMapper para Biss mantendo:
- ✅ Funcionalidade completa
- ✅ Performance superior (6-10x mais rápido)
- ✅ Compatibilidade AOT
- ✅ Facilidade de uso
