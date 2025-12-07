# 📋 Como Validar a Migração dos Handlers

A ferramenta de validação verifica se os handlers migrados estão corretos e prontos para uso.

## 🚀 **Comandos de Validação**

### **1. Validar Handlers Migrados**
```bash
dotnet run --project tools/Biss.MigrationTools -- validate-handlers --path "C:\Migrado\Handlers"
```

**Com comparação com originais:**
```bash
dotnet run --project tools/Biss.MigrationTools -- validate-handlers --path "C:\Migrado\Handlers" --original "C:\Projeto\src"
```

### **2. Validar Mappers Migrados**
```bash
dotnet run --project tools/Biss.MigrationTools -- validate-mappers --path "C:\Migrado\Mappers"
```

**Com comparação com originais:**
```bash
dotnet run --project tools/Biss.MigrationTools -- validate-mappers --path "C:\Migrado\Mappers" --original "C:\Projeto\src"
```

## ✅ **O que a ferramenta valida:**

### **Para Handlers:**
- ✅ Presença de `using Biss.Mediator.Abstractions`
- ✅ Implementação das interfaces (`ICommandHandler`, `IQueryHandler`, etc.)
- ✅ Presença do método `Handle()`
- ✅ Dependências necessárias (`ILogger`)
- ✅ Sintaxe básica (chaves balanceadas)
- ✅ Presença de TODOs (warnings)

### **Para Mappers:**
- ✅ Presença do atributo `[Mapper]`
- ✅ Presença de métodos de mapeamento
- ✅ Presença de TODOs (warnings)
- ✅ Sintaxe básica

## 📊 **Exemplo de Saída**

```
🔍 Validating handlers in: C:\Migrado\Handlers
📊 Found 96 handler files
✅ Validation completed: 96 files, 0 errors, 15 warnings

=== MIGRATION VALIDATION REPORT ===
Started: 2025-01-27 22:30:00
Completed: 2025-01-27 22:30:15
Duration: 15s

Status: ✅ VALID
Total Files: 96
Total Errors: 0
Total Warnings: 15

=== WARNINGS ===
⚠️ Handler is missing ILogger dependency
⚠️ Contains 2 TODO comments that need implementation
⚠️ Handler is missing ILogger dependency
...

✅ Validation passed!
```

## 🔧 **Integração com Testes**

Se você tem handlers de teste, pode criar testes específicos:

```bash
# No diretório do seu projeto
dotnet test tests/Biss.Mediator.Tests
```

Ou criar testes para código migrado:

```csharp
// Test para handler migrado
public class MigratedHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsResult()
    {
        // Arrange
        var handler = new MigratedHandler(Mock.Of<ILogger<MigratedHandler>>());
        var command = new MigratedCommand();

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
```

## 📝 **Checklist de Validação Manual**

Além da validação automática, revise:

- [ ] Implementar TODOs nos handlers
- [ ] Adicionar testes unitários
- [ ] Verificar configuração de DI
- [ ] Testar comportamento em cenários reais
- [ ] Comparar comportamento com o código antigo
- [ ] Validar logging
- [ ] Verificar tratamento de erros

## 🚀 **Próximo Passo**

Após validação bem-sucedida:

1. **Testar no ambiente local**
2. **Implementar TODOs restantes**
3. **Rodar testes completos**
4. **Comparar performance**
5. **Migrar em produção**
