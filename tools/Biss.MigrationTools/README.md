# Biss Migration Tools

Ferramentas para migração automática de AutoMapper e MediatR para os componentes Biss.

## 🚀 **Funcionalidades**

### **Análise de Código**
- ✅ Detecção automática de AutoMapper profiles e usages
- ✅ Detecção automática de MediatR handlers e usages
- ✅ Relatórios detalhados de análise
- ✅ Suporte a múltiplos arquivos e diretórios

### **Geração de Código**
- ✅ Conversão automática de AutoMapper profiles para Biss Mappers
- ✅ Conversão automática de MediatR handlers para Biss Handlers
- ✅ Geração de código com TODOs para implementação manual
- ✅ Preservação de namespaces e estrutura

## 📋 **Comandos Disponíveis**

### **1. Análise de Codebase**
```bash
dotnet run -- analyze --path "C:\MyProject\src" --output "analysis-report.json"
```

### **2. Migração AutoMapper**
```bash
dotnet run -- automapper --path "C:\MyProject\src" --output "C:\MyProject\migrated\mappers"
```

### **3. Migração MediatR**
```bash
dotnet run -- mediatr --path "C:\MyProject\src" --output "C:\MyProject\migrated\handlers"
```

## 🔍 **Exemplo de Uso**

### **Antes (AutoMapper)**
```csharp
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
    }
}
```

### **Depois (Biss Mapper)**
```csharp
[Mapper]
public partial class UserMapper
{
    public partial UserDto ToUserDto(User source)
    {
        if (source == null)
            return null!;

        return new UserDto
        {
            // TODO: Implement property mappings
            // This is a placeholder - actual mappings should be generated based on type analysis
        };
    }
}
```

### **Antes (MediatR)**
```csharp
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Unit>
{
    public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Implementation
        return Unit.Value;
    }
}
```

### **Depois (Biss Mediator)**
```csharp
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(ILogger<CreateUserHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling command {CommandType}", typeof(CreateUserCommand).Name);
        
        // TODO: Implement command handling logic
        await Task.CompletedTask;
        
        return Result<Unit>.Success(Unit.Value);
    }
}
```

## 📊 **Relatório de Análise**

O comando `analyze` gera um relatório JSON detalhado:

```json
{
  "AnalyzedAt": "2025-01-27T10:30:00Z",
  "Path": "C:\\MyProject\\src",
  "AutoMapper": {
    "TotalFiles": 15,
    "FilesWithReferences": 3,
    "TotalProfiles": 2,
    "TotalUsages": 8,
    "Profiles": [
      {
        "ClassName": "UserProfile",
        "Namespace": "MyProject.Mappings",
        "MappingCount": 2
      }
    ]
  },
  "MediatR": {
    "TotalFiles": 15,
    "FilesWithReferences": 5,
    "TotalHandlers": 3,
    "TotalNotificationHandlers": 1,
    "TotalUsages": 12,
    "Handlers": [
      {
        "ClassName": "CreateUserHandler",
        "Namespace": "MyProject.Handlers",
        "RequestType": "CreateUserCommand",
        "ResponseType": "Unit"
      }
    ]
  }
}
```

## ⚠️ **Limitações Atuais**

1. **Mapeamento de Propriedades**: O gerador cria TODOs para mapeamento manual
2. **Análise de Tipos**: Não analisa tipos complexos automaticamente
3. **Configurações Customizadas**: Não migra configurações específicas do AutoMapper
4. **Behaviors**: Não migra pipeline behaviors do MediatR

## 🔧 **Próximos Passos**

1. Implementar análise de tipos para mapeamento automático
2. Adicionar suporte a configurações customizadas
3. Migração de pipeline behaviors
4. Validação de código gerado
5. Testes automáticos para código migrado

## 🎯 **Como Usar**

1. **Execute a análise** para entender o escopo da migração
2. **Migre AutoMapper** primeiro (se aplicável)
3. **Migre MediatR** depois
4. **Revise o código gerado** e implemente os TODOs
5. **Teste** a funcionalidade migrada
6. **Remova** as dependências antigas

## 📝 **Notas Importantes**

- O código gerado contém TODOs que precisam ser implementados manualmente
- Revise sempre o código gerado antes de usar em produção
- Teste a funcionalidade migrada antes de remover o código antigo
- Mantenha backups do código original durante a migração
