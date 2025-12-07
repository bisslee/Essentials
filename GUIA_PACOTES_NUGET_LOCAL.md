# 📦 Pacotes NuGet Locais - Biss Essentials

## ✅ Pacotes Criados

Todos os componentes foram empacotados em `nuget-packages/`:

1. **Biss.Mediator.Abstractions.1.0.0.nupkg** (10 KB)
   - Interfaces e contratos do Mediator

2. **Biss.Mediator.1.0.0.nupkg** (12 KB)
   - Implementação do padrão Mediator

3. **Biss.Mediator.Extensions.DependencyInjection.1.0.0.nupkg** (6.6 KB)
   - Extensões de DI para Mediator

4. **Biss.Mediator.Behaviors.1.0.0.nupkg** (14 KB)
   - Pipeline behaviors (Logging, Validation, Performance, etc.)

5. **Biss.Mapper.Abstractions.1.0.0.nupkg** (6.4 KB)
   - Interfaces e contratos do Mapper

6. **Biss.Mapper.1.0.0.nupkg** (9.3 KB)
   - Implementação do Mapper

7. **Biss.Mapper.Extensions.DependencyInjection.1.0.0.nupkg** (6.5 KB)
   - Extensões de DI para Mapper

## 🚀 Como Usar os Pacotes Locais

### Opção 1: Adicionar Fonte Local no NuGet

```powershell
# Adicionar a pasta como fonte do NuGet
dotnet nuget add source "P:\proj\Libs\Biss.Essentials\nuget-packages" --name "Biss Local"

# Verificar se foi adicionado
dotnet nuget list source
```

### Opção 2: Usar Diretamente no Projeto

No `.csproj` do seu projeto (ex: KbSports.Core):

```xml
<ItemGroup>
  <PackageReference Include="Biss.Mediator.Abstractions" Version="1.0.0" />
  <PackageReference Include="Biss.Mediator" Version="1.0.0" />
  <PackageReference Include="Biss.Mediator.Extensions.DependencyInjection" Version="1.0.0" />
  <PackageReference Include="Biss.Mediator.Behaviors" Version="1.0.0" />
  
  <!-- Se usar Mapper também -->
  <PackageReference Include="Biss.Mapper.Abstractions" Version="1.0.0" />
  <PackageReference Include="Biss.Mapper" Version="1.0.0" />
  <PackageReference Include="Biss.Mapper.Extensions.DependencyInjection" Version="1.0.0" />
</ItemGroup>
```

Configurar o `NuGet.config` para usar a pasta local:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="Biss Local" value="P:\proj\Libs\Biss.Essentials\nuget-packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

### Opção 3: Restaurar Pacotes

```powershell
# Restaurar pacotes do projeto que usa Biss
cd "P:\proj\KbSports"
dotnet restore
```

## 📝 Usar no KbSports

### 1. Configurar NuGet Local

No projeto KbSports, criar `NuGet.config` na raiz:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="Biss Local" value="P:\proj\Libs\Biss.Essentials\nuget-packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

### 2. Adicionar Pacotes

```powershell
cd "P:\proj\KbSports\src\Back\Core\KbSports.Core"

# Adicionar pacotes Biss
dotnet add package Biss.Mediator.Abstractions --version 1.0.0 --source "P:\proj\Libs\Biss.Essentials\nuget-packages"
dotnet add package Biss.Mediator --version 1.0.0 --source "P:\proj\Libs\Biss.Essentials\nuget-packages"
dotnet add package Biss.Mediator.Extensions.DependencyInjection --version 1.0.0 --source "P:\proj\Libs\Biss.Essentials\nuget-packages"
dotnet add package Biss.Mediator.Behaviors --version 1.0.0 --source "P:\proj\Libs\Biss.Essentials\nuget-packages"
```

### 3. Verificar Instalação

```powershell
# Listar pacotes instalados
dotnet list package

# Restaurar dependências
dotnet restore
```

### 4. Atualizar Código

Seguir as instruções do `GUIA_INTEGRACAO_KbSports.md`:

```csharp
// Startup.cs ou Program.cs
using Biss.Mediator.Extensions.DependencyInjection;
using Biss.Mediator.Behaviors;

// Substituir MediatR
services.AddMediator(typeof(ApplicationAssemblyMarker).Assembly);
services.AddMediatorBehaviors(typeof(ApplicationAssemblyMarker).Assembly);
```

## 🔄 Atualizar Pacotes

Quando fizer alterações nos componentes:

```powershell
# Re-empacotar
cd "P:\proj\Libs\Biss.Essentials"

dotnet pack src/Biss.Mediator.Abstractions -c Release -o nuget-packages
dotnet pack src/Biss.Mediator -c Release -o nuget-packages
# ... repita para os outros pacotes

# Atualizar versão (modificar em Directory.Build.props)
# Version=1.0.0 → Version=1.0.1

# Limpar cache do NuGet
dotnet nuget locals all --clear

# Restaurar no projeto que usa
cd "P:\proj\KbSports"
dotnet restore
```

## 📊 Resumo dos Pacotes

| Pacote | Versão | Tamanho | Descrição |
|--------|--------|---------|-----------|
| Biss.Mediator.Abstractions | 1.0.0 | 10 KB | Interfaces e contratos |
| Biss.Mediator | 1.0.0 | 12 KB | Implementação do Mediator |
| Biss.Mediator.Extensions.DependencyInjection | 1.0.0 | 6.6 KB | Extensões DI |
| Biss.Mediator.Behaviors | 1.0.0 | 14 KB | Pipeline behaviors |
| Biss.Mapper.Abstractions | 1.0.0 | 6.4 KB | Interfaces do Mapper |
| Biss.Mapper | 1.0.0 | 9.3 KB | Implementação do Mapper |
| Biss.Mapper.Extensions.DependencyInjection | 1.0.0 | 6.5 KB | Extensões DI |

**Total**: ~65 KB (7 pacotes)

## 🎯 Próximos Passos

1. ✅ Pacotes criados em `nuget-packages/`
2. ⏳ Configurar fonte local no KbSports
3. ⏳ Instalar pacotes no projeto
4. ⏳ Seguir guia de integração
5. ⏳ Testar e validar

---

**Localização**: `P:\proj\Libs\Biss.Essentials\nuget-packages`
**Pronto para uso**: ✅ Sim
